import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import {
  meSavedPostsCreateMutation,
  meSavedPostsDeleteMutation,
  meSavedPostsListInfiniteOptions,
  meSavedPostsListInfiniteQueryKey,
} from '~~/generated/api/@tanstack/vue-query.gen'

import {
  type SavedPostTopicCachePatch,
  cancelTopicPostCacheQuery,
  rollbackPostSavedInTopicCache,
  setPostSavedInTopicCache,
} from './savedPostsCache'

import type { Options } from '~~/generated/api/sdk.gen'
import type {
  MeSavedPostsCreateData,
  MeSavedPostsCreateError,
  MeSavedPostsCreateResponse,
  MeSavedPostsDeleteData,
  MeSavedPostsDeleteError,
  MeSavedPostsDeleteResponse,
  SavePostRequest,
} from '~~/generated/api/types.gen'

export interface UseServerSavedPostsOptions {
  enabled: Ref<boolean>
}

export interface ServerSavedPostRequest extends SavePostRequest {
  topicSlug: string
}

interface SavedPostMutationContext {
  patch: SavedPostTopicCachePatch | null
}

const savedPostsPageSize = 10

export const useServerSavedPosts = ({ enabled }: UseServerSavedPostsOptions) => {
  const { lang } = useLang()
  const queryClient = useQueryClient()

  const getTopicSlug = (meta: Record<string, unknown> | undefined) =>
    typeof meta?.topicSlug === 'string' ? meta.topicSlug : undefined

  const query = useInfiniteQuery({
    ...meSavedPostsListInfiniteOptions({ query: { limit: savedPostsPageSize } }),
    enabled,
    initialPageParam: '',
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
  })

  const savedPosts = computed(() => query.data.value?.pages.flatMap((page) => page.items) ?? [])

  const saveMutation = useMutation<
    MeSavedPostsCreateResponse,
    MeSavedPostsCreateError,
    Options<MeSavedPostsCreateData>,
    SavedPostMutationContext
  >({
    ...meSavedPostsCreateMutation(),
    onMutate: async (variables) => {
      const postId = +variables.body.postId
      const topicSlug = getTopicSlug(variables.meta)

      if (!topicSlug) return { patch: null }

      const target = { postId, topicSlug, lang: lang.value }

      await cancelTopicPostCacheQuery(queryClient, target)

      return {
        patch: setPostSavedInTopicCache(queryClient, target, true),
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: meSavedPostsListInfiniteQueryKey() })
    },
    onError(_err, _variables, onMutateResult) {
      rollbackPostSavedInTopicCache(queryClient, onMutateResult?.patch ?? null)
    },
  })

  const removeMutation = useMutation<
    MeSavedPostsDeleteResponse,
    MeSavedPostsDeleteError,
    Options<MeSavedPostsDeleteData>,
    SavedPostMutationContext
  >({
    ...meSavedPostsDeleteMutation(),
    onMutate: async (variables) => {
      const postId = +variables.path.postId
      const topicSlug = getTopicSlug(variables.meta)

      if (!topicSlug) return { patch: null }

      const target = { postId, topicSlug, lang: lang.value }

      await cancelTopicPostCacheQuery(queryClient, target)

      return {
        patch: setPostSavedInTopicCache(queryClient, target, false),
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: meSavedPostsListInfiniteQueryKey() })
    },
    onError(_err, _variables, onMutateResult) {
      rollbackPostSavedInTopicCache(queryClient, onMutateResult?.patch ?? null)
    },
  })

  const save = ({ postId, topicSlug }: ServerSavedPostRequest) =>
    saveMutation.mutateAsync({
      body: { postId },
      meta: { topicSlug },
    })

  const remove = ({ postId, topicSlug }: ServerSavedPostRequest) =>
    removeMutation.mutateAsync({
      path: { postId: +postId },
      meta: { topicSlug },
    })

  const toggle = (request: ServerSavedPostRequest, isSaved: boolean) =>
    isSaved ? remove(request) : save(request)

  return { savedPosts, save, remove, toggle, ...query }
}
