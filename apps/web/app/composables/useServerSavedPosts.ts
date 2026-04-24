import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import {
  meSavedPostsCreateMutation,
  meSavedPostsDeleteMutation,
  meSavedPostsListInfiniteOptions,
  meSavedPostsListInfiniteQueryKey,
} from '~~/generated/api/@tanstack/vue-query.gen'

import type { SavePostRequest } from '~~/generated/api/types.gen'

const savedPostsPageSize = 10

export const useServerSavedPosts = ({ enabled }: { enabled: Ref<boolean> }) => {
  const queryClient = useQueryClient()

  const query = useInfiniteQuery({
    ...meSavedPostsListInfiniteOptions({ query: { limit: savedPostsPageSize } }),
    enabled,
    initialPageParam: '',
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
  })

  const savedPosts = computed(() => query.data.value?.pages.flatMap((page) => page.items) ?? [])

  const saveMutation = useMutation({
    ...meSavedPostsCreateMutation(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: meSavedPostsListInfiniteQueryKey() })
    },
  })

  const removeMutation = useMutation({
    ...meSavedPostsDeleteMutation(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: meSavedPostsListInfiniteQueryKey() })
    },
  })

  const save = (request: SavePostRequest) =>
    saveMutation.mutateAsync({
      body: request,
    })

  const remove = (postId: number) =>
    removeMutation.mutateAsync({
      path: { postId },
    })

  const toggle = (postId: number, isSaved: boolean) => (isSaved ? remove(postId) : save({ postId }))

  return { savedPosts, save, remove, toggle, ...query }
}
