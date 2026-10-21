import { useState } from 'react'
import type { FormEvent } from 'react'
import { getErrorMessage } from '@/shared/api/http'
import { isNonNegativeNumber, parseDecimal } from '@/shared/lib/number'
import { Alert, Button, Card, Field, Input } from '@/shared/ui'
import { useCreateAccount } from '../hooks'

interface NewAccountFormProps {
  onCreated: () => void
  onCancel: () => void
}

interface FormErrors {
  holderName?: string
  document?: string
  initialBalance?: string
  overdraftLimit?: string
}

const initialValues = {
  holderName: '',
  document: '',
  initialBalance: '0',
  overdraftLimit: '0',
}

function validate(values: typeof initialValues): FormErrors {
  const errors: FormErrors = {}
  if (!values.holderName.trim()) errors.holderName = 'Informe o titular.'
  if (!values.document.replace(/\D/g, '')) errors.document = 'Informe o CPF ou CNPJ.'
  if (!isNonNegativeNumber(parseDecimal(values.initialBalance))) {
    errors.initialBalance = 'Informe um saldo válido.'
  }
  if (!isNonNegativeNumber(parseDecimal(values.overdraftLimit))) {
    errors.overdraftLimit = 'Informe um limite válido.'
  }
  return errors
}

export function NewAccountForm({ onCreated, onCancel }: NewAccountFormProps) {
  const [values, setValues] = useState(initialValues)
  const [errors, setErrors] = useState<FormErrors>({})
  const createAccount = useCreateAccount()

  const handleChange = (field: keyof typeof initialValues) => (value: string) => {
    setValues((current) => ({ ...current, [field]: value }))
  }

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    const validationErrors = validate(values)
    setErrors(validationErrors)
    if (Object.keys(validationErrors).length > 0) return

    createAccount.mutate(
      {
        holderName: values.holderName.trim(),
        document: values.document.replace(/\D/g, ''),
        initialBalance: parseDecimal(values.initialBalance),
        overdraftLimit: parseDecimal(values.overdraftLimit),
      },
      {
        onSuccess: () => {
          setValues(initialValues)
          onCreated()
        },
      },
    )
  }

  return (
    <Card title="Abrir nova conta">
      <form className="form" onSubmit={handleSubmit} noValidate>
        <div className="form__grid">
          <Field label="Titular" error={errors.holderName}>
            <Input
              value={values.holderName}
              onChange={(event) => handleChange('holderName')(event.target.value)}
              autoComplete="off"
            />
          </Field>
          <Field label="CPF/CNPJ" error={errors.document}>
            <Input
              value={values.document}
              onChange={(event) => handleChange('document')(event.target.value)}
              inputMode="numeric"
              autoComplete="off"
            />
          </Field>
          <Field label="Saldo inicial (R$)" error={errors.initialBalance}>
            <Input
              type="number"
              min="0"
              step="0.01"
              value={values.initialBalance}
              onChange={(event) => handleChange('initialBalance')(event.target.value)}
            />
          </Field>
          <Field label="Limite de cheque especial (R$)" error={errors.overdraftLimit}>
            <Input
              type="number"
              min="0"
              step="0.01"
              value={values.overdraftLimit}
              onChange={(event) => handleChange('overdraftLimit')(event.target.value)}
            />
          </Field>
        </div>
        {createAccount.error && (
          <Alert tone="danger" title="Não foi possível abrir a conta">
            {getErrorMessage(createAccount.error)}
          </Alert>
        )}
        <div className="form__actions">
          <Button type="submit" isLoading={createAccount.isPending}>
            Abrir conta
          </Button>
          <Button variant="secondary" onClick={onCancel}>
            Cancelar
          </Button>
        </div>
      </form>
    </Card>
  )
}
