import { useQuery } from '@tanstack/vue-query'

import { topicsGetLinksOptions } from '../../client/@tanstack/vue-query.gen'

import type { TopicLink } from '../../client/types.gen'

export function useTopicLinks(slug: Ref<string>, lang: Ref<string>, enabled: Ref<boolean>) {
  const query = useQuery({
    ...topicsGetLinksOptions({
      path: { slug: slug.value },
      query: { lang: lang.value },
    }),
    enabled,
  })

  const links = computed<TopicLink[]>(() => query.data.value ?? [])

  return {
    links,
    isLoading: query.isLoading,
  }
}
