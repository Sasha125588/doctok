import { feedTopicsListInfiniteOptions } from '#api/@tanstack/vue-query.gen'
import { useInfiniteQuery } from '@tanstack/vue-query'

import type { TopicFeedItem } from '#api/types.gen'

const feedPageSize = 3

export function useFeed(lang: Ref<string>) {
  const query = useInfiniteQuery(
    feedTopicsListInfiniteOptions({
      query: {
        lang: lang.value,
        limit: feedPageSize,
      },
    })
  )

  const topics = computed<TopicFeedItem[]>(
    () => query.data.value?.pages.flatMap((page) => page.items) ?? []
  )

  return {
    topics,
    fetchNextPage: query.fetchNextPage,
    hasNextPage: query.hasNextPage,
    isLoading: query.isLoading,
    isFetchingNextPage: query.isFetchingNextPage,
  }
}
