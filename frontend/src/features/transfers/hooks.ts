import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { accountKeys } from '@/features/accounts/hooks'
import { transfersApi } from './api'
import type { CreateTransferInput, ScheduleTransferInput } from './types'

export const transferKeys = {
  all: ['transfers'] as const,
  byAccount: (accountId: string) => ['transfers', 'account', accountId] as const,
  detail: (id: string) => ['transfers', 'detail', id] as const,
}

interface SubmissionVariables<TInput> {
  input: TInput
  idempotencyKey: string
}

export function useAccountTransfers(accountId: string) {
  return useQuery({
    queryKey: transferKeys.byAccount(accountId),
    queryFn: () => transfersApi.listByAccount(accountId),
    enabled: accountId !== '',
  })
}

export function useTransfer(id: string) {
  return useQuery({
    queryKey: transferKeys.detail(id),
    queryFn: () => transfersApi.get(id),
    enabled: id !== '',
    retry: false,
  })
}

function useInvalidateAfterTransfer() {
  const queryClient = useQueryClient()
  return () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: transferKeys.all }),
      queryClient.invalidateQueries({ queryKey: accountKeys.all }),
    ])
}

export function useCreateTransfer() {
  const invalidate = useInvalidateAfterTransfer()
  return useMutation({
    mutationFn: ({ input, idempotencyKey }: SubmissionVariables<CreateTransferInput>) =>
      transfersApi.createImmediate(input, idempotencyKey),
    onSuccess: invalidate,
  })
}

export function useCreateScheduledTransfer() {
  const invalidate = useInvalidateAfterTransfer()
  return useMutation({
    mutationFn: ({ input, idempotencyKey }: SubmissionVariables<ScheduleTransferInput>) =>
      transfersApi.createScheduled(input, idempotencyKey),
    onSuccess: invalidate,
  })
}

export function useCancelTransfer() {
  const invalidate = useInvalidateAfterTransfer()
  return useMutation({
    mutationFn: (id: string) => transfersApi.cancel(id),
    onSuccess: invalidate,
  })
}
