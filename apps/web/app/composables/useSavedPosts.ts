import { useGuestSavedPosts } from './useGuestSavedPosts'
import { useServerSavedPosts } from './useServerSavedPosts'

import type { SavedPostView, TopicPostView } from '~~/generated/api/types.gen'

export const useSavedPosts = () => {
  const session = useSession()

  const guest = useGuestSavedPosts()
  const server = useServerSavedPosts()

  const isAuthenticated = computed(
    () => session.isSuccess.value && Boolean(session.data.value?.userId)
  )

  const savedPosts = computed(() =>
    isAuthenticated.value ? server.savedPosts.value : guest.savedPosts.value
  )

  const isSaved = (post: TopicPostView) =>
    isAuthenticated.value ? post.isSaved : guest.isSaved(+post.id)

  const save = async (post: TopicPostView) =>
    isAuthenticated.value
      ? await server.save({ postId: post.id, topicSlug: post.topicSlug })
      : guest.save(post)

  const remove = async (post: SavedPostView) =>
    isAuthenticated.value
      ? await server.remove({ postId: post.postId, topicSlug: post.topicSlug })
      : guest.remove(+post.postId)

  const toggle = async (post: TopicPostView) =>
    isAuthenticated.value
      ? await server.toggle({ postId: post.id, topicSlug: post.topicSlug }, post.isSaved)
      : guest.toggle(post)

  const clear = async () => (isAuthenticated.value ? await server.clear() : guest.clear())

  const isClearing = computed(() => (isAuthenticated.value ? server.isClearing.value : false))

  return { savedPosts, isSaved, save, remove, toggle, clear, isClearing }
}
