import { useState } from 'react'
import type { FormEvent } from 'react'
import { Button, Card, Field, Input, QueryBoundary } from '@/shared/ui'
import { useTransfer } from '../hooks'
import { TransferList } from './TransferList'

export function TransferLookup() {
  const [inputValue, setInputValue] = useState('')
  const [searchedId, setSearchedId] = useState('')
  const { data: transfer, isLoading, error } = useTransfer(searchedId)

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    setSearchedId(inputValue.trim())
  }

  return (
    <Card title="Consultar por código">
      <form className="inline-form" onSubmit={handleSubmit}>
        <Field label="Código da transferência">
          <Input
            value={inputValue}
            onChange={(event) => setInputValue(event.target.value)}
            placeholder="00000000-0000-0000-0000-000000000000"
            autoComplete="off"
          />
        </Field>
        <Button type="submit" disabled={inputValue.trim() === ''}>
          Buscar
        </Button>
      </form>
      {searchedId && (
        <QueryBoundary isLoading={isLoading} error={error}>
          {transfer && <TransferList transfers={[transfer]} />}
        </QueryBoundary>
      )}
    </Card>
  )
}
