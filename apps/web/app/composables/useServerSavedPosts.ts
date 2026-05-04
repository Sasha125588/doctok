import { type QueryKey, useInfiniteQuery, useMutation } from '@tanstack/vue-query'
import { createSharedComposable } from '@vueuse/core'
import {
  meSavedPostsCreateMutation,
  meSavedPostsDeleteMutation,
  meSavedPostsListInfiniteOptions,
  meSavedPostsListInfiniteQueryKey,
  topicsGetPostsQueryKey,
} from '~~/generated/api/@tanstack/vue-query.gen'

import type { Options } from '~~/generated/api/sdk.gen'
import type {
  MeSavedPostsDeleteData,
  MeSavedPostsDeleteError,
  MeSavedPostsDeleteResponse,
  SavePostRequest,
  TopicsGetPostsResponse,
} from '~~/generated/api/types.gen'

export interface SavedPostMutationContext {
  queryKey?: QueryKey
  previousData?: TopicsGetPostsResponse
}

export interface ServerSavedPostRequest extends SavePostRequest {
  topicSlug: string
}

const savedPostsPageSize = 10

function getTopicSlugFromMeta(meta: Record<string, unknown> | undefined) {
  const raw = meta?.topicSlug
  return typeof raw === 'string' ? raw.trim() : ''
}

function topicsListQueryKey() {
  return meSavedPostsListInfiniteQueryKey({
    query: { limit: savedPostsPageSize },
  })
}

function useServerSavedPostsImpl() {
  const session = useSession()
  const { lang } = useLang()

  const enabled = computed(() => session.isSuccess.value && Boolean(session.data.value?.userId))

  const query = useInfiniteQuery({
    ...meSavedPostsListInfiniteOptions({
      query: { limit: savedPostsPageSize },
    }),
    enabled,
    initialPageParam: '',
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
  })

  const savedPosts = computed(() => query.data.value?.pages.flatMap((page) => page.items) ?? [])

  const saveMutation = useMutation({
    ...meSavedPostsCreateMutation(),
    onMutate: async (variables, context) => {
      const postId = +variables.body.postId
      const topicSlug = getTopicSlugFromMeta(variables.meta)
      if (!topicSlug) return {}

      const queryKey = topicsGetPostsQueryKey({
        query: { slug: topicSlug, lang: lang.value },
      })

      await context.client.cancelQueries({ queryKey })

      const previousData = context.client.getQueryData<TopicsGetPostsResponse>(queryKey)

      context.client.setQueryData<TopicsGetPostsResponse>(queryKey, (oldData) => {
        if (!oldData) return oldData

        return {
          ...oldData,
          items: oldData.items.map((post) => {
            if (+post.id !== postId) return post

            return {
              ...post,
              isSaved: true,
            }
          }),
        }
      })

      return {
        previousData,
        queryKey,
      }
    },
    onSuccess: (_data, _variables, _onMutateResult, context) => {
      context.client.invalidateQueries({ queryKey: topicsListQueryKey() })
    },
    onError: (_err, _variables, onMutateResult, context) => {
      if (!onMutateResult?.queryKey) return

      context.client.setQueryData(onMutateResult.queryKey, onMutateResult.previousData)
    },
  })

  const removeMutation = useMutation<
    MeSavedPostsDeleteResponse,
    MeSavedPostsDeleteError,
    Options<MeSavedPostsDeleteData>,
    SavedPostMutationContext
  >({
    ...meSavedPostsDeleteMutation(),
    onMutate: async (variables, context) => {
      const postId = +variables.path.postId
      const topicSlug = getTopicSlugFromMeta(variables.meta)
      if (!topicSlug) return {}

      const queryKey = topicsGetPostsQueryKey({
        query: { slug: topicSlug, lang: lang.value },
      })

      await context.client.cancelQueries({ queryKey })

      const previousData = context.client.getQueryData<TopicsGetPostsResponse>(queryKey)

      context.client.setQueryData<TopicsGetPostsResponse>(queryKey, (oldData) => {
        if (!oldData) return oldData

        return {
          ...oldData,
          items: oldData.items.map((post) => {
            if (+post.id !== postId) return post

            return {
              ...post,
              isSaved: false,
            }
          }),
        }
      })

      return {
        previousData,
        queryKey,
      }
    },
    onSuccess: (_data, _variables, _onMutateResult, context) => {
      context.client.invalidateQueries({ queryKey: topicsListQueryKey() })
    },

    onError: (_err, _variables, onMutateResult, context) => {
      if (!onMutateResult?.queryKey) return

      context.client.setQueryData(onMutateResult.queryKey, onMutateResult.previousData)
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

export const useServerSavedPosts = createSharedComposable(useServerSavedPostsImpl)
