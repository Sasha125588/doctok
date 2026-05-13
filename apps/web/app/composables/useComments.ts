import {
  commentsReactionsToggleMutation,
  postsCommentsCreateMutation,
  postsCommentsListOptions,
  postsCommentsListQueryKey,
  topicsGetPostsQueryKey,
} from '#api/@tanstack/vue-query.gen'
import { type QueryKey, useMutation, useQuery } from '@tanstack/vue-query'

import type {
  CommentView,
  CommentsReactionsToggleData,
  CommentsReactionsToggleError,
  CommentsReactionsToggleResponse,
  CommentsResponse,
  PostsCommentsCreateData,
  PostsCommentsCreateError,
  PostsCommentsCreateResponse,
  PostsCommentsListResponse,
  ReactionValue,
  TopicsGetPostsResponse,
} from '#api/types.gen'
import type { Options } from '~~/generated/api/sdk.gen'

export interface UseCommentReactionContext {
  commentsQueryKey: QueryKey
  previousData?: CommentsResponse
}

export interface UseCommentCreateContext {
  comments: {
    queryKey: QueryKey
    previousData?: PostsCommentsListResponse
  }
  topicPosts: {
    queryKey: QueryKey
    previousData?: TopicsGetPostsResponse
  }
}

export const useComments = (postId: Ref<number>, topicSlug: Ref<string>, enabled: Ref<boolean>) => {
  const { postsContentLang } = useLang()
  const getCommentsQueryKey = () => postsCommentsListQueryKey({ path: { postId: postId.value } })
  const getTopicPostsQueryKey = () =>
    topicsGetPostsQueryKey({
      query: {
        slug: topicSlug.value,
        lang: postsContentLang.value,
      },
    })

  const queryOptions = computed(() => ({
    path: { postId: postId.value },
  }))

  const query = useQuery(() => ({ enabled, ...postsCommentsListOptions(queryOptions.value) }))

  const createMutation = useMutation<
    PostsCommentsCreateResponse,
    PostsCommentsCreateError,
    Options<PostsCommentsCreateData>,
    UseCommentCreateContext
  >({
    ...postsCommentsCreateMutation(),
    onMutate: async (variables, context) => {
      const postId = variables.path.postId
      const commentsQueryKey = getCommentsQueryKey()
      const topicPostsQueryKey = getTopicPostsQueryKey()

      await context.client.cancelQueries({ queryKey: commentsQueryKey })
      await context.client.cancelQueries({ queryKey: topicPostsQueryKey })

      const commentsPreviousData =
        context.client.getQueryData<PostsCommentsListResponse>(commentsQueryKey)

      const topicPostsPreviousData =
        context.client.getQueryData<TopicsGetPostsResponse>(topicPostsQueryKey)

      const newComment: CommentView = {
        id: `optimistic-${Date.now()}`,
        postId,
        userId: '',
        parentCommentId: null,
        body: variables.body.body,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        deletedAt: null,
        likeCount: 0,
        dislikeCount: 0,
        replyCount: 0,
        myVote: 'none',
      }

      context.client.setQueryData<PostsCommentsListResponse>(commentsQueryKey, (oldData) => {
        if (!oldData) return oldData

        return {
          ...oldData,
          items: [newComment, ...oldData.items],
        }
      })

      context.client.setQueryData<TopicsGetPostsResponse>(topicPostsQueryKey, (oldData) => {
        if (!oldData) return oldData

        return {
          ...oldData,
          items: oldData.items.map((post) => {
            if (+post.id === postId) return { ...post, commentCount: +post.commentCount + 1 }

            return post
          }),
        }
      })

      return {
        comments: {
          queryKey: commentsQueryKey,
          previousData: commentsPreviousData,
        },
        topicPosts: {
          queryKey: topicPostsQueryKey,
          previousData: topicPostsPreviousData,
        },
      }
    },
    onSuccess: (_data, _variables, onMutateResult, context) => {
      context.client.invalidateQueries({
        queryKey: onMutateResult.comments.queryKey,
      })
      context.client.invalidateQueries({
        queryKey: onMutateResult.topicPosts.queryKey,
      })
    },
    onError: (_data, _variables, onMutateResult, context) => {
      if (!onMutateResult) return

      if (onMutateResult.topicPosts.previousData) {
        context.client.setQueryData(
          onMutateResult.topicPosts.queryKey,
          onMutateResult.topicPosts.previousData
        )
      }

      if (onMutateResult.comments.previousData) {
        context.client.setQueryData(
          onMutateResult.comments.queryKey,
          onMutateResult.comments.previousData
        )
      }
    },
  })

  const reactionMutation = useMutation<
    CommentsReactionsToggleResponse,
    CommentsReactionsToggleError,
    Options<CommentsReactionsToggleData>,
    UseCommentReactionContext
  >({
    ...commentsReactionsToggleMutation(),
    onMutate: async (variables, context) => {
      const commentId = variables.path.commentId
      const nextReaction = variables.body.value
      const commentsQueryKey = getCommentsQueryKey()

      await context.client.cancelQueries({ queryKey: commentsQueryKey })

      const previousData = context.client.getQueryData<CommentsResponse>(commentsQueryKey)

      context.client.setQueryData<CommentsResponse>(commentsQueryKey, (oldData) => {
        if (!oldData) return oldData

        return {
          ...oldData,
          items: oldData.items.map((comment) => {
            if (+comment.id !== commentId) return comment

            const prev = comment.myVote

            let likeCount = +comment.likeCount
            let dislikeCount = +comment.dislikeCount
            let myVote = prev

            if (nextReaction === 'like') {
              if (prev === 'like') {
                likeCount--
                myVote = 'none'
              } else {
                likeCount++
                if (prev === 'dislike') dislikeCount--
                myVote = 'like'
              }
            }

            if (nextReaction === 'dislike') {
              if (prev === 'dislike') {
                dislikeCount--
                myVote = 'none'
              } else {
                dislikeCount++
                if (prev === 'like') likeCount--
                myVote = 'dislike'
              }
            }

            return {
              ...comment,
              likeCount,
              dislikeCount,
              myVote,
            }
          }),
        }
      })

      return { previousData, commentsQueryKey }
    },
    onSuccess(data, variables, onMutateResult, context) {
      const commentId = variables.path.commentId

      context.client.setQueryData<CommentsResponse>(onMutateResult.commentsQueryKey, (oldData) => {
        if (!oldData) return oldData

        return {
          ...oldData,
          items: oldData.items.map((comment) => {
            if (+comment.id !== commentId) return comment

            return {
              ...comment,
              likeCount: data.likeCount,
              dislikeCount: data.dislikeCount,
              myVote: data.myVote,
            }
          }),
        }
      })
    },
    onError(_error, _variables, onMutateResult, context) {
      if (!onMutateResult?.previousData) return

      context.client.setQueryData(onMutateResult.commentsQueryKey, onMutateResult.previousData)
    },
  })

  const send = (body: string, onSuccess?: () => void) => {
    if (postId.value == null || !body.trim()) return
    createMutation.mutate(
      {
        path: { postId: postId.value },
        body: { body: body.trim() },
      },
      { onSuccess }
    )
  }

  const vote = (commentId: number, value: ReactionValue) => {
    if (reactionMutation.isPending.value) return

    reactionMutation.mutate({
      path: { commentId: +commentId },
      body: { value },
    })
  }

  return {
    comments: computed(() => query.data.value?.items ?? []),
    isLoading: query.isLoading,
    isSending: createMutation.isPending,
    send,
    vote,
  }
}
