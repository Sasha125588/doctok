import {
  type InfiniteData,
  type Query,
  type QueryKey,
  useInfiniteQuery,
  useMutation,
} from '@tanstack/vue-query'
import { createSharedComposable } from '@vueuse/core'
import {
  meSavedPostsClearMutation,
  meSavedPostsCreateMutation,
  meSavedPostsDeleteMutation,
  meSavedPostsListInfiniteOptions,
  meSavedPostsListInfiniteQueryKey,
  topicsGetPostsQueryKey,
} from '~~/generated/api/@tanstack/vue-query.gen'

import type { Options } from '~~/generated/api/sdk.gen'
import type {
  MeSavedPostsClearData,
  MeSavedPostsClearError,
  MeSavedPostsClearResponse,
  MeSavedPostsCreateData,
  MeSavedPostsCreateError,
  MeSavedPostsCreateResponse,
  MeSavedPostsDeleteData,
  MeSavedPostsDeleteError,
  MeSavedPostsDeleteResponse,
  MeSavedPostsListResponse,
  SavePostRequest,
  SavedPostView,
  TopicsGetPostsResponse,
} from '~~/generated/api/types.gen'

type SavedPostsInfiniteData = InfiniteData<MeSavedPostsListResponse>

export interface SavedPostMutationContext {
  topicPosts: {
    queryKey: QueryKey
    previousData?: TopicsGetPostsResponse
  }
  savedPosts: {
    queryKey: QueryKey
    previousData?: SavedPostsInfiniteData
  }
}

export interface ServerSavedPostRequest extends SavePostRequest {
  topicSlug: string
  savedPost?: SavedPostView
}

const savedPostsPageSize = 10

const savedPostsListQueryKey = () =>
  meSavedPostsListInfiniteQueryKey({
    query: { limit: savedPostsPageSize },
  })

