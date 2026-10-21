import { useAccountNames } from '@/features/accounts/hooks'
import { getErrorMessage } from '@/shared/api/http'
import { Alert, EmptyState } from '@/shared/ui'
import { useCancelTransfer } from '../hooks'
import type { Transfer } from '../types'
import { TransferTable } from './TransferTable'

interface TransferListProps {
  transfers: Transfer[]
  emptyTitle?: string
}

export function TransferList({
  transfers,
  emptyTitle = 'Nenhuma transferência encontrada',
}: TransferListProps) {
  const accountNames = useAccountNames()
  const cancelTransfer = useCancelTransfer()
  const cancellingId = cancelTransfer.isPending ? cancelTransfer.variables : null

  if (transfers.length === 0) return <EmptyState title={emptyTitle} />

  return (
    <>
      {cancelTransfer.error && (
        <Alert tone="danger" title="Não foi possível cancelar">
          {getErrorMessage(cancelTransfer.error)}
        </Alert>
      )}
      <TransferTable
        transfers={transfers}
        accountNames={accountNames}
        cancellingId={cancellingId ?? null}
        onCancel={(id) => cancelTransfer.mutate(id)}
      />
    </>
  )
}
