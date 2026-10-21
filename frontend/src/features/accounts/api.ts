import { http } from '@/shared/api/http'
import type {
  Account,
  AccountStatusAction,
  CreateAccountInput,
  TransferLimits,
} from './types'

export const accountsApi = {
  list: () => http.get<Account[]>('/accounts'),
  get: (id: string) => http.get<Account>(`/accounts/${id}`),
  create: (input: CreateAccountInput) => http.post<Account>('/accounts', input),
  updateOverdraftLimit: (id: string, overdraftLimit: number) =>
    http.put<Account>(`/accounts/${id}/overdraft-limit`, { overdraftLimit }),
  updateTransferLimits: (id: string, limits: TransferLimits) =>
    http.put<Account>(`/accounts/${id}/transfer-limits`, limits),
  changeStatus: (id: string, action: AccountStatusAction) =>
    http.post<Account>(`/accounts/${id}/${action}`),
}
