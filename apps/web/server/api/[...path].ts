const trailingSlash = /\/$/

export default defineEventHandler((event) => {
  const runtimeConfig = useRuntimeConfig(event)
  const apiBaseUrl = runtimeConfig.apiBaseUrl.replace(trailingSlash, '')
  const requestUrl = getRequestURL(event)
  const targetUrl = new URL(`${requestUrl.pathname}${requestUrl.search}`, `${apiBaseUrl}/`)

  return proxyRequest(event, targetUrl.toString())
})
