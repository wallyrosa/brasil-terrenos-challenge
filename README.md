# Transferências Financeiras — Teste Técnico Brasil Terrenos

API REST em .NET 10 + interface em React para realizar, agendar, consultar e cancelar transferências entre contas, com cheque especial, limites por hora (dia/noite), controle de concorrência e idempotência.

## Stack

| Camada | Tecnologia |
| --- | --- |
| Backend | .NET 10, ASP.NET Core (Controllers), EF Core 10, Npgsql |
| Banco | PostgreSQL 17 |
| Documentação | Swagger / OpenAPI (Swashbuckle) |
| Agendamentos | `BackgroundService` com `FOR UPDATE SKIP LOCKED` |
| Testes | xUnit, FluentAssertions, Testcontainers (PostgreSQL real), `WebApplicationFactory` |
| Frontend | React 18 + TypeScript + Vite, React Router, TanStack Query |
| Infra | Docker / Docker Compose (db, api, web) |

## Como executar

### Opção 1 — Docker Compose (recomendado)

Pré-requisito: Docker em execução.

```bash
docker compose up --build
```

| Serviço | URL |
| --- | --- |
| Frontend | http://localhost:3000 |
| API / Swagger | http://localhost:5080/swagger |
| PostgreSQL | localhost:5432 (postgres / postgres) |

Se a porta 5432 já estiver em uso, defina outra porta para o banco com a variável `DB_PORT` (por exemplo, `DB_PORT=5433 docker compose up --build`).

As migrations são aplicadas automaticamente na inicialização da API e três contas de demonstração são criadas (Ana: saldo R$ 500 + cheque especial R$ 1.000; Bruno: R$ 2.000 + R$ 500; Carla: R$ 10.000 sem cheque especial).

### Opção 2 — Local

Pré-requisitos: .NET SDK 10, Node 20+ e um PostgreSQL acessível (a connection string padrão está em `backend/src/Transfers.Api/appsettings.json`).

```bash
docker compose up -d db
```

```bash
dotnet run --project backend/src/Transfers.Api
```

```bash
cd frontend && npm install && npm run dev
```

API em http://localhost:5080/swagger e frontend em http://localhost:5173 (o Vite faz proxy de `/api` para a API).

### Migrations

As migrations ficam em `backend/src/Transfers.Infrastructure/Persistence/Migrations`. Para gerar uma nova:

```bash
cd backend && dotnet tool restore && dotnet ef migrations add NomeDaMigration -p src/Transfers.Infrastructure -s src/Transfers.Api -o Persistence/Migrations
```

### Testes

```bash
cd backend && dotnet test
```

Os testes de integração sobem um PostgreSQL real via Testcontainers, portanto exigem Docker em execução.

## Endpoints

| Método | Rota | Descrição |
| --- | --- | --- |
| POST | `/api/transfers` | Realiza transferência imediata (header opcional `Idempotency-Key`) |
| POST | `/api/transfers/scheduled` | Agenda transferência (header opcional `Idempotency-Key`) |
| POST | `/api/transfers/{id}/cancel` | Cancela transferência agendada |
| GET | `/api/transfers/{id}` | Consulta transferência |
| GET | `/api/transfers?accountId={id}` | Lista transferências de uma conta |
| GET | `/api/accounts` | Lista contas |
| GET | `/api/accounts/{id}` | Consulta conta |
| POST | `/api/accounts` | Abre conta |
| PUT | `/api/accounts/{id}/overdraft-limit` | Altera limite de cheque especial |
| PUT | `/api/accounts/{id}/transfer-limits` | Altera limites por hora (dia/noite) |
| POST | `/api/accounts/{id}/activate` \| `block` \| `deactivate` | Altera status da conta |

Respostas de erro seguem o padrão `ProblemDetails` (RFC 9457) com um campo `code` estável:

| Status | Quando |
| --- | --- |
| 400 | Requisição malformada / campos obrigatórios ausentes |
| 404 | Conta ou transferência inexistente |
| 409 | Chave de idempotência reutilizada com outro payload |
| 422 | Regra de negócio violada. Para transferências imediatas rejeitadas, a extensão `transfer` traz a transferência registrada com status `Failed` e o `failureReason` |

## Arquitetura

Clean Architecture com DDD tático, em quatro projetos com dependências apontando para o domínio:

```
backend/
├── src/
│   ├── Transfers.Domain          Entidades, value objects, domain service, eventos e erros de domínio
│   ├── Transfers.Application     Casos de uso (um handler por caso), portas (repositórios, UoW), regras de período
│   ├── Transfers.Infrastructure  EF Core/PostgreSQL, repositórios, Unit of Work, dispatcher de eventos, worker
│   └── Transfers.Api             Controllers, contratos HTTP, tratamento global de erros, Swagger
└── tests/
    ├── Transfers.UnitTests       Regras de domínio e resolução de período dia/noite
    └── Transfers.IntegrationTests Cenários ponta a ponta contra PostgreSQL real
frontend/
└── src/
    ├── app                       Rotas, providers e layout
    ├── shared                    Cliente HTTP, formatadores e componentes de UI
    └── features                  accounts e transfers (api, hooks, tipos, componentes e páginas)
```

### Modelagem de domínio

