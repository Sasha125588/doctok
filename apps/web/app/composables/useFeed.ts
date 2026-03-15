import { useInfiniteQuery } from '@tanstack/vue-query'

import type { TopicFeedItem, TopicFeedResponse } from '~/lib/topic-feed'

const feedPageSize = 6

export function useFeed(lang: Ref<string>) {
  const config = useRuntimeConfig()

  const query = useInfiniteQuery({
    queryKey: ['feed-topics', lang],
    initialPageParam: null as null | string,
    queryFn: async ({ pageParam, signal }) => {
      const cursor = typeof pageParam === 'string' && pageParam.length > 0 ? pageParam : undefined

      return await $fetch<TopicFeedResponse>('/api/feed/topics', {
        baseURL: config.public.apiBaseUrl,
        query: {
          lang: lang.value,
          limit: feedPageSize,
          cursor,
        },
        signal,
      })
    },
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
  })

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
