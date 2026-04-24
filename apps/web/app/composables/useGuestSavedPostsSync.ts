import { useGuestSavedPosts } from './useGuestSavedPosts'
import { useServerSavedPosts } from './useServerSavedPosts'

export const useGuestSavedPostsSync = () => {
  const session = useSession()
  const guest = useGuestSavedPosts()

  const isAuthenticated = computed(
    () => session.isSuccess.value && Boolean(session.data.value?.userId)
  )

  const server = useServerSavedPosts({
    enabled: isAuthenticated,
  })

  const isSyncing = useState('saved-posts:syncing-guest', () => false)

  const syncGuestToServer = async () => {
    if (!isAuthenticated.value) return
    if (isSyncing.value) return

    const guestPosts = [...guest.savedPosts.value]
    if (!guestPosts.length) return

    isSyncing.value = true

    try {
      for (const post of guestPosts) {
        await server.save({ postId: post.postId, topicSlug: post.topicSlug })
      }

      guest.clear()
    } finally {
      isSyncing.value = false
    }
  }

  watch(
    isAuthenticated,
    async (authed) => {
      if (!authed) return
      await syncGuestToServer()
    },
    { immediate: true }
  )
}
