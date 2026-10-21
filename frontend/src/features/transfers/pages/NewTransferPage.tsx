import { useSearchParams } from 'react-router-dom'
import { useAccounts } from '@/features/accounts/hooks'
import { PageHeader, QueryBoundary } from '@/shared/ui'
import { TransferForm } from '../components/TransferForm'

export function NewTransferPage() {
  const [searchParams] = useSearchParams()
  const { data: accounts, isLoading, error } = useAccounts()

  return (
    <>
      <PageHeader
        title="Nova transferência"
        subtitle="Transfira valores entre contas imediatamente ou agende para depois."
      />
      <QueryBoundary isLoading={isLoading} error={error}>
        <TransferForm
          accounts={accounts ?? []}
          initialSourceAccountId={searchParams.get('origem') ?? ''}
        />
      </QueryBoundary>
    </>
  )
}
