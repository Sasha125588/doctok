type ApiError = {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
  errors?: Array<{
    code: string
    description: string
    type: string
  }>
}

export function isApiError(error: unknown): error is ApiError {
  return !!error && typeof error === 'object' && 'status' in error
}
