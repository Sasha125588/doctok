import {
  postsCommentsCreateMutation,
  postsCommentsListOptions,
  postsCommentsListQueryKey,
} from '#api/@tanstack/vue-query.gen'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'

export function useComments(postId: Ref<number | null>) {
  const enabled = computed(() => postId.value != null)
  const queryClient = useQueryClient()

  const listOptions = computed(() => ({
    ...postsCommentsListOptions({
      path: { postId: postId.value ?? 0 },
    }),
    enabled: enabled.value,
  }))

  const query = useQuery(() => listOptions.value)

  const createMutation = useMutation({
    ...postsCommentsCreateMutation(),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: postsCommentsListQueryKey({ path: { postId: variables.path.postId } }),
      })
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
