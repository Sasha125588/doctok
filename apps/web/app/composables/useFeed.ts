import { useInfiniteQuery } from '@tanstack/vue-query'

import { feedListInfiniteOptions } from '../../client/@tanstack/vue-query.gen'

import type { PostItem } from '../../client/types.gen'

export function useFeed(lang: Ref<string>) {
  const query = useInfiniteQuery({
    ...feedListInfiniteOptions({
      query: { lang: lang.value, limit: 2 },
    }),
    getNextPageParam: (lastPage) => lastPage.nextCursor,
    initialPageParam: '',
  })

  const posts = computed<PostItem[]>(() => query.data.value?.pages.flatMap((p) => p.items) ?? [])

  return {
    posts,
    fetchNextPage: query.fetchNextPage,
    hasNextPage: query.hasNextPage,
    isLoading: query.isLoading,
    isFetchingNextPage: query.isFetchingNextPage,
  }
}
