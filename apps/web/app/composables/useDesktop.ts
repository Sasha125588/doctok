import { useMediaQuery } from '@vueuse/core'

export const useDesktop = () => useMediaQuery('(min-width: 1024px)', { ssrWidth: 1024 })
