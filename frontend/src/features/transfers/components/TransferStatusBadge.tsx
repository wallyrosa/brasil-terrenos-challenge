import { Badge } from '@/shared/ui'
import { transferStatusLabels, transferStatusTones } from '../labels'
import type { TransferStatus } from '../types'

interface TransferStatusBadgeProps {
  status: TransferStatus
}

export function TransferStatusBadge({ status }: TransferStatusBadgeProps) {
  return <Badge tone={transferStatusTones[status]}>{transferStatusLabels[status]}</Badge>
}
