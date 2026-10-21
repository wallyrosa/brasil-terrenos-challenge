import { useState } from 'react'
import { useAccounts } from '@/features/accounts/hooks'
import { accountOptionLabel } from '@/features/accounts/labels'
import { Card, EmptyState, Field, PageHeader, QueryBoundary, Select } from '@/shared/ui'
import { AccountTransfersHistory } from '../components/AccountTransfersHistory'
import { TransferLookup } from '../components/TransferLookup'

export function TransfersPage() {
  const [accountId, setAccountId] = useState('')
  const { data: accounts, isLoading, error } = useAccounts()

  return (
    <>
      <PageHeader title="Transferências" subtitle="Consulte e cancele transferências agendadas." />
      <TransferLookup />
      <Card title="Transferências por conta">
        <QueryBoundary isLoading={isLoading} error={error}>
          <Field label="Conta">
            <Select value={accountId} onChange={(event) => setAccountId(event.target.value)}>
              <option value="">Selecione uma conta...</option>
              {accounts?.map((account) => (
                <option key={account.id} value={account.id}>
                  {accountOptionLabel(account)}
                </option>
              ))}
            </Select>
          </Field>
        </QueryBoundary>
      </Card>
      {accountId ? (
        <AccountTransfersHistory accountId={accountId} title="Resultado" />
      ) : (
        <EmptyState title="Selecione uma conta para ver as transferências" />
      )}
    </>
  )
}
