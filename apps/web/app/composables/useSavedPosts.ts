import { useLocalStorage } from '@vueuse/core'

import type { TopicPostView } from '#api/types.gen'

export interface SavedPost {
  postId: number
  topicSlug: string
  title: string
  kind: string
  // Optional to stay compatible with legacy localStorage entries written before
  // this field existed. New entries always set it via Date.now().
  savedAt?: number
}

const saved = useLocalStorage<SavedPost[]>('dt:saved', [])

export function useSavedPosts() {
  function isSaved(postId: number) {
    return saved.value.some((s) => s.postId === postId)
  }

  function toggle(post: TopicPostView) {
    const id = +post.id
    if (isSaved(id)) {
      remove(id)
    } else {
      saved.value = [
        ...saved.value,
        {
          postId: id,
          topicSlug: post.topicSlug,
          title: post.title,
          kind: post.kind,
          savedAt: Date.now(),
        },
      ]
    }
  }

  function remove(postId: number) {
    saved.value = saved.value.filter((s) => s.postId !== postId)
  }

  // Newest first. Legacy entries (no savedAt) default to 0 and sink to the end.
  // No write-back to localStorage; legacy entries stay legacy until toggled.
  const sorted = computed<SavedPost[]>(() =>
    [...saved.value].sort((a, b) => (b.savedAt ?? 0) - (a.savedAt ?? 0))
  )

  return { saved, sorted, isSaved, toggle, remove }
}
