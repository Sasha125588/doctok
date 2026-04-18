import { topicsGetLinksOptions } from '#api/@tanstack/vue-query.gen'
import { type Options } from '#api/sdk.gen'
import { useQuery } from '@tanstack/vue-query'

import type { TopicLink, TopicsGetLinksData } from '#api/types.gen'

export function useTopicLinks(options: Options<TopicsGetLinksData>, enabled: Ref<boolean>) {
  const query = useQuery({ ...topicsGetLinksOptions(options), enabled })

  const links = computed<TopicLink[]>(() => query.data.value ?? [])

  return {
    state: {
      links,
      isLoading: query.isLoading,
    },
  }
}
