import { useLocalStorage } from '@vueuse/core'

import type { TopicPostView } from '#api/types.gen'

export interface SavedPost {
  postId: number
  topicSlug: string
  title: string
  kind: string
  savedAt: number
}

const saved = useLocalStorage<SavedPost[]>('dt:saved', [])

export function useSavedPosts() {
  function toggle(post: TopicPostView) {
    const id = +post.id
    if (isSaved(id)) {
      remove(id)
    } else {
      saved.value.push({
        postId: id,
        topicSlug: post.topicSlug,
        title: post.title,
        kind: post.kind,
        savedAt: Date.now(),
      })
    }
  }

  function isSaved(postId: number) {
    return saved.value.some((s) => s.postId === postId)
  }

  function remove(postId: number) {
    saved.value = saved.value.filter((s) => s.postId !== postId)
  }

  const sorted = computed(() => saved.value.toSorted((a, b) => (b.savedAt ?? 0) - (a.savedAt ?? 0)))

  return { saved, sorted, isSaved, toggle, remove }
}
