import { useRouteQuery } from '@vueuse/router'

export const savedKindFilterValues = ['all', 'summary', 'example', 'concept', 'tip'] as const
export const savedSortModeValues = ['newest', 'oldest', 'topic'] as const

export type SavedKindFilter = (typeof savedKindFilterValues)[number]
export type SavedSortMode = (typeof savedSortModeValues)[number]

export const useSavedRouteState = () => {
  const searchQuery = useRouteQuery('q', '', { mode: 'replace' })
  const selectedKind = useRouteQuery<SavedKindFilter>('kind', 'all', { mode: 'replace' })
  const selectedSort = useRouteQuery<SavedSortMode>('sort', 'newest', { mode: 'replace' })

  const resetFilters = () => {
    searchQuery.value = ''
    selectedKind.value = 'all'
  }

  return {
    searchQuery,
    selectedKind,
    selectedSort,
    resetFilters,
  }
}
