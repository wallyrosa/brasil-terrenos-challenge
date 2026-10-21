import type { BadgeTone } from '@/shared/ui'
import type { TransferFailureReason, TransferStatus, TransferType } from './types'

export const transferStatusLabels: Record<TransferStatus, string> = {
  Scheduled: 'Agendada',
  Processing: 'Processando',
  Completed: 'Concluída',
  Failed: 'Falhou',
  Cancelled: 'Cancelada',
}

export const transferStatusTones: Record<TransferStatus, BadgeTone> = {
  Scheduled: 'info',
  Processing: 'warning',
  Completed: 'success',
  Failed: 'danger',
  Cancelled: 'neutral',
}

export const transferTypeLabels: Record<TransferType, string> = {
  Immediate: 'Imediata',
  Scheduled: 'Agendada',
}

export const failureReasonLabels: Record<TransferFailureReason, string> = {
  SourceAccountInactive: 'A conta de origem não está ativa.',
  DestinationAccountInactive: 'A conta de destino não está ativa.',
  InsufficientFunds: 'Saldo insuficiente (incluindo o cheque especial disponível).',
  HourlyAmountLimitExceeded: 'O valor máximo por hora da conta de origem foi excedido.',
  HourlyAttemptsLimitExceeded: 'O número máximo de tentativas por hora da conta de origem foi excedido.',
}
