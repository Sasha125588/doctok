export const savedKindFilterValues = ['all', 'summary', 'example', 'concept', 'tip'] as const
export const savedSortModeValues = ['newest', 'oldest', 'topic'] as const

export type SavedKindFilter = (typeof savedKindFilterValues)[number]
export type SavedSortMode = (typeof savedSortModeValues)[number]
