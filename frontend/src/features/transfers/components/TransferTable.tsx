import { formatCurrency, formatDateTime, shortId } from '@/shared/lib/format'
import { Button } from '@/shared/ui'
import { failureReasonLabels, transferTypeLabels } from '../labels'
import type { Transfer } from '../types'
import { TransferStatusBadge } from './TransferStatusBadge'

interface TransferTableProps {
  transfers: Transfer[]
  accountNames: Map<string, string>
  cancellingId: string | null
  onCancel: (id: string) => void
}

export function TransferTable({
  transfers,
  accountNames,
  cancellingId,
  onCancel,
}: TransferTableProps) {
  const accountLabel = (id: string) => accountNames.get(id) ?? shortId(id)

  return (
    <div className="table-wrapper">
      <table className="table">
        <thead>
          <tr>
            <th>Criada em</th>
            <th>Origem</th>
            <th>Destino</th>
            <th className="numeric">Valor</th>
            <th>Tipo</th>
            <th>Status</th>
            <th>Ações</th>
          </tr>
        </thead>
        <tbody>
          {transfers.map((transfer) => (
            <tr key={transfer.id}>
              <td>
                {formatDateTime(transfer.createdAt)}
                <span className="cell-secondary">{shortId(transfer.id)}</span>
              </td>
              <td>{accountLabel(transfer.sourceAccountId)}</td>
              <td>{accountLabel(transfer.destinationAccountId)}</td>
              <td className="numeric">{formatCurrency(transfer.amount)}</td>
              <td>
                {transferTypeLabels[transfer.type]}
                {transfer.scheduledFor && (
                  <span className="cell-secondary">{formatDateTime(transfer.scheduledFor)}</span>
                )}
              </td>
              <td>
                <TransferStatusBadge status={transfer.status} />
                {transfer.failureReason && (
                  <span className="cell-secondary">
                    {failureReasonLabels[transfer.failureReason]}
                  </span>
                )}
              </td>
              <td>
                {transfer.status === 'Scheduled' && (
                  <Button
                    variant="danger"
                    isLoading={cancellingId === transfer.id}
                    onClick={() => onCancel(transfer.id)}
                  >
                    Cancelar
                  </Button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
