export type FeedMode = 'focus' | 'browse'
export type ReadMode = 'simplified' | 'standard' | 'detailed' | 'original'
export type FeedPanel = 'comments' | 'notes' | null

const activeTopicSlug = ref<string | null>(null)
const activePostIndex = ref(0)
const mode = ref<FeedMode>('focus')
const readMode = ref<ReadMode>('simplified')
const activePanel = ref<FeedPanel>(null)
const sidebarHidden = ref(false)
const activeTopicPostCount = ref(0)

export function useFeedView() {
  return {
    activeTopicSlug,
    activePostIndex,
    mode,
    readMode,
    activePanel,
    sidebarHidden,
    activeTopicPostCount,
  }
}
