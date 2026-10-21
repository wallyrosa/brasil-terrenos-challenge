const API_BASE = '/api'

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  code?: string
  [extension: string]: unknown
}

export class ApiError extends Error {
  readonly status: number
  readonly code: string | null
  readonly problem: ProblemDetails | null

  constructor(message: string, status: number, problem: ProblemDetails | null = null) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = problem?.code ?? null
    this.problem = problem
  }

  get isNetworkError(): boolean {
    return this.status === 0
  }

  get isServerError(): boolean {
    return this.status >= 500
  }
}

const FALLBACK_MESSAGES: Record<number, string> = {
  400: 'Os dados informados são inválidos.',
  404: 'Registro não encontrado.',
  409: 'A operação conflita com o estado atual do registro.',
  422: 'A operação não pôde ser concluída.',
  500: 'Erro interno do servidor. Tente novamente em instantes.',
}

async function toApiError(response: Response): Promise<ApiError> {
  let problem: ProblemDetails | null = null
  try {
    problem = (await response.json()) as ProblemDetails
  } catch {
    problem = null
  }
  const message =
    problem?.detail ??
    problem?.title ??
    FALLBACK_MESSAGES[response.status] ??
    `Erro inesperado (${response.status}).`
  return new ApiError(message, response.status, problem)
}

async function request<T>(
  method: string,
  path: string,
  body?: unknown,
  headers?: Record<string, string>,
): Promise<T> {
  const hasBody = body !== undefined
  let response: Response
  try {
    response = await fetch(`${API_BASE}${path}`, {
      method,
      headers: {
        Accept: 'application/json',
        ...(hasBody ? { 'Content-Type': 'application/json' } : {}),
        ...headers,
      },
      body: hasBody ? JSON.stringify(body) : undefined,
    })
  } catch {
    throw new ApiError(
      'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.',
      0,
    )
  }

  if (!response.ok) throw await toApiError(response)
  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}

export const http = {
  get: <T>(path: string) => request<T>('GET', path),
  post: <T>(path: string, body?: unknown, headers?: Record<string, string>) =>
    request<T>('POST', path, body, headers),
  put: <T>(path: string, body?: unknown) => request<T>('PUT', path, body),
}

export function getErrorMessage(error: unknown): string {
  if (error instanceof Error) return error.message
  return 'Ocorreu um erro inesperado.'
}
