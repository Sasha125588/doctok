import { topicsGetPostsOptions } from '#api/@tanstack/vue-query.gen'
import { type Options } from '#api/sdk.gen'
import { useQuery } from '@tanstack/vue-query'

import type { PostItem, TopicsGetPostsData } from '#api/types.gen'

export function useTopicPosts(options: Options<TopicsGetPostsData>) {
  const query = useQuery(topicsGetPostsOptions(options))

  const posts = computed<PostItem[]>(() => query.data.value?.items ?? [])

  return {
    posts,
    isLoading: query.isLoading,
  }
}