- **`Account` (agregado)**: saldo, limite de cheque especial, status (`Active`, `Blocked`, `Inactive`) e `TransferLimits`. Garante as invariantes de débito e crédito: só contas ativas movimentam, e o débito respeita `saldo + cheque especial`. O crédito em uma conta negativa abate primeiro o cheque especial usado, porque o saldo é um único valor com sinal (−800 + 1.000 = 200). `UsedOverdraft`, `AvailableOverdraft` e `AvailableToTransfer` são derivados, nunca persistidos.
- **`Transfer` (agregado)**: máquina de estados explícita `Scheduled → Processing → Completed | Failed` e `Scheduled → Cancelled`. Transições inválidas lançam `DomainException`. `Complete` e `Fail` são `internal` e só podem ser chamados pelo domain service.
- **`TransferLimits` / `HourlyLimit` (value objects)**: limites por pessoa (cada pessoa possui uma conta), separados por período. São mapeados como *complex types* do EF Core, em colunas da própria tabela `accounts`.
- **`TransferExecutionService` (domain service)**: orquestra a regra que envolve os dois agregados. Valida o status das contas, as tentativas por hora, o valor por hora e o saldo, nessa ordem. Então debita e credita ou marca a transferência como `Failed` com o motivo.
- **Domain Events**: `TransferScheduled`, `TransferCompleted`, `TransferFailed` e `TransferCancelled`. São coletados pelo Unit of Work e despachados **após o commit**. Hoje alimentam um handler de auditoria em log, e são o ponto de extensão natural para outbox/mensageria.

### Regras de negócio e decisões

- **Transferência rejeitada conta como tentativa.** Rejeições por regra de negócio (saldo, limites, conta inativa) são persistidas como `Failed` e entram na contagem de tentativas da hora. Erros de entrada (valor ≤ 0, origem = destino, conta inexistente) são rejeitados antes de virar tentativa, porque não chegam a ser uma transferência válida.
- **Janela de limite deslizante de 60 minutos** por conta de origem. O limite aplicado é o do período (dia/noite) do momento da execução. Tentativas contam as transferências `Completed` e `Failed` processadas na janela, e o valor conta só as `Completed`.
- **Dia/noite** é configurável em `TransferPeriods` (padrão: dia de 06:00 às 22:00 no fuso `America/Sao_Paulo`). Os limites padrão de novas contas ficam em `DefaultTransferLimits`, e cada conta pode ter os seus via API.
- **Agendamento**: a data deve ser futura e as contas devem estar ativas no momento do agendamento. Na execução, **saldo, cheque especial, status e limites são verificados novamente** pelo mesmo fluxo da transferência imediata.
- **Valores monetários** usam `decimal` com no máximo 2 casas (`numeric(18,2)`). Datas são armazenadas em UTC (`timestamptz`).

### Concorrência

- **Lock pessimista por conta**: toda operação que movimenta saldo abre uma transação `READ COMMITTED` e trava as linhas das contas com `SELECT ... FOR UPDATE`, **sempre em ordem crescente de id**. Isso serializa operações concorrentes sobre o mesmo saldo e evita deadlocks em transferências cruzadas (A→B e B→A ao mesmo tempo).
- O cálculo de uso da hora acontece **depois** do lock, portanto também é serializado. Duas requisições simultâneas não conseguem ultrapassar juntas o limite de valor ou de tentativas.
- **Agendamentos**: o worker seleciona a próxima transferência vencida com `FOR UPDATE SKIP LOCKED`. Várias instâncias da API podem rodar em paralelo sem processar o mesmo item. O cancelamento trava a transferência com `FOR UPDATE`, então cancelamento e execução nunca se sobrepõem: quem chegar depois encontra o status final e recebe 422.
- Os testes de integração cobrem 10 transferências simultâneas disputando o mesmo saldo (exatamente 5 concluem e o saldo final é 0) e 20 transferências cruzadas simultâneas sem deadlock.

### Idempotência

O header `Idempotency-Key` é opcional e é armazenado com índice único. A verificação ocorre dentro do lock da conta de origem. Assim, requisições repetidas, inclusive simultâneas, devolvem a mesma transferência sem novo débito. A mesma chave com payload diferente retorna 409.

### Processamento de agendamentos

`ScheduledTransfersWorker` (`BackgroundService`) roda a cada `ScheduledTransfers:PollingInterval` (padrão 5s). Cada transferência é processada em escopo e transação próprios, então uma falha isolada não afeta as demais. Escolhi um worker com polling no banco em vez de um broker de mensagens: o próprio PostgreSQL já oferece fila confiável com `SKIP LOCKED`, sem acrescentar infraestrutura. Os domain events despachados após o commit são o ponto de integração caso se queira publicar em um broker (idealmente via outbox).

### Por que não MediatR/AutoMapper?

Cada caso de uso é uma classe pequena injetada diretamente no controller, e o mapeamento é feito por métodos `From` explícitos. Para o tamanho do problema isso mantém o fluxo legível e rastreável, sem bibliotecas extras.

## Cenários cobertos por testes

| Cenário | Onde |
| --- | --- |
| Transferência com saldo suficiente | `ImmediateTransferTests`, `TransferExecutionServiceTests` |
| Transferência usando cheque especial | `ImmediateTransferTests`, `AccountTests` |
| Transferência que ultrapassa o cheque especial | `ImmediateTransferTests`, `TransferExecutionServiceTests` |
| Recebimento enquanto usa cheque especial | `ImmediateTransferTests`, `AccountTests` |
| Transferência acima do limite da hora | `TransferLimitTests` |
| Excesso de tentativas (incluindo rejeitadas) | `TransferLimitTests` |
| Transferência no período noturno | `TransferLimitTests`, `TransferPeriodResolverTests` |
| Agendada sem saldo no momento da execução | `ScheduledTransferTests` |
| Duas transferências simultâneas com o mesmo saldo | `ConcurrencyAndIdempotencyTests` |
| Cancelamento, idempotência, contas bloqueadas, validações | Diversos |
