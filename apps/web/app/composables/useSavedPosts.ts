import { useGuestSavedPosts } from './useGuestSavedPosts'
import { useServerSavedPosts } from './useServerSavedPosts'

import type { TopicPostView } from '~~/generated/api/types.gen'

export const useSavedPosts = () => {
  const session = useSession()

  const guest = useGuestSavedPosts()

  const isAuthenticated = computed(
    () => session.isSuccess.value && Boolean(session.data.value?.userId)
  )

  const server = useServerSavedPosts({
    enabled: isAuthenticated,
  })

  const savedPosts = computed(() =>
    isAuthenticated.value ? server.savedPosts.value : guest.savedPosts.value
  )

  const isSaved = (postId: number, isSaved: boolean) => {
    if (isAuthenticated.value) {
      return isSaved
    }

    return guest.isSaved(postId)
  }

  const save = async (post: TopicPostView) => {
    if (isAuthenticated.value) {
      await server.save({ postId: post.id })
      return
    }

    guest.save(post)
  }

  const remove = async (postId: number) => {
    if (isAuthenticated.value) {
      await server.remove(postId)
      return
    }

    guest.remove(postId)
  }

  const toggle = async (post: TopicPostView) => {
    if (isAuthenticated.value) {
      await server.toggle(+post.id, post.isSaved)
      return
    }

    guest.toggle(post)
  }

  const clear = async () => {
    // if (isAuthenticated.value) {
    //   await server.remove(postId)
    //   return
    // }

    guest.clear()
  }

  return { savedPosts, isSaved, save, remove, toggle, clear }
}
