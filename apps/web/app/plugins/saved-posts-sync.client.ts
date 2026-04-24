import { useGuestSavedPostsSync } from '~/composables/useGuestSavedPostsSync'

export default defineNuxtPlugin(() => {
  useGuestSavedPostsSync()
})
