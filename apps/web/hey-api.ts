import { useCookie } from 'nuxt/app'

import type { CreateClientConfig } from './client/client.gen'

export const createClientConfig: CreateClientConfig = (config) => ({
  ...config,
  baseUrl: 'http://localhost:5005',
  onRequest: ({ options }) => {
    const token = useCookie('auth_token')
    if (token.value) {
      options.headers.set('Authorization', `Bearer ${token.value}`)
    }
  },
})
