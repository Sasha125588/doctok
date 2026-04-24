import { useGuestSavedPosts } from './useGuestSavedPosts'
import { useServerSavedPosts } from './useServerSavedPosts'

import type { SavedPostView, TopicPostView } from '~~/generated/api/types.gen'

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

  const isSaved = (post: TopicPostView) => {
    if (isAuthenticated.value) {
      return post.isSaved
    }

    return guest.isSaved(+post.id)
  }

  const save = async (post: TopicPostView) => {
    if (isAuthenticated.value) {
      await server.save({ postId: post.id, topicSlug: post.topicSlug })
      return
    }

    guest.save(post)
  }

  const remove = async (post: SavedPostView) => {
    if (isAuthenticated.value) {
      await server.remove({ postId: post.postId, topicSlug: post.topicSlug })
      return
    }

    guest.remove(+post.postId)
  }

  const toggle = async (post: TopicPostView) => {
    if (isAuthenticated.value) {
      await server.toggle({ postId: post.id, topicSlug: post.topicSlug }, post.isSaved)
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