const useServerSavedPostsImpl = () => {
  const session = useSession()
  const { postsContentLang } = useLang()

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

  const saveMutation = useMutation<
    MeSavedPostsCreateResponse,
    MeSavedPostsCreateError,
    Options<MeSavedPostsCreateData>,
    SavedPostMutationContext
  >({
    ...meSavedPostsCreateMutation(),
    onMutate: async (variables, context) => {
      const postId = +variables.body.postId
      const topicSlug = variables.meta?.topicSlug as string
      const savedPost = variables.meta?.savedPost as SavedPostView

      const topicPostsQueryKey = topicsGetPostsQueryKey({
        query: { slug: topicSlug, lang: postsContentLang.value },
      })
      const savedPostsQueryKey = savedPostsListQueryKey()

      await Promise.all([
        context.client.cancelQueries({ queryKey: topicPostsQueryKey }),
        context.client.cancelQueries({ queryKey: savedPostsQueryKey }),
      ])

      const previousTopicPostsData =
        context.client.getQueryData<TopicsGetPostsResponse>(topicPostsQueryKey)
      const previousSavedPostsData =
        context.client.getQueryData<SavedPostsInfiniteData>(savedPostsQueryKey)

      context.client.setQueryData<TopicsGetPostsResponse>(topicPostsQueryKey, (oldData) =>
        markTopicPostSaved(oldData, postId, true)
      )
      context.client.setQueryData<SavedPostsInfiniteData>(savedPostsQueryKey, (oldData) =>
        addSavedPostToList(oldData, savedPost)
      )

      return {
        topicPosts: {
          previousData: previousTopicPostsData,
          queryKey: topicPostsQueryKey,
        },
        savedPosts: {
          previousData: previousSavedPostsData,
          queryKey: savedPostsQueryKey,
        },
      }
    },
    onSuccess: (_data, _variables, _onMutateResult, context) => {
      context.client.invalidateQueries({ queryKey: savedPostsListQueryKey() })
    },
    onError: (_err, _variables, onMutateResult, context) => {
      if (onMutateResult?.topicPosts) {
        context.client.setQueryData(
          onMutateResult.topicPosts.queryKey,
          onMutateResult.topicPosts.previousData
        )
      }

      if (onMutateResult?.savedPosts) {
        context.client.setQueryData(
          onMutateResult.savedPosts.queryKey,
          onMutateResult.savedPosts.previousData
        )
      }
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
      const topicSlug = variables.meta?.topicSlug as string

      const topicPostsQueryKey = topicsGetPostsQueryKey({
        query: { slug: topicSlug, lang: postsContentLang.value },
      })
      const savedPostsQueryKey = savedPostsListQueryKey()

      await Promise.all([
        context.client.cancelQueries({ queryKey: topicPostsQueryKey }),
        context.client.cancelQueries({ queryKey: savedPostsQueryKey }),
      ])

      const previousTopicPostsData =
        context.client.getQueryData<TopicsGetPostsResponse>(topicPostsQueryKey)
      const previousSavedPostsData =
        context.client.getQueryData<SavedPostsInfiniteData>(savedPostsQueryKey)

      context.client.setQueryData<TopicsGetPostsResponse>(topicPostsQueryKey, (oldData) =>
        markTopicPostSaved(oldData, postId, false)
      )
      context.client.setQueryData<SavedPostsInfiniteData>(savedPostsQueryKey, (oldData) =>
        removeSavedPostFromList(oldData, postId)
      )

      return {
        topicPosts: {
          previousData: previousTopicPostsData,
          queryKey: topicPostsQueryKey,
        },
        savedPosts: {
          previousData: previousSavedPostsData,
          queryKey: savedPostsQueryKey,
        },
      }
    },
    onSuccess: (_data, _variables, _onMutateResult, context) => {
      context.client.invalidateQueries({ queryKey: savedPostsListQueryKey() })
    },
    onError: (_err, _variables, onMutateResult, context) => {
      if (onMutateResult?.topicPosts.previousData) {
        context.client.setQueryData(
          onMutateResult.topicPosts.queryKey,
          onMutateResult.topicPosts.previousData
        )
      }

      if (onMutateResult?.savedPosts.previousData) {
        context.client.setQueryData(
          onMutateResult.savedPosts.queryKey,
          onMutateResult.savedPosts.previousData
        )
      }
    },
  })

  const isTopicsGetPostsQuery = (query: Query) =>
    (query.queryKey[0] as { _id: string })._id === 'topicsGetPosts'

  const clearMutation = useMutation<
    MeSavedPostsClearResponse,
    MeSavedPostsClearError,
    Options<MeSavedPostsClearData>
  >({
    ...meSavedPostsClearMutation(),
    onSuccess: (_data, _variables, _onMutateResult, context) => {
      context.client.invalidateQueries({ queryKey: savedPostsListQueryKey() })

      context.client.setQueriesData<TopicsGetPostsResponse>(
        { predicate: isTopicsGetPostsQuery },
        (oldData) => {
          if (!oldData) return oldData

          return {
            ...oldData,
            items: oldData.items.map((post) => ({
              ...post,
              isSaved: false,
            })),
          }
        }
      )

      context.client.invalidateQueries({
        predicate: isTopicsGetPostsQuery,
      })
    },
  })

  const save = ({ postId, topicSlug, savedPost }: ServerSavedPostRequest) =>
    saveMutation.mutateAsync({
      body: { postId },
      meta: { savedPost, topicSlug },
    })

  const remove = ({ postId, topicSlug }: ServerSavedPostRequest) =>
    removeMutation.mutateAsync({
      path: { postId: +postId },
      meta: { topicSlug },
    })

  const toggle = (request: ServerSavedPostRequest, isSaved: boolean) =>
    isSaved ? remove(request) : save(request)

  const clear = () => clearMutation.mutateAsync({})

  const isClearing = computed(() => clearMutation.isPending.value)

  return { savedPosts, save, remove, toggle, clear, isClearing, ...query }
}

const markTopicPostSaved = (
  oldData: TopicsGetPostsResponse | undefined,
  postId: number,
  isSaved: boolean
) => {
  if (!oldData) return oldData

  return {
    ...oldData,
    items: oldData.items.map((post) => {
      if (+post.id !== postId) return post

      return {
        ...post,
        isSaved,
      }
    }),
  }
}

const removeSavedPostFromList = (oldData: SavedPostsInfiniteData | undefined, postId: number) => {
  if (!oldData) return oldData

  return {
    ...oldData,
    pages: oldData.pages.map((page) => ({
      ...page,
      items: page.items.filter((post) => +post.postId !== postId),
    })),
  }
}

const addSavedPostToList = (
  oldData: SavedPostsInfiniteData | undefined,
  savedPost: SavedPostView
) => {
  if (!oldData) return oldData

  const hasPost = oldData.pages.some((page) =>
    page.items.some((post) => +post.postId === +savedPost.postId)
  )
  const [firstPage, ...restPages] = oldData.pages

  if (hasPost || !firstPage) return oldData

  return {
    ...oldData,
    pages: [
      {
        ...firstPage,
        items: [savedPost, ...firstPage.items],
      },
      ...restPages,
    ],
  }
}

export const useServerSavedPosts = createSharedComposable(useServerSavedPostsImpl)
