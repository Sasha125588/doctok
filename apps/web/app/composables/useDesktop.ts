import { useMediaQuery } from '@vueuse/core'

export function useDesktop() {
  return useMediaQuery('(min-width: 1024px)', { ssrWidth: 1024 })
}
