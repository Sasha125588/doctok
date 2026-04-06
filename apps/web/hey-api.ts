import type { CreateClientConfig } from './generated/api/client.gen'

const apiBaseUrl = process.env.NUXT_PUBLIC_API_BASE_URL || 'http://localhost:5005'

export const createClientConfig: CreateClientConfig = (config) => ({
  ...config,
  baseUrl: apiBaseUrl,
})
