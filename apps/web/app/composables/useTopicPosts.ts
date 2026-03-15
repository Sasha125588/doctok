import { useQuery } from '@tanstack/vue-query'

import { topicsGetPosts } from '../../client/sdk.gen'

import type { PostItem } from '../../client/types.gen'

export function useTopicPosts(
  slug: Ref<string>,
  lang: Ref<string>,
  enabled: Ref<boolean> = computed(() => true)
) {
  const query = useQuery({
    queryKey: ['topic-posts', slug, lang],
    enabled: computed(() => enabled.value && !!slug.value),
    queryFn: async ({ signal }) => {
      const { data } = await topicsGetPosts({
        query: {
          slug: slug.value,
          lang: lang.value,
        },
        signal,
        throwOnError: true,
      })

      return data
    },
  })

  const posts = computed<PostItem[]>(() => query.data.value?.items ?? [])

  return {
    posts,
    isLoading: query.isLoading,
  }
}
