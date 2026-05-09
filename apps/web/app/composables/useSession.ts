import { useQuery } from '@tanstack/vue-query'
import { sessionMeGetOptions } from '~~/generated/api/@tanstack/vue-query.gen'

export const useSession = () => {
  const token = useCookie('doctok_auth_token')
  const enabled = computed(() => !!token.value)

  return useQuery({
    enabled,
    retry: false,
    retryOnMount: false,
    refetchOnMount: false,
    refetchOnReconnect: false,
    staleTime: Infinity,
    ...sessionMeGetOptions(),
  })
}
