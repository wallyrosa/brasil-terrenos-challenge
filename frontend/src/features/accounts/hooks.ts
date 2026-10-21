import { useMemo } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { accountsApi } from './api'
import type { AccountStatusAction, CreateAccountInput, TransferLimits } from './types'

export const accountKeys = {
  all: ['accounts'] as const,
  detail: (id: string) => ['accounts', id] as const,
}

export function useAccounts() {
  return useQuery({ queryKey: accountKeys.all, queryFn: accountsApi.list })
}

export function useAccount(id: string) {
  return useQuery({
    queryKey: accountKeys.detail(id),
    queryFn: () => accountsApi.get(id),
  })
}

export function useAccountNames() {
  const { data } = useAccounts()
  return useMemo(() => {
    const names = new Map<string, string>()
    data?.forEach((account) => names.set(account.id, account.holderName))
    return names
  }, [data])
}

function useInvalidateAccounts() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: accountKeys.all })
}

export function useCreateAccount() {
  const invalidateAccounts = useInvalidateAccounts()
  return useMutation({
    mutationFn: (input: CreateAccountInput) => accountsApi.create(input),
    onSuccess: invalidateAccounts,
  })
}

export function useUpdateOverdraftLimit(id: string) {
  const invalidateAccounts = useInvalidateAccounts()
  return useMutation({
    mutationFn: (overdraftLimit: number) => accountsApi.updateOverdraftLimit(id, overdraftLimit),
    onSuccess: invalidateAccounts,
  })
}

export function useUpdateTransferLimits(id: string) {
  const invalidateAccounts = useInvalidateAccounts()
  return useMutation({
    mutationFn: (limits: TransferLimits) => accountsApi.updateTransferLimits(id, limits),
    onSuccess: invalidateAccounts,
  })
}

export function useChangeAccountStatus(id: string) {
  const invalidateAccounts = useInvalidateAccounts()
  return useMutation({
    mutationFn: (action: AccountStatusAction) => accountsApi.changeStatus(id, action),
    onSuccess: invalidateAccounts,
  })
}
