import { getErrorMessage } from '@/shared/api/http'
import { Alert, Button, Card } from '@/shared/ui'
import { useChangeAccountStatus } from '../hooks'
import type { Account, AccountStatusAction } from '../types'

interface AccountStatusActionsProps {
  account: Account
}

interface StatusActionOption {
  action: AccountStatusAction
  label: string
  variant: 'secondary' | 'danger'
  isAvailable: (account: Account) => boolean
}

const options: StatusActionOption[] = [
  {
    action: 'activate',
    label: 'Ativar conta',
    variant: 'secondary',
    isAvailable: (account) => account.status !== 'Active',
  },
  {
    action: 'block',
    label: 'Bloquear conta',
    variant: 'danger',
    isAvailable: (account) => account.status !== 'Blocked',
  },
  {
    action: 'deactivate',
    label: 'Inativar conta',
    variant: 'danger',
    isAvailable: (account) => account.status !== 'Inactive',
  },
]

export function AccountStatusActions({ account }: AccountStatusActionsProps) {
  const changeStatus = useChangeAccountStatus(account.id)

  return (
    <Card title="Situação da conta">
      <div className="form__actions">
        {options
          .filter((option) => option.isAvailable(account))
          .map((option) => (
            <Button
              key={option.action}
              variant={option.variant}
              disabled={changeStatus.isPending}
              onClick={() => changeStatus.mutate(option.action)}
            >
              {option.label}
            </Button>
          ))}
      </div>
      {changeStatus.error && (
        <Alert tone="danger" title="Não foi possível alterar a situação">
          {getErrorMessage(changeStatus.error)}
        </Alert>
      )}
    </Card>
  )
}
