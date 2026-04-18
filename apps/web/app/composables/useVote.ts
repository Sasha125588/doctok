import { postsReactionsToggleMutation, topicsGetPostsQueryKey } from '#api/@tanstack/vue-query.gen'
import { useMutation, useQueryClient } from '@tanstack/vue-query'

import type { ReactionValue, TopicsGetPostsResponse } from '#api/types.gen'

export interface useVoteProps {
  postId: number
  topicSlug: string
  localMyVote: MaybeRefOrGetter<ReactionValue>
  localLikeCount: MaybeRefOrGetter<number>
  localDislikeCount: MaybeRefOrGetter<number>
}

export const useVote = (props: useVoteProps) => {
  const { lang } = useLang()

  const queryClient = useQueryClient()

  const voteMutation = useMutation(postsReactionsToggleMutation())

  const onVote = (value: ReactionValue) =>
    voteMutation.mutate(
      {
        path: { postId: props.postId },
        body: { value },
      },
      {
        onSuccess(data) {
          props.localMyVote = data.myVote
          props.localLikeCount = +data.likeCount
          props.localDislikeCount = +data.dislikeCount

          queryClient.setQueryData(
            topicsGetPostsQueryKey({
              query: {
                slug: props.topicSlug,
                lang: lang.value,
              },
            }),
            (oldData: TopicsGetPostsResponse | undefined) => {
              if (!oldData) return oldData
              const targetId = props.postId

              const changes = {
                likeCount: data.likeCount,
                dislikeCount: data.dislikeCount,
                myVote: data.myVote,
              }

              return {
                ...oldData,
                items: oldData.items.map((post) =>
                  post.id === targetId ? { ...post, ...changes } : post
                ),
              }
            }
          )
        },
        onError(error) {
          console.error(`${value} Error:`, error)
        },
      }
    )

  return {
    functions: {
      onVote,
    },
  }
}
