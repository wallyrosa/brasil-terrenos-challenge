import { useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { ApiError, getErrorMessage } from '@/shared/api/http'
import { isComplete, parseBrazilianDateTime } from '@/shared/lib/dateTime'
import { isPositiveNumber, parseDecimal } from '@/shared/lib/number'
import { generateUuid } from '@/shared/lib/uuid'
import { useCreateScheduledTransfer, useCreateTransfer } from './hooks'
import type { Transfer } from './types'

export interface TransferFormValues {
  sourceAccountId: string
  destinationAccountId: string
  amount: string
  isScheduled: boolean
  scheduledFor: string
}

export type TransferFormErrors = Partial<Record<keyof TransferFormValues, string>>

function validate(values: TransferFormValues): TransferFormErrors {
  const errors: TransferFormErrors = {}
  if (!values.sourceAccountId) errors.sourceAccountId = 'Selecione a conta de origem.'
  if (!values.destinationAccountId) {
    errors.destinationAccountId = 'Selecione a conta de destino.'
  } else if (values.destinationAccountId === values.sourceAccountId) {
    errors.destinationAccountId = 'A conta de destino deve ser diferente da origem.'
  }
  if (!isPositiveNumber(parseDecimal(values.amount))) {
    errors.amount = 'Informe um valor maior que zero.'
  }
  if (values.isScheduled) {
    errors.scheduledFor = validateScheduledFor(values.scheduledFor)
    if (!errors.scheduledFor) delete errors.scheduledFor
  }
  return errors
}

function validateScheduledFor(value: string): string | undefined {
  const scheduledDate = parseBrazilianDateTime(value)
  if (!scheduledDate) return 'Informe uma data e hora válidas no formato dd/mm/aaaa hh:mm.'
  if (scheduledDate.getTime() <= Date.now()) return 'O agendamento deve ser em uma data e hora futuras.'
  return undefined
}

function shouldKeepIdempotencyKey(error: unknown): boolean {
  return error instanceof ApiError && (error.isNetworkError || error.isServerError)
}

export function useTransferForm(initialSourceAccountId: string) {
  const [values, setValues] = useState<TransferFormValues>({
    sourceAccountId: initialSourceAccountId,
    destinationAccountId: '',
    amount: '',
    isScheduled: false,
    scheduledFor: '',
  })
  const [errors, setErrors] = useState<TransferFormErrors>({})
  const [result, setResult] = useState<Transfer | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const idempotencyKey = useRef(generateUuid())
  const createTransfer = useCreateTransfer()
  const createScheduledTransfer = useCreateScheduledTransfer()

  const isSubmitting = createTransfer.isPending || createScheduledTransfer.isPending

  const setValue = <K extends keyof TransferFormValues>(field: K, value: TransferFormValues[K]) => {
    idempotencyKey.current = generateUuid()
    setValues((current) => ({ ...current, [field]: value }))
    const fieldError =
      field === 'scheduledFor' && typeof value === 'string' && isComplete(value)
        ? validateScheduledFor(value)
        : undefined
    setErrors((current) => ({ ...current, [field]: fieldError }))
  }

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setResult(null)
    setSubmitError(null)

    const validationErrors = validate(values)
    setErrors(validationErrors)
    if (Object.keys(validationErrors).length > 0) return

    const input = {
      sourceAccountId: values.sourceAccountId,
      destinationAccountId: values.destinationAccountId,
      amount: parseDecimal(values.amount),
    }
    const variables = { idempotencyKey: idempotencyKey.current }

    try {
      const transfer = values.isScheduled
        ? await createScheduledTransfer.mutateAsync({
            ...variables,
            input: { ...input, scheduledFor: parseBrazilianDateTime(values.scheduledFor)!.toISOString() },
          })
        : await createTransfer.mutateAsync({ ...variables, input })
      setResult(transfer)
      idempotencyKey.current = generateUuid()
      setValues((current) => ({ ...current, amount: '' }))
    } catch (error) {
      if (!shouldKeepIdempotencyKey(error)) idempotencyKey.current = generateUuid()
      setSubmitError(getErrorMessage(error))
    }
  }

  return { values, errors, result, submitError, isSubmitting, setValue, submit }
}
