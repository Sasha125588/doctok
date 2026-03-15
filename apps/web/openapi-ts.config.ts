import { defineConfig } from '@hey-api/openapi-ts'

export default defineConfig({
  input: 'http://localhost:5005/openapi/v1.json',
  output: {
    path: 'client',
    fileName: {
      suffix: '.gen',
    },
    postProcess: ['oxfmt', 'oxlint'],
  },
  plugins: [
    {
      name: '@hey-api/client-ofetch',
      runtimeConfigPath: '../hey-api',
    },
    '@tanstack/vue-query',
    'valibot',
    {
      auth: true,
      name: '@hey-api/sdk',
      validator: true,
    },
  ],
})
