import { useRouteQuery } from '@vueuse/router'

import {
  type SavedKindFilter,
  type SavedSortMode,
  type SavedViewMode,
  savedViewModeValues,
} from '../_types'

const isSavedViewMode = (value: unknown): value is SavedViewMode =>
  savedViewModeValues.includes(value as SavedViewMode)

export const useSavedRouteState = () => {
  const searchQuery = useRouteQuery<string>('q', '')
  const selectedKind = useRouteQuery<SavedKindFilter>('kind', 'all')
  const selectedSort = useRouteQuery<SavedSortMode>('sort', 'newest')
  const routeView = useRouteQuery<SavedViewMode | null>('view', null)
  const storedView = useLocalStorage<SavedViewMode>('dt:saved:view', 'grid')

  watch(
    routeView,
    (value) => {
      if (isSavedViewMode(value)) {
        storedView.value = value
      }
    },
    { immediate: true }
  )

  const selectedView = computed<SavedViewMode>({
    get: () => {
      if (isSavedViewMode(routeView.value)) return routeView.value
      if (isSavedViewMode(storedView.value)) return storedView.value

      return 'grid'
    },
    set: (value) => {
      storedView.value = value
      routeView.value = value
    },
  })

  const resetFilters = () => {
    searchQuery.value = ''
    selectedKind.value = 'all'
  }

  const normalizedSearchQuery = computed(() => searchQuery.value.trim().toLowerCase())

  return {
    searchQuery,
    normalizedSearchQuery,
    selectedKind,
    selectedSort,
    selectedView,
    resetFilters,
  }
}
