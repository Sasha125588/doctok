import type { CreateClientConfig } from './generated/api/client.gen'

const baseUrl = process.env.NUXT_PUBLIC_API_BASE_URL

export const createClientConfig: CreateClientConfig = (config) => ({
  ...config,
  baseUrl,
})
