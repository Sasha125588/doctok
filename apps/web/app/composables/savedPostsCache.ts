import { topicsGetPostsQueryKey } from '~~/generated/api/@tanstack/vue-query.gen'

import type { QueryClient } from '@tanstack/vue-query'
import type { TopicsGetPostsResponse } from '~~/generated/api/types.gen'

export interface SavedPostTopicCacheTarget {
  postId: number
  topicSlug: string
  lang: string
}

export interface SavedPostTopicCachePatch extends SavedPostTopicCacheTarget {
  previousIsSaved: boolean
}

const getTopicPostsQueryKey = (target: Omit<SavedPostTopicCacheTarget, 'postId'>) =>
  topicsGetPostsQueryKey({
    query: {
      slug: target.topicSlug,
      lang: target.lang,
    },
  })

const setPostSavedInTopicData = (
  data: TopicsGetPostsResponse | undefined,
  postId: number,
  isSaved: boolean
): TopicsGetPostsResponse | undefined => {
  if (!data) return data

  return {
    ...data,
    items: data.items.map((post) =>
      +post.id === postId
        ? {
            ...post,
            isSaved,
          }
        : post
    ),
  }
}

export const cancelTopicPostCacheQuery = (
  queryClient: QueryClient,
  target: SavedPostTopicCacheTarget
) =>
  queryClient.cancelQueries({
    queryKey: getTopicPostsQueryKey(target),
  })

export const setPostSavedInTopicCache = (
  queryClient: QueryClient,
  target: SavedPostTopicCacheTarget,
  isSaved: boolean
) => {
  const queryKey = getTopicPostsQueryKey(target)
  const data = queryClient.getQueryData<TopicsGetPostsResponse>(queryKey)
  const post = data?.items.find((item) => +item.id === target.postId)

  if (!post) return null

  queryClient.setQueryData<TopicsGetPostsResponse>(queryKey, (oldData) =>
    setPostSavedInTopicData(oldData, target.postId, isSaved)
  )

  return {
    ...target,
    previousIsSaved: post.isSaved,
  }
}

export const rollbackPostSavedInTopicCache = (
  queryClient: QueryClient,
  patch: SavedPostTopicCachePatch | null
) => {
  if (!patch) return

  const queryKey = getTopicPostsQueryKey(patch)

  queryClient.setQueryData<TopicsGetPostsResponse>(queryKey, (oldData) =>
    setPostSavedInTopicData(oldData, patch.postId, patch.previousIsSaved)
  )
}
