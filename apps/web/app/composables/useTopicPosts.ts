import { resolveMdnOptions, topicsGetPostsOptions } from '#api/@tanstack/vue-query.gen'
import { type Options } from '#api/sdk.gen'
import { useQuery } from '@tanstack/vue-query'

import { isApiError } from '~/lib/api/errors/errors'

import type { TopicsGetPostsData } from '#api/types.gen'

export type TopicStatus = 'ready' | 'failed'

export interface TopicEvent {
  slug: string
  lang: string
  status: TopicStatus
  error: string | null
}

const topicEvents = ['topic-ready', 'topic-failed', 'topic-timeout'] as const

export function useTopicPosts(options: Options<TopicsGetPostsData>) {
  const query = useQuery(topicsGetPostsOptions(options))

  const posts = computed(() => query.data.value?.items ?? [])

  const params = new URLSearchParams({
    slug: options.query.slug,
    lang: options.query.lang!,
  })

  const sse = useEventSource(`/api/topics/stream?${params.toString()}`, [...topicEvents], {
    immediate: false,
    autoReconnect: false,
    serializer: {
      read: (rawData): TopicEvent => JSON.parse(rawData ?? ''),
    },
  })

  const isTopicNotFound = computed(
    () => query.isError.value && isApiError(query.error.value) && query.error.value.status === 404
  )

  const shouldResolve = computed(() => {
    return isTopicNotFound.value && !!options.query.slug && !!options.query.lang
  })

  const resolveQuery = useQuery({
    ...resolveMdnOptions({
      query: { externalRef: options.query.slug.replace(/^mdn\//, ''), lang: options.query.lang },
    }),
    enabled: shouldResolve,
    retry: false,
    refetchOnWindowFocus: false,
    staleTime: Infinity,
  })

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
    state: {
      posts,
      ...query,
    },
  }
}
