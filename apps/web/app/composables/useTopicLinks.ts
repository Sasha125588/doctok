import { useQuery } from '@tanstack/vue-query'

import type { TopicLink } from '../../client/types.gen'

export function useTopicLinks(slug: Ref<string>, lang: Ref<string>, enabled: Ref<boolean>) {
  const config = useRuntimeConfig()

  const query = useQuery({
    queryKey: ['topic-links', slug, lang],
    enabled,
    queryFn: async ({ signal }) => {
      return await $fetch<TopicLink[]>(`/api/topics/${slug.value}/links`, {
        baseURL: config.public.apiBaseUrl,
        query: { lang: lang.value },
        signal,
      })
    },
  })

  const links = computed<TopicLink[]>(() => query.data.value ?? [])

  return {
    links,
    isLoading: query.isLoading,
  }
}
