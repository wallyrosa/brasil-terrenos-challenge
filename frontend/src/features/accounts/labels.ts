import type { BadgeTone } from '@/shared/ui'
import type { Account, AccountStatus } from './types'

export const accountStatusLabels: Record<AccountStatus, string> = {
  Active: 'Ativa',
  Blocked: 'Bloqueada',
  Inactive: 'Inativa',
}

export const accountStatusTones: Record<AccountStatus, BadgeTone> = {
  Active: 'success',
  Blocked: 'warning',
  Inactive: 'neutral',
}

export function accountOptionLabel(account: Account): string {
  return `${account.holderName} (${accountStatusLabels[account.status]})`
}
