import { useLocalStorage } from '@vueuse/core'

const RECENT_LIMIT = 5

const pinned = useLocalStorage<string[]>('dt:pinned', [])
const recent = useLocalStorage<string[]>('dt:recent', [])

export function useTopicHistory() {
  function addRecent(slug: string) {
    const next = [slug, ...recent.value.filter((s) => s !== slug)]
    recent.value = next.slice(0, RECENT_LIMIT)
  }

  function togglePin(slug: string) {
    if (pinned.value.includes(slug)) {
      pinned.value = pinned.value.filter((s) => s !== slug)
    } else {
      pinned.value = [...pinned.value, slug]
    }
  }

  function isPinned(slug: string) {
    return pinned.value.includes(slug)
  }

  return { pinned, recent, addRecent, togglePin, isPinned }
}
