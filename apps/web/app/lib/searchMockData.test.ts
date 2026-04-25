import { describe, expect, test } from 'bun:test'

import { filterMockSearchHits } from './searchMockData'

describe('filterMockSearchHits', () => {
  test('matches query against title, topic name, and related terms case-insensitively', () => {
    expect(filterMockSearchHits('grid').map((hit) => hit.post.title)).toContain('CSS Grid')
    expect(filterMockSearchHits('promises').map((hit) => hit.post.title)).toContain(
      'JavaScript Promises'
    )
    expect(filterMockSearchHits('AUTO-FIT').map((hit) => hit.post.title)).toContain(
      'Три рівні колонки'
    )
  })

  test('limits the result set and trims surrounding whitespace', () => {
    const trimmedResults = filterMockSearchHits('  css  ', 2)
    const plainResults = filterMockSearchHits('css', 2)

    expect(trimmedResults).toEqual(plainResults)
    expect(trimmedResults.length).toBe(2)
  })

  test('returns an empty list for a blank query', () => {
    expect(filterMockSearchHits('   ')).toEqual([])
  })
})
