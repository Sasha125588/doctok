import type { SavedPostView } from '~~/generated/api/types.gen'

export const savedKindFilterValues = ['all', 'summary', 'example', 'concept', 'tip'] as const
export const savedSortModeValues = ['newest', 'oldest', 'topic'] as const
export const savedViewModeValues = ['grid', 'grouped'] as const

export type SavedKindFilter = (typeof savedKindFilterValues)[number]
export type SavedSortMode = (typeof savedSortModeValues)[number]
export type SavedViewMode = (typeof savedViewModeValues)[number]

export interface SavedTopicGroup {
  topicTitle: string
  topicSlug: string
  lastSavedAt: string
  posts: SavedPostView[]
}
