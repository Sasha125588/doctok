import { postsReactionsToggleMutation, topicsGetPostsQueryKey } from '#api/@tanstack/vue-query.gen'
import { useMutation, useQueryClient } from '@tanstack/vue-query'

import type { ReactionValue, TopicsGetPostsResponse } from '#api/types.gen'

export interface useVoteOptions {
  postId: number
  topicSlug: string
}

export interface VoteContext {
  previousData?: TopicsGetPostsResponse
}

export const useVote = (options: useVoteOptions) => {
  const { lang } = useLang()

  const queryKey = topicsGetPostsQueryKey({
    query: { slug: options.topicSlug, lang: lang.value },
  })

  const queryClient = useQueryClient()

  const voteMutation = useMutation({
    ...postsReactionsToggleMutation(),

    onMutate: async (variables) => {
      const postId = variables.path.postId
      const nextVote = variables.body.value

      await queryClient.cancelQueries({ queryKey })

      const previousData = queryClient.getQueryData<TopicsGetPostsResponse>(queryKey)

      queryClient.setQueryData<TopicsGetPostsResponse>(queryKey, (oldData) => {
        if (!oldData) return oldData

        return {
          ...oldData,
          items: oldData.items.map((post) => {
            if (post.id !== postId) return post

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

      return { previousData }
    },

    onSuccess(data, variables) {
      const postId = variables.path.postId

      queryClient.setQueryData<TopicsGetPostsResponse>(queryKey, (oldData) => {
        if (!oldData) return oldData

        return {
          ...oldData,
          items: oldData.items.map((post) =>
            post.id === postId
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

    onError(_err, _variables, onMutateResult) {
      if (!onMutateResult?.previousData) return

      queryClient.setQueryData(queryKey, onMutateResult.previousData)
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
