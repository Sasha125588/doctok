import { useLocalStorage } from '@vueuse/core'

import type { SavedPostView, TopicPostView } from '#api/types.gen'

const saved = useLocalStorage<SavedPostView[]>('dt:saved:guest', [])

export function useGuestSavedPosts() {
  const toggle = (post: TopicPostView) => (isSaved(+post.id) ? remove(+post.id) : save(post))

  const isSaved = (postId: number) => saved.value.some((s) => s.postId === postId)

  const save = (post: TopicPostView) =>
    saved.value.push({
      postId: +post.id,
      title: post.title,
      kind: post.kind,
      topicSlug: post.topicSlug,
      topicTitle: post.topicTitle,
      savedAt: Date.now().toString(),
    })

  const remove = (postId: number) => (saved.value = saved.value.filter((s) => s.postId !== postId))

  const clear = () => (saved.value = [])

  const savedPosts = computed(() =>
    saved.value.toSorted((a, b) => {
      const timeA = a.savedAt ? new Date(a.savedAt).getTime() : 0
      const timeB = b.savedAt ? new Date(b.savedAt).getTime() : 0
      return timeB - timeA
    })
  )

  return { savedPosts, isSaved, save, remove, toggle, clear }
}
