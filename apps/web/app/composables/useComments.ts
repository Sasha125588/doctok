import {
  postsCommentsCreateMutation,
  postsCommentsListOptions,
  postsCommentsListQueryKey,
  topicsGetPostsQueryKey,
} from '#api/@tanstack/vue-query.gen'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'

import type { TopicsGetPostsResponse } from '~~/generated/api/types.gen'

export function useComments(postId: Ref<number>, topicSlug: Ref<string>) {
  const queryClient = useQueryClient()
  const { lang } = useLang()

  const listOptions = computed(() => ({
    ...postsCommentsListOptions({
      path: { postId: postId.value ?? 0 },
    }),
  }))

  const query = useQuery(listOptions)

  const createMutation = useMutation({
    ...postsCommentsCreateMutation(),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: postsCommentsListQueryKey({ path: { postId: variables.path.postId } }),
      })

      queryClient.setQueryData<TopicsGetPostsResponse>(
        topicsGetPostsQueryKey({
          query: {
            slug: topicSlug.value,
            lang: lang.value,
          },
        }),
        (oldData) => {
          if (!oldData) return oldData

          return {
            ...oldData,
            items: oldData.items.map((post) =>
              +post.id === postId.value ? { ...post, commentCount: +post.commentCount + 1 } : post
            ),
          }
        }
      )
    },
  })

  function send(body: string, onSuccess?: () => void) {
    if (postId.value == null || !body.trim()) return
    createMutation.mutate(
      {
        path: { postId: postId.value },
        body: { body: body.trim() },
      },
      { onSuccess }
    )
  }

  return {
    comments: computed(() => query.data.value ?? []),
    isLoading: query.isLoading,
    isSending: createMutation.isPending,
    send,
  }
}
