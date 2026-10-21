import { Badge } from '@/shared/ui'
import { accountStatusLabels, accountStatusTones } from '../labels'
import type { AccountStatus } from '../types'

interface AccountStatusBadgeProps {
  status: AccountStatus
}

export function AccountStatusBadge({ status }: AccountStatusBadgeProps) {
  return <Badge tone={accountStatusTones[status]}>{accountStatusLabels[status]}</Badge>
}
