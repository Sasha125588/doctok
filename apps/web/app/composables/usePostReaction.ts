import { postsReactionsToggleMutation, topicsGetPostsQueryKey } from '#api/@tanstack/vue-query.gen'
import { type QueryKey, useMutation } from '@tanstack/vue-query'

import type {
  PostsReactionsToggleData,
  PostsReactionsToggleError,
  PostsReactionsToggleResponse,
  ReactionValue,
  TopicsGetPostsResponse,
} from '#api/types.gen'
import type { Options } from '~~/generated/api/sdk.gen'

export interface PostReactionMutationContext {
  queryKey: QueryKey
  previousData?: TopicsGetPostsResponse
}

export const usePostReaction = (topicSlug: string) => {
  const { postsContentLang } = useLang()

  const queryKey = topicsGetPostsQueryKey({
    query: { slug: topicSlug, lang: postsContentLang.value },
  })

  const postReactionMutation = useMutation<
    PostsReactionsToggleResponse,
    PostsReactionsToggleError,
    Options<PostsReactionsToggleData>,
    PostReactionMutationContext
  >({
    ...postsReactionsToggleMutation(),

    onMutate: async (variables, context) => {
      const postId = variables.path.postId
      const nextReaction = variables.body.value

      await context.client.cancelQueries({ queryKey })

      const previousData = context.client.getQueryData<TopicsGetPostsResponse>(queryKey)

      context.client.setQueryData<TopicsGetPostsResponse>(queryKey, (oldData) => {
        if (!oldData) return oldData

        return {
          ...oldData,
          items: oldData.items.map((post) => {
            if (+post.id !== postId) return post

            const prev = post.myVote

            let likeCount = +post.likeCount
            let dislikeCount = +post.dislikeCount
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
              ...post,
              likeCount,
              dislikeCount,
              myVote,
            }
          }),
        }
      })

      return { previousData, queryKey }
    },
    onSuccess(data, variables, onMutateResult, context) {
      const postId = variables.path.postId

      context.client.setQueryData<TopicsGetPostsResponse>(onMutateResult.queryKey, (oldData) => {
        if (!oldData) return oldData

        return {
          ...oldData,
          items: oldData.items.map((post) => {
            if (+post.id !== postId) return post

            return {
              ...post,
              likeCount: data.likeCount,
              dislikeCount: data.dislikeCount,
              myVote: data.myVote,
            }
          }),
        }
      })
    },
    onError(_err, _variables, onMutateResult, context) {
      if (!onMutateResult?.previousData) return

      context.client.setQueryData(onMutateResult.queryKey, onMutateResult.previousData)
    },
  })

  const mutatePostReaction = (postId: number, value: ReactionValue) =>
    postReactionMutation.mutate({
      path: { postId },
      body: { value },
    })

  return {
    mutatePostReaction,
  }
}
