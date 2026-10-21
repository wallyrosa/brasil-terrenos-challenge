import { accountOptionLabel } from '@/features/accounts/labels'
import type { Account } from '@/features/accounts/types'
import { Alert, Button, Card, DateTimeInput, Field, Input, Select } from '@/shared/ui'
import { useTransferForm } from '../useTransferForm'
import { TransferResultAlert } from './TransferResultAlert'

interface TransferFormProps {
  accounts: Account[]
  initialSourceAccountId: string
}

export function TransferForm({ accounts, initialSourceAccountId }: TransferFormProps) {
  const { values, errors, result, submitError, isSubmitting, setValue, submit } =
    useTransferForm(initialSourceAccountId)

  return (
    <Card>
      <form className="form" onSubmit={submit} noValidate>
        <div className="form__grid">
          <Field label="Conta de origem" error={errors.sourceAccountId}>
            <Select
              value={values.sourceAccountId}
              onChange={(event) => setValue('sourceAccountId', event.target.value)}
            >
              <option value="">Selecione...</option>
              {accounts.map((account) => (
                <option key={account.id} value={account.id}>
                  {accountOptionLabel(account)}
                </option>
              ))}
            </Select>
          </Field>
          <Field label="Conta de destino" error={errors.destinationAccountId}>
            <Select
              value={values.destinationAccountId}
              onChange={(event) => setValue('destinationAccountId', event.target.value)}
            >
              <option value="">Selecione...</option>
              {accounts.map((account) => (
                <option key={account.id} value={account.id}>
                  {accountOptionLabel(account)}
                </option>
              ))}
            </Select>
          </Field>
          <Field label="Valor (R$)" error={errors.amount}>
            <Input
              type="number"
              min="0"
              step="0.01"
              value={values.amount}
              onChange={(event) => setValue('amount', event.target.value)}
              placeholder="0,00"
            />
          </Field>
        </div>

        <label className="checkbox">
          <input
            type="checkbox"
            checked={values.isScheduled}
            onChange={(event) => setValue('isScheduled', event.target.checked)}
          />
          Agendar
        </label>

        {values.isScheduled && (
          <div className="form__grid">
            <Field
              label="Data e hora do agendamento"
              hint="Horário de Brasília"
              error={errors.scheduledFor}
            >
              <DateTimeInput
                value={values.scheduledFor}
                onChange={(value) => setValue('scheduledFor', value)}
              />
            </Field>
          </div>
        )}

        {submitError && (
          <Alert tone="danger" title="Não foi possível transferir">
            {submitError}
          </Alert>
        )}
        {result && <TransferResultAlert transfer={result} />}

        <div className="form__actions">
          <Button type="submit" isLoading={isSubmitting}>
            {values.isScheduled ? 'Agendar transferência' : 'Transferir'}
          </Button>
        </div>
      </form>
    </Card>
  )
}
