import { useQuery } from '@tanstack/vue-query'

import { topicsGetLinks } from '../../client/sdk.gen'

import type { TopicLink } from '../../client/types.gen'

export function useTopicLinks(slug: Ref<string>, lang: Ref<string>, enabled: Ref<boolean>) {
  const query = useQuery({
    queryKey: ['topic-links', slug, lang],
    enabled,
    queryFn: async ({ signal }) => {
      const { data } = await topicsGetLinks({
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

  const links = computed<TopicLink[]>(() => query.data.value ?? [])

  return {
    links,
    isLoading: query.isLoading,
  }
}
