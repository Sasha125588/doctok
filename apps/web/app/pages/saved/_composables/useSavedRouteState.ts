import { useRouteQuery } from '@vueuse/router'

import type { SavedKindFilter, SavedSortMode } from '../_types'

export const useSavedRouteState = () => {
  const searchQuery = useRouteQuery<string>('q', '')
  const selectedKind = useRouteQuery<SavedKindFilter>('kind', 'all')
  const selectedSort = useRouteQuery<SavedSortMode>('sort', 'newest')

  const resetFilters = () => {
    searchQuery.value = ''
    selectedKind.value = 'all'
    selectedSort.value = 'newest'
  }

  const normalizedSearchQuery = computed(() => searchQuery.value.trim().toLowerCase())

  return {
    searchQuery,
    normalizedSearchQuery,
    selectedKind,
    selectedSort,
    resetFilters,
  }
}
