import { useState } from 'react'
import { Button, EmptyState, PageHeader, QueryBoundary } from '@/shared/ui'
import { AccountCard } from '../components/AccountCard'
import { NewAccountForm } from '../components/NewAccountForm'
import { useAccounts } from '../hooks'

export function AccountsPage() {
  const [isCreating, setIsCreating] = useState(false)
  const { data: accounts, isLoading, error } = useAccounts()

  return (
    <>
      <PageHeader
        title="Contas"
        subtitle="Gerencie as contas e acompanhe saldos e limites."
        actions={
          !isCreating && <Button onClick={() => setIsCreating(true)}>Nova conta</Button>
        }
      />
      {isCreating && (
        <NewAccountForm
          onCreated={() => setIsCreating(false)}
          onCancel={() => setIsCreating(false)}
        />
      )}
      <QueryBoundary isLoading={isLoading} error={error}>
        {accounts && accounts.length > 0 ? (
          <div className="account-grid">
            {accounts.map((account) => (
              <AccountCard key={account.id} account={account} />
            ))}
          </div>
        ) : (
          <EmptyState
            title="Nenhuma conta cadastrada"
            description="Abra uma nova conta para começar."
          />
        )}
      </QueryBoundary>
    </>
  )
}
