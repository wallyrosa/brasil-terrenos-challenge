export type TransferType = 'Immediate' | 'Scheduled'

export type TransferStatus = 'Scheduled' | 'Processing' | 'Completed' | 'Failed' | 'Cancelled'

export type TransferFailureReason =
  | 'SourceAccountInactive'
  | 'DestinationAccountInactive'
  | 'InsufficientFunds'
  | 'HourlyAmountLimitExceeded'
  | 'HourlyAttemptsLimitExceeded'

export interface Transfer {
  id: string
  sourceAccountId: string
  destinationAccountId: string
  amount: number
  type: TransferType
  status: TransferStatus
  scheduledFor: string | null
  createdAt: string
  processedAt: string | null
  cancelledAt: string | null
  failureReason: TransferFailureReason | null
}

export interface CreateTransferInput {
  sourceAccountId: string
  destinationAccountId: string
  amount: number
}

export interface ScheduleTransferInput extends CreateTransferInput {
  scheduledFor: string
}
