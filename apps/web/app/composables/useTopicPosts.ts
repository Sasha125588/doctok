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

export function useTopicPosts(options: Ref<Options<TopicsGetPostsData>>) {
  const canFetch = computed(
    () => Boolean(options.value.query.slug?.trim()) && Boolean(options.value.query.lang?.trim())
  )

  const query = useQuery(() => ({
    ...topicsGetPostsOptions(options.value),
    enabled: canFetch.value,
  }))

  const posts = computed(() => query.data.value?.items ?? [])

  const topicStreamUrl = computed(() => {
    const params = new URLSearchParams({
      slug: options.value.query.slug,
      lang: options.value.query.lang,
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
    ...resolveMdnOptions({
      query: {
        externalRef: options.value.query.slug.replace(/^mdn\//, ''),
        lang: options.value.query.lang,
      },
    }),
    enabled: shouldResolve.value,
    retry: false,
    refetchOnWindowFocus: false,
    staleTime: Infinity,
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
    state: {
      posts,
      ...query,
    },
  }
}
