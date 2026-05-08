import { topicsGetLinksOptions } from '#api/@tanstack/vue-query.gen'
import { type Options } from '#api/sdk.gen'
import { useQuery } from '@tanstack/vue-query'

import type { TopicLink, TopicsGetLinksData } from '#api/types.gen'

export function useTopicLinks(options: Ref<Options<TopicsGetLinksData>>, enabled: Ref<boolean>) {
  const query = useQuery(() => ({
    enabled,
    ...topicsGetLinksOptions(options.value),
  }))

  const links = computed<TopicLink[]>(() => query.data.value ?? [])

  return {
    links,
    isLoading: query.isLoading,
  }
}
