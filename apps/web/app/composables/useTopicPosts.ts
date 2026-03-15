import { useQuery } from '@tanstack/vue-query'

import type { PostItem, TopicPostsResponse } from '../../client/types.gen'

export function useTopicPosts(
  slug: Ref<string>,
  lang: Ref<string>,
  enabled: Ref<boolean> = computed(() => true)
) {
  const config = useRuntimeConfig()

  const query = useQuery({
    queryKey: ['topic-posts', slug, lang],
    enabled: computed(() => enabled.value && !!slug.value),
    queryFn: async ({ signal }) => {
      return await $fetch<TopicPostsResponse>(`/api/topics/${slug.value}`, {
        baseURL: config.public.apiBaseUrl,
        query: { lang: lang.value },
        signal,
      })
    },
  })

  const posts = computed<PostItem[]>(() => query.data.value?.items ?? [])

  return {
    posts,
    isLoading: query.isLoading,
  }
}
