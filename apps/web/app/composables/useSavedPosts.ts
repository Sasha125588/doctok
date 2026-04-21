import { useLocalStorage } from '@vueuse/core'

import type { TopicPostView } from '#api/types.gen'

export interface SavedPost {
  postId: number
  topicSlug: string
  title: string
  kind: string
}

const saved = useLocalStorage<SavedPost[]>('dt:saved', [])

export function useSavedPosts() {
  function isSaved(postId: number) {
    return saved.value.some((s) => s.postId === postId)
  }

  function toggle(post: TopicPostView) {
    const id = +post.id
    if (isSaved(id)) {
      saved.value = saved.value.filter((s) => s.postId !== id)
    } else {
      saved.value = [
        ...saved.value,
        { postId: id, topicSlug: post.topicSlug, title: post.title, kind: post.kind },
      ]
    }
  }

  return { saved, isSaved, toggle }
}
