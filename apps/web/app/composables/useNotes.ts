import { useLocalStorage } from '@vueuse/core'

const notes = useLocalStorage<Record<string, string>>('dt:notes', {})

export const useNotes = () => {
  const get = (postId: number) => notes.value[postId] ?? ''

  const set = (postId: number, text: string) => {
    const trimmed = text.trim()
    if (trimmed) {
      notes.value = { ...notes.value, [postId]: trimmed }
    } else {
      const { [postId]: _, ...rest } = notes.value
      notes.value = rest
    }
  }

  const has = (postId: number) => !!notes.value[postId]

  return { notes, get, set, has }
}
