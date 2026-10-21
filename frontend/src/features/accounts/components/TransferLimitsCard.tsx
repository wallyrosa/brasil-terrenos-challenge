import { useState } from 'react'
import type { FormEvent } from 'react'
import { getErrorMessage } from '@/shared/api/http'
import { formatCurrency } from '@/shared/lib/format'
import { isNonNegativeNumber, parseDecimal } from '@/shared/lib/number'
import { Alert, Button, Card, Field, Input } from '@/shared/ui'
import { useUpdateTransferLimits } from '../hooks'
import type { Account, HourlyLimit } from '../types'

interface TransferLimitsCardProps {
  account: Account
}

type Period = 'daytime' | 'nighttime'

interface LimitInputs {
  maxAmountPerHour: string
  maxAttemptsPerHour: string
}

type LimitsInputs = Record<Period, LimitInputs>

const periodLabels: Record<Period, string> = {
  daytime: 'Dia',
  nighttime: 'Noite',
}

const periods: Period[] = ['daytime', 'nighttime']

function toInputs(limit: HourlyLimit): LimitInputs {
  return {
    maxAmountPerHour: String(limit.maxAmountPerHour),
    maxAttemptsPerHour: String(limit.maxAttemptsPerHour),
  }
}

function toLimit(inputs: LimitInputs): HourlyLimit {
  return {
    maxAmountPerHour: parseDecimal(inputs.maxAmountPerHour),
    maxAttemptsPerHour: parseDecimal(inputs.maxAttemptsPerHour),
  }
}

function isValidLimit(limit: HourlyLimit): boolean {
  return (
    isNonNegativeNumber(limit.maxAmountPerHour) &&
    Number.isInteger(limit.maxAttemptsPerHour) &&
    limit.maxAttemptsPerHour >= 0
  )
}

export function TransferLimitsCard({ account }: TransferLimitsCardProps) {
  const [isEditing, setIsEditing] = useState(false)
  const [inputs, setInputs] = useState<LimitsInputs>({
    daytime: toInputs(account.transferLimits.daytime),
    nighttime: toInputs(account.transferLimits.nighttime),
  })
  const [validationError, setValidationError] = useState<string | undefined>()
  const updateLimits = useUpdateTransferLimits(account.id)

  const handleStartEditing = () => {
    setInputs({
      daytime: toInputs(account.transferLimits.daytime),
      nighttime: toInputs(account.transferLimits.nighttime),
    })
    setValidationError(undefined)
    setIsEditing(true)
  }

  const handleChange = (period: Period, field: keyof LimitInputs, value: string) => {
    setInputs((current) => ({ ...current, [period]: { ...current[period], [field]: value } }))
  }

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    const limits = { daytime: toLimit(inputs.daytime), nighttime: toLimit(inputs.nighttime) }
    if (!isValidLimit(limits.daytime) || !isValidLimit(limits.nighttime)) {
      setValidationError('Informe valores válidos. As tentativas devem ser números inteiros.')
      return
    }
    setValidationError(undefined)
    updateLimits.mutate(limits, { onSuccess: () => setIsEditing(false) })
  }

  if (!isEditing) {
    return (
      <Card
        title="Limites de transferência"
        actions={
          <Button variant="secondary" onClick={handleStartEditing}>
            Editar limites
          </Button>
        }
      >
        <div className="table-wrapper">
          <table className="table">
            <thead>
              <tr>
                <th>Período</th>
                <th className="numeric">Valor máximo por hora</th>
                <th className="numeric">Tentativas por hora</th>
              </tr>
            </thead>
            <tbody>
              {periods.map((period) => (
                <tr key={period}>
                  <td>{periodLabels[period]}</td>
                  <td className="numeric">
                    {formatCurrency(account.transferLimits[period].maxAmountPerHour)}
                  </td>
                  <td className="numeric">{account.transferLimits[period].maxAttemptsPerHour}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    )
  }

  return (
    <Card title="Limites de transferência">
      <form className="form" onSubmit={handleSubmit} noValidate>
        {periods.map((period) => (
          <div key={period} className="form__grid">
            <Field label={`${periodLabels[period]}: valor máximo por hora (R$)`}>
              <Input
                type="number"
                min="0"
                step="0.01"
                value={inputs[period].maxAmountPerHour}
                onChange={(event) => handleChange(period, 'maxAmountPerHour', event.target.value)}
              />
            </Field>
            <Field label={`${periodLabels[period]}: tentativas por hora`}>
              <Input
                type="number"
                min="0"
                step="1"
                value={inputs[period].maxAttemptsPerHour}
                onChange={(event) => handleChange(period, 'maxAttemptsPerHour', event.target.value)}
              />
            </Field>
          </div>
        ))}
        {validationError && <Alert tone="danger">{validationError}</Alert>}
        {updateLimits.error && (
          <Alert tone="danger" title="Não foi possível alterar os limites">
            {getErrorMessage(updateLimits.error)}
          </Alert>
        )}
        <div className="form__actions">
          <Button type="submit" isLoading={updateLimits.isPending}>
            Salvar limites
          </Button>
          <Button variant="secondary" onClick={() => setIsEditing(false)}>
            Cancelar
          </Button>
        </div>
      </form>
    </Card>
  )
}
