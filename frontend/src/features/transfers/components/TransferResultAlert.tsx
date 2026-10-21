import { formatCurrency, formatDateTime } from '@/shared/lib/format'
import { Alert } from '@/shared/ui'
import { failureReasonLabels } from '../labels'
import type { Transfer } from '../types'

interface TransferResultAlertProps {
  transfer: Transfer
}

export function TransferResultAlert({ transfer }: TransferResultAlertProps) {
  const amount = formatCurrency(transfer.amount)

  if (transfer.status === 'Failed') {
    return (
      <Alert tone="danger" title="Transferência recusada">
        {transfer.failureReason
          ? failureReasonLabels[transfer.failureReason]
          : 'A transferência não pôde ser concluída.'}
      </Alert>
    )
  }

  if (transfer.status === 'Scheduled') {
    return (
      <Alert tone="info" title="Transferência agendada">
        {`${amount} serão transferidos em ${formatDateTime(transfer.scheduledFor)}. Código: ${transfer.id}`}
      </Alert>
    )
  }

  return (
    <Alert tone="success" title="Transferência realizada">
      {`${amount} transferidos com sucesso. Código: ${transfer.id}`}
    </Alert>
  )
}
