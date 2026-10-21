import { ApiError, http } from '@/shared/api/http'
import type { CreateTransferInput, ScheduleTransferInput, Transfer } from './types'

const IDEMPOTENCY_HEADER = 'Idempotency-Key'
const UNPROCESSABLE_ENTITY = 422

function extractRejectedTransfer(error: unknown): Transfer | null {
  if (!(error instanceof ApiError) || error.status !== UNPROCESSABLE_ENTITY) return null
  const transfer = error.problem?.transfer
  return transfer ? (transfer as Transfer) : null
}

export const transfersApi = {
  get: (id: string) => http.get<Transfer>(`/transfers/${id}`),
  listByAccount: (accountId: string) =>
    http.get<Transfer[]>(`/transfers?accountId=${encodeURIComponent(accountId)}`),
  createImmediate: async (input: CreateTransferInput, idempotencyKey: string) => {
    try {
      return await http.post<Transfer>('/transfers', input, {
        [IDEMPOTENCY_HEADER]: idempotencyKey,
      })
    } catch (error) {
      const rejectedTransfer = extractRejectedTransfer(error)
      if (rejectedTransfer) return rejectedTransfer
      throw error
    }
  },
  createScheduled: (input: ScheduleTransferInput, idempotencyKey: string) =>
    http.post<Transfer>('/transfers/scheduled', input, {
      [IDEMPOTENCY_HEADER]: idempotencyKey,
    }),
  cancel: (id: string) => http.post<Transfer>(`/transfers/${id}/cancel`),
}
