import { useLocalStorage } from '@vueuse/core'

import type { SavedPostView, TopicPostView } from '#api/types.gen'

const saved = useLocalStorage<SavedPostView[]>('dt:saved:guest', [])

export const useGuestSavedPosts = () => {
  const toggle = (post: TopicPostView) => (isSaved(+post.id) ? remove(+post.id) : save(post))

  const isSaved = (postId: number) => saved.value.some((s) => s.postId === postId)

  const restore = (post: SavedPostView) => {
    if (isSaved(+post.postId)) return

    saved.value.push(post)
  }

  const save = (post: TopicPostView) => {
    if (isSaved(+post.id)) return

    saved.value.push({
      postId: +post.id,
      title: post.title,
      kind: post.kind,
      topicSlug: post.topicSlug,
      topicTitle: post.topicTitle,
      savedAt: new Date().toISOString(),
    })
  }

  const remove = (postId: number) => (saved.value = saved.value.filter((s) => s.postId !== postId))

  const clear = () => (saved.value = [])

  const savedPosts = computed(() =>
    saved.value.toSorted((a, b) => b.savedAt.localeCompare(a.savedAt))
  )

  return { savedPosts, isSaved, save, restore, remove, toggle, clear }
}
