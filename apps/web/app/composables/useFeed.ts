import { feedTopicsListInfiniteOptions } from '#api/@tanstack/vue-query.gen'
import { useInfiniteQuery } from '@tanstack/vue-query'

const feedPageSize = 3

export function useFeed(lang: Ref<string>) {
  const query = useInfiniteQuery(() => ({
    ...feedTopicsListInfiniteOptions({
      query: {
        lang: lang.value,
        limit: feedPageSize,
      },
    }),
    initialPageParam: '',
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
  }))

  const topics = computed(() => query.data.value?.pages.flatMap((page) => page.items) ?? [])

  return {
    state: {
      topics: topics,
      hasNextPage: query.hasNextPage,
      isLoading: query.isLoading,
      isFetchingNextPage: query.isFetchingNextPage,
    },
    functions: {
      fetchNextPage: query.fetchNextPage,
    },
  }
}
