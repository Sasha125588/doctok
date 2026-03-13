import { useQuery } from '@tanstack/vue-query'

import { topicsGetPostsOptions } from '../../client/@tanstack/vue-query.gen'

import type { PostItem } from '../../client/types.gen'

export function useTopicPosts(slug: Ref<string>, lang: Ref<string>) {
  const query = useQuery({
    ...topicsGetPostsOptions({
      path: { slug: slug.value },
      query: { lang: lang.value },
    }),
    enabled: computed(() => !!slug.value),
  })

  const posts = computed<PostItem[]>(() => query.data.value?.items ?? [])

  return {
    posts,
    isLoading: query.isLoading,
  }
}
