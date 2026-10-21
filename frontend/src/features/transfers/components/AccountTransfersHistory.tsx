import { Card, QueryBoundary } from '@/shared/ui'
import { useAccountTransfers } from '../hooks'
import { TransferList } from './TransferList'

interface AccountTransfersHistoryProps {
  accountId: string
  title?: string
}

export function AccountTransfersHistory({
  accountId,
  title = 'Histórico de transferências',
}: AccountTransfersHistoryProps) {
  const { data: transfers, isLoading, error } = useAccountTransfers(accountId)

  return (
    <Card title={title}>
      <QueryBoundary isLoading={isLoading} error={error}>
        <TransferList
          transfers={transfers ?? []}
          emptyTitle="Esta conta ainda não possui transferências"
        />
      </QueryBoundary>
    </Card>
  )
}
