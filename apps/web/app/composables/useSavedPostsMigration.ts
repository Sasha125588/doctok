import { useGuestSavedPosts } from './useGuestSavedPosts'
import { useServerSavedPosts } from './useServerSavedPosts'

export const useSavedPostsMigration = async () => {
  const session = useSession()
  const guest = useGuestSavedPosts()

  const isAuthenticated = computed(
    () => session.isSuccess.value && Boolean(session.data.value?.userId)
  )

  const server = useServerSavedPosts({
    enabled: isAuthenticated,
  })

  const isMigrating = useState('saved-posts:migrating', () => false)

  const migrateGuestToServer = async () => {
    if (!isAuthenticated.value) return
    if (isMigrating.value) return

    const guestPosts = guest.savedPosts.value
    if (!guestPosts.length) return

    isMigrating.value = true

    try {
      for (const post of guestPosts) {
        await server.save({ postId: post.postId })
      }

      guest.clear()
    } finally {
      isMigrating.value = false
    }
  }

  watch(
    isAuthenticated,
    async (authed) => {
      if (authed) return
      await migrateGuestToServer()
    },
    { immediate: true }
  )

  //   return {
  //     isMigrating,
  //     migrateGuestToServer,
  //   }
}
