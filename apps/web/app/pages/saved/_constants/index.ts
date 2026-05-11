import type { SavedKindFilter, SavedSortMode, SavedViewMode } from '../_types'

export const kindFilters: Array<{ value: SavedKindFilter; label: string }> = [
  { value: 'all', label: 'all' },
  { value: 'summary', label: 'summary' },
  { value: 'example', label: 'example' },
  { value: 'concept', label: 'concept' },
  { value: 'tip', label: 'tip' },
]

export const sortOptions: Array<{ value: SavedSortMode; label: string }> = [
  { value: 'newest', label: '↓ новіші спочатку' },
  { value: 'oldest', label: '↑ старіші спочатку' },
  { value: 'topic', label: 'A-Z за темою' },
]

export const viewOptions: Array<{ value: SavedViewMode; label: string; icon: string }> = [
  { value: 'grid', label: 'Сітка', icon: 'lucide:layout-grid' },
  { value: 'grouped', label: 'За темами', icon: 'lucide:layers-2' },
]
