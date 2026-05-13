import {
  postsGetContentOptions,
  resolveMdnOptions,
  topicsGetPostsOptions,
} from '#api/@tanstack/vue-query.gen'
import { useQueries, useQuery } from '@tanstack/vue-query'

import { isApiError } from '~/lib/api/errors/errors'

import type { PostContentView, TopicPostView } from '#api/types.gen'

export type TopicStatus = 'ready' | 'failed'

export interface TopicEvent {
  slug: string
  lang: string
  status: TopicStatus
  error: string | null
}

const topicEvents = ['topic-ready', 'topic-failed', 'topic-timeout'] as const

export const useTopicPosts = (topicSlug: Ref<string | null>) => {
  const { postsContentLang } = useLang()

  const queryOptions = computed(() => ({
    query: {
      slug: topicSlug.value ?? '',
      lang: postsContentLang.value,
    },
  }))

  const canFetch = computed(() => !!topicSlug.value?.trim() && !!postsContentLang.value?.trim())

  const variant = usePostContentVariant()

  const query = useQuery(() => ({
    enabled: canFetch,
    ...topicsGetPostsOptions(queryOptions.value),
  }))

  const contentQueries = useQueries({
    queries: computed(
      () =>
        query.data.value?.items.map((post) => ({
          ...postsGetContentOptions({
            path: { postId: +post.id },
            query: { variant: variant.value },
          }),
          enabled: canFetch.value && query.isSuccess.value,
        })) ?? []
    ),
  })

  const contentByPostId = computed(() => {
    const result = new Map<number, PostContentView>()

    for (const contentQuery of contentQueries.value) {
      const content = contentQuery.data
      if (content) result.set(+content.postId, content)
    }

    return result
  })

  const posts = computed<TopicPostView[]>(() => {
    const metas = query.data.value?.items ?? []

    return metas.flatMap((post) => {
      const content = contentByPostId.value.get(+post.id)
      if (!content) return []

      return [
        {
          ...post,
          variantCode: content.variantCode,
          title: content.title,
          body: content.body,
          bodyHtml: content.bodyHtml,
        },
      ]
    })
  })

  const isContentLoading = computed(() => contentQueries.value.some((item) => item.isLoading))
  const isContentFetching = computed(() => contentQueries.value.some((item) => item.isFetching))
  const isLoading = computed(() => query.isLoading.value || isContentLoading.value)
  const isFetching = computed(() => query.isFetching.value || isContentFetching.value)

  const topicStreamUrl = computed(() => {
    const params = new URLSearchParams({
      slug: topicSlug.value ?? '',
      lang: postsContentLang.value,
    })
    return `/api/topics/stream?${params.toString()}`
  })

  const sse = useEventSource(topicStreamUrl, [...topicEvents], {
    immediate: false,
    autoReconnect: false,
    serializer: {
      read: (rawData): TopicEvent => JSON.parse(rawData ?? ''),
    },
  })

  const isTopicNotFound = computed(
    () => query.isError.value && isApiError(query.error.value) && query.error.value.status === 404
  )

  const shouldResolve = computed(() => isTopicNotFound.value && canFetch.value)

  const resolveQuery = useQuery(() => ({
    enabled: shouldResolve.value,
    retry: false,
    refetchOnWindowFocus: false,
    staleTime: Infinity,
    ...resolveMdnOptions({
      query: {
        externalRef: topicSlug.value?.replace(/^mdn\//, '') ?? '',
        lang: postsContentLang.value,
      },
    }),
  }))

  const shouldOpenSse = computed(
    () => resolveQuery.isSuccess.value && resolveQuery.data.value?.status === 'pending'
  )

  watch(shouldOpenSse, (enabled) => (enabled ? sse.open() : sse.close()), {
    immediate: true,
  })

  watch(sse.data, async (payload) => {
    if (!payload) return

    sse.close()

    if (payload.status === 'ready') {
      await query.refetch()
    }
  })

  return {
    ...query,
    posts,
    isLoading,
    isFetching,
  }
}
