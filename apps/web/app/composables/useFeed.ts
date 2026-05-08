import { feedTopicsListInfiniteOptions } from '#api/@tanstack/vue-query.gen'
import { useInfiniteQuery } from '@tanstack/vue-query'

const feedPageSize = 3

export const useFeed = () => {
  const { lang } = useLang()

  const queryOptions = computed(() => ({
    query: {
      lang: lang.value,
      limit: feedPageSize,
    },
  }))

  const query = useInfiniteQuery(() => ({
    ...feedTopicsListInfiniteOptions(queryOptions.value),
    initialPageParam: '',
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
  }))

  const topics = computed(() => query.data.value?.pages.flatMap((page) => page.items) ?? [])

  return {
    topics: topics,
    hasNextPage: query.hasNextPage,
    isLoading: query.isLoading,
    isFetchingNextPage: query.isFetchingNextPage,
    fetchNextPage: query.fetchNextPage,
  }
}
