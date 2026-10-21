import { Link, useParams } from 'react-router-dom'
import { formatCurrency, formatDateTime, formatDocument } from '@/shared/lib/format'
import { Card, DataList, PageHeader, QueryBoundary } from '@/shared/ui'
import { AccountTransfersHistory } from '@/features/transfers/components/AccountTransfersHistory'
import { AccountStatusActions } from '../components/AccountStatusActions'
import { AccountStatusBadge } from '../components/AccountStatusBadge'
import { OverdraftLimitForm } from '../components/OverdraftLimitForm'
import { TransferLimitsCard } from '../components/TransferLimitsCard'
import { useAccount } from '../hooks'

export function AccountDetailPage() {
  const { id = '' } = useParams()
  const { data: account, isLoading, error } = useAccount(id)

  return (
    <>
      <Link to="/contas" className="back-link">
        Voltar para contas
      </Link>
      <QueryBoundary isLoading={isLoading} error={error}>
        {account && (
          <>
            <PageHeader
              title={account.holderName}
              subtitle={formatDocument(account.document)}
              actions={
                <>
                  <AccountStatusBadge status={account.status} />
                  <Link
                    to={`/transferencias/nova?origem=${account.id}`}
                    className="btn btn--primary"
                  >
                    Nova transferência
                  </Link>
                </>
              }
            />
            <Card title="Dados da conta">
              <DataList
                items={[
                  { label: 'Saldo', value: formatCurrency(account.balance) },
                  { label: 'Limite de cheque especial', value: formatCurrency(account.overdraftLimit) },
                  { label: 'Cheque especial utilizado', value: formatCurrency(account.usedOverdraft) },
                  {
                    label: 'Cheque especial disponível',
                    value: formatCurrency(account.availableOverdraft),
                  },
                  {
                    label: 'Disponível para transferir',
                    value: formatCurrency(account.availableToTransfer),
                  },
                  { label: 'Aberta em', value: formatDateTime(account.createdAt) },
                ]}
              />
            </Card>
            <TransferLimitsCard account={account} />
            <div className="split">
              <OverdraftLimitForm account={account} />
              <AccountStatusActions account={account} />
            </div>
            <AccountTransfersHistory accountId={account.id} />
          </>
        )}
      </QueryBoundary>
    </>
  )
}
