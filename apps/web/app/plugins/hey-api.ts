import { client } from '#api/client.gen'

export default defineNuxtPlugin(() => {
  const runtimeConfig = useRuntimeConfig()
  const token = useCookie('doctok_auth_token')

  client.setConfig({
    baseUrl: runtimeConfig.public.apiBaseUrl,
    onRequest: ({ options }) => {
      if (token.value) {
        options.headers.set('Authorization', `Bearer ${token.value}`)
      }
    },
  })
})
