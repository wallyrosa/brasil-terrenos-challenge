export type AccountStatus = 'Active' | 'Blocked' | 'Inactive'

export type AccountStatusAction = 'block' | 'activate' | 'deactivate'

export interface HourlyLimit {
  maxAmountPerHour: number
  maxAttemptsPerHour: number
}

export interface TransferLimits {
  daytime: HourlyLimit
  nighttime: HourlyLimit
}

export interface Account {
  id: string
  holderName: string
  document: string
  status: AccountStatus
  balance: number
  overdraftLimit: number
  usedOverdraft: number
  availableOverdraft: number
  availableToTransfer: number
  transferLimits: TransferLimits
  createdAt: string
}

export interface CreateAccountInput {
  holderName: string
  document: string
  initialBalance: number
  overdraftLimit: number
}
