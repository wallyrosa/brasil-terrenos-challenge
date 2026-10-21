import type { ReactNode } from 'react'
import { getErrorMessage } from '@/shared/api/http'
import { Alert } from './Alert'
import { Spinner } from './Spinner'

interface QueryBoundaryProps {
  isLoading: boolean
  error: unknown
  children: ReactNode
}

export function QueryBoundary({ isLoading, error, children }: QueryBoundaryProps) {
  if (isLoading) return <Spinner />
  if (error) {
    return (
      <Alert tone="danger" title="Erro ao carregar">
        {getErrorMessage(error)}
      </Alert>
    )
  }
  return <>{children}</>
}
