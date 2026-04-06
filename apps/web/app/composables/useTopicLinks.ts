import { topicsGetLinksOptions } from '#api/@tanstack/vue-query.gen'
import { type Options } from '#api/sdk.gen'
import { useQuery } from '@tanstack/vue-query'

import type { TopicLink, TopicsGetLinksData } from '#api/types.gen'

export function useTopicLinks(options: Options<TopicsGetLinksData>) {
  const query = useQuery(topicsGetLinksOptions(options))

  const links = computed<TopicLink[]>(() => query.data.value ?? [])

  return {
    links,
    isLoading: query.isLoading,
  }
}
