import { useState } from 'react'
import type { FormEvent } from 'react'
import { getErrorMessage } from '@/shared/api/http'
import { isNonNegativeNumber, parseDecimal } from '@/shared/lib/number'
import { Alert, Button, Card, Field, Input } from '@/shared/ui'
import { useUpdateOverdraftLimit } from '../hooks'
import type { Account } from '../types'

interface OverdraftLimitFormProps {
  account: Account
}

export function OverdraftLimitForm({ account }: OverdraftLimitFormProps) {
  const [value, setValue] = useState(String(account.overdraftLimit))
  const [validationError, setValidationError] = useState<string | undefined>()
  const [isSaved, setIsSaved] = useState(false)
  const updateLimit = useUpdateOverdraftLimit(account.id)

  const handleChange = (nextValue: string) => {
    setValue(nextValue)
    setIsSaved(false)
  }

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    const parsed = parseDecimal(value)
    if (!isNonNegativeNumber(parsed)) {
      setValidationError('Informe um limite válido.')
      return
    }
    setValidationError(undefined)
    updateLimit.mutate(parsed, { onSuccess: () => setIsSaved(true) })
  }

  return (
    <Card title="Limite de cheque especial">
      <form className="form" onSubmit={handleSubmit} noValidate>
        <div className="inline-form">
          <Field label="Novo limite (R$)" error={validationError}>
            <Input
              type="number"
              min="0"
              step="0.01"
              value={value}
              onChange={(event) => handleChange(event.target.value)}
            />
          </Field>
          <Button type="submit" isLoading={updateLimit.isPending}>
            Alterar limite
          </Button>
        </div>
        {updateLimit.error && (
          <Alert tone="danger" title="Não foi possível alterar o limite">
            {getErrorMessage(updateLimit.error)}
          </Alert>
        )}
        {isSaved && <Alert tone="success">Limite de cheque especial atualizado.</Alert>}
      </form>
    </Card>
  )
}
