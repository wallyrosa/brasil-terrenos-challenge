import { Link } from 'react-router-dom'
import { formatCurrency, formatDocument } from '@/shared/lib/format'
import { Card } from '@/shared/ui'
import type { Account } from '../types'
import { AccountStatusBadge } from './AccountStatusBadge'

interface AccountCardProps {
  account: Account
}

export function AccountCard({ account }: AccountCardProps) {
  return (
    <Link to={`/contas/${account.id}`} className="account-card-link" aria-label={`Ver conta de ${account.holderName}`}>
      <Card className="account-card">
        <div className="account-card__top">
          <div>
            <p className="account-card__holder">{account.holderName}</p>
            <p className="account-card__document">{formatDocument(account.document)}</p>
          </div>
          <AccountStatusBadge status={account.status} />
        </div>
        <div>
          <span className="muted">Saldo</span>
          <p className="account-card__balance">{formatCurrency(account.balance)}</p>
        </div>
        <div className="account-card__rows">
          <div className="account-card__row">
            <span>Limite de cheque especial</span>
            <strong>{formatCurrency(account.overdraftLimit)}</strong>
          </div>
          <div className="account-card__row">
            <span>Cheque especial utilizado</span>
            <strong>{formatCurrency(account.usedOverdraft)}</strong>
          </div>
          <div className="account-card__row">
            <span>Disponível para transferir</span>
            <strong>{formatCurrency(account.availableToTransfer)}</strong>
          </div>
        </div>
      </Card>
    </Link>
  )
}
