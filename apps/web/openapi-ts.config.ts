import { defineConfig } from '@hey-api/openapi-ts'

const openapiSpecUrl = process.env.OPENAPI_SPEC_URL ?? 'http://localhost:5005/openapi/v1.json'

export default defineConfig({
  input: openapiSpecUrl,
  output: {
    path: 'generated/api',
    fileName: {
      suffix: '.gen',
    },
    postProcess: ['oxfmt'],
  },
  plugins: [
    {
      name: '@hey-api/client-ofetch',
      runtimeConfigPath: '../../hey-api',
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
