import { useGuestSavedPosts } from './useGuestSavedPosts'
import { useServerSavedPosts } from './useServerSavedPosts'

import type { SavedPostView, TopicPostView } from '#api/types.gen'

export const savedPostRemoveUndoDelayMs = 5000

interface PendingSavedPostRemoval {
  timeoutId: ReturnType<typeof setTimeout>
}

const pendingRemovals = reactive(new Map<number, PendingSavedPostRemoval>())

export const useSavedPosts = () => {
  const session = useSession()

  const guest = useGuestSavedPosts()
  const server = useServerSavedPosts()

  const isAuthenticated = computed(
    () => session.isSuccess.value && Boolean(session.data.value?.userId)
  )

  const sourceSavedPosts = computed(() =>
    isAuthenticated.value ? server.savedPosts.value : guest.savedPosts.value
  )

  const savedPosts = computed(() =>
    sourceSavedPosts.value.filter((post) => !pendingRemovals.has(+post.postId))
  )

  const toSavedPostView = (post: TopicPostView) => ({
    postId: post.id,
    title: post.title,
    kind: post.kind,
    topicSlug: post.topicSlug,
    topicTitle: post.topicTitle,
    savedAt: new Date().toISOString(),
  })

  const isSaved = (post: TopicPostView) => {
    if (pendingRemovals.has(+post.id)) return false

    return isAuthenticated.value ? post.isSaved : guest.isSaved(+post.id)
  }

  const remove = async (post: SavedPostView) => {
    if (pendingRemovals.has(+post.postId)) return

    const timeoutId = setTimeout(() => commitRemove(post), savedPostRemoveUndoDelayMs)

    pendingRemovals.set(+post.postId, { timeoutId })
  }

  const commitRemove = async (post: SavedPostView) => {
    try {
      isAuthenticated.value
        ? await server.remove({ postId: post.postId, topicSlug: post.topicSlug })
        : guest.remove(+post.postId)
    } finally {
      pendingRemovals.delete(+post.postId)
    }
  }

  const restore = async (post: SavedPostView) => {
    if (cancelRemove(+post.postId)) return

    return isAuthenticated.value
      ? await server.save({ postId: post.postId, savedPost: post, topicSlug: post.topicSlug })
      : guest.restore(post)
  }

  const toggle = async (post: TopicPostView) => {
    if (cancelRemove(+post.id)) return

    return isAuthenticated.value
      ? await server.toggle(
          {
            postId: post.id,
            savedPost: toSavedPostView(post),
            topicSlug: post.topicSlug,
          },
          post.isSaved
        )
      : guest.toggle(post)
  }

  const cancelRemove = (postId: number) => {
    const pendingRemoval = pendingRemovals.get(postId)
    if (!pendingRemoval) return false

    clearTimeout(pendingRemoval.timeoutId)
    pendingRemovals.delete(postId)
    return true
  }

  const clear = async () => {
    clearPendingRemovals()

    return isAuthenticated.value ? await server.clear() : guest.clear()
  }

  const clearPendingRemovals = () => {
    pendingRemovals.forEach(({ timeoutId }) => clearTimeout(timeoutId))
    pendingRemovals.clear()
  }

  const isClearing = computed(() => (isAuthenticated.value ? server.isClearing.value : false))
  const isLoading = computed(() => (isAuthenticated.value ? server.isLoading.value : false))
  const hasNextPage = computed(() => (isAuthenticated.value ? server.hasNextPage.value : false))
  const isFetchingNextPage = computed(() =>
    isAuthenticated.value ? server.isFetchingNextPage.value : false
  )

  return {
    savedPosts,
    isSaved,
    remove,
    restore,
    toggle,
    clear,
    isClearing,
    isLoading,
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage: server.fetchNextPage,
  }
}
