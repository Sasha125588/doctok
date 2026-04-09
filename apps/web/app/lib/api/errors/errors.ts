type ApiProblem = {
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

export function isApiProblem(error: unknown): error is ApiProblem {
  return !!error && typeof error === 'object' && 'status' in error
}
