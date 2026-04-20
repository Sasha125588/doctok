export type FeedMode = 'focus' | 'browse'
export type ReadMode = 'simplified' | 'standard' | 'detailed' | 'original'
export type FeedPanel = 'comments' | 'notes' | null

export function useFeedView() {
  const activeTopicSlug = useState<string | null>('activeTopicSlug', () => null)
  const activePostIndex = useState<number>('activePostIndex', () => 0)
  const mode = useState<FeedMode>('mode', () => 'focus')
  const readMode = useState<ReadMode>('readMode', () => 'simplified')
  const activePanel = useState<FeedPanel | null>('activePanel', () => null)
  const sidebarHidden = useState<boolean>('sidebarHidden', () => false)
  const activeTopicPostCount = useState<number>('activeTopicPostCount', () => 0)

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
