import { postsReactionsToggleMutation, topicsGetPostsQueryKey } from '#api/@tanstack/vue-query.gen'
import { useMutation } from '@tanstack/vue-query'

import type { ReactionValue, TopicsGetPostsResponse } from '#api/types.gen'

export interface UseVoteOptions {
  postId: number
  topicSlug: string
}

export interface UseVoteContext {
  queryKey: ReturnType<typeof topicsGetPostsQueryKey>
  previousData?: TopicsGetPostsResponse
}

export const useVote = (options: UseVoteOptions) => {
  const { lang } = useLang()

  const getQueryKey = () =>
    topicsGetPostsQueryKey({
      query: { slug: options.topicSlug, lang: lang.value },
    })

  const voteMutation = useMutation({
    ...postsReactionsToggleMutation(),

    onMutate: async (variables, context) => {
      const queryKey = getQueryKey()

      const postId = variables.path.postId
      const nextVote = variables.body.value

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

            if (nextVote === 'like') {
              if (prev === 'like') {
                likeCount--
                myVote = 'none'
              } else {
                likeCount++
                if (prev === 'dislike') dislikeCount--
                myVote = 'like'
              }
            }

            if (nextVote === 'dislike') {
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

    onSuccess(data, variables, _onMutateResult, context) {
      const queryKey = getQueryKey()

      const postId = variables.path.postId

      context.client.setQueryData<TopicsGetPostsResponse>(queryKey, (oldData) => {
        if (!oldData) return oldData

        return {
          ...oldData,
          items: oldData.items.map((post) =>
            +post.id === postId
              ? {
                  ...post,
                  likeCount: data.likeCount,
                  dislikeCount: data.dislikeCount,
                  myVote: data.myVote,
                }
              : post
          ),
        }
      })
    },
    onError(_err, _variables, onMutateResult, context) {
      if (!onMutateResult?.previousData || !onMutateResult.queryKey) return

      context.client.setQueryData(onMutateResult.queryKey, onMutateResult.previousData)
    },
  })

  const onVote = (value: ReactionValue) =>
    voteMutation.mutate({
      path: { postId: options.postId },
      body: { value },
    })

  return {
    functions: {
      onVote,
    },
  }
}
