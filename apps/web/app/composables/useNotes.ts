import { useLocalStorage } from '@vueuse/core'

const notes = useLocalStorage<Record<string, string>>('dt:notes', {})

export function useNotes() {
  function get(postId: number | string | null) {
    if (postId == null) return ''
    return notes.value[String(postId)] ?? ''
  }

  function set(postId: number | string, text: string) {
    const trimmed = text.trim()
    const key = String(postId)
    if (trimmed) {
      notes.value = { ...notes.value, [key]: trimmed }
    } else {
      const { [key]: _, ...rest } = notes.value
      notes.value = rest
    }
  }

  function has(postId: number | string | null) {
    if (postId == null) return false
    return !!notes.value[String(postId)]
  }

  return { notes, get, set, has }
}
