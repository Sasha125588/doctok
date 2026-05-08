import { acceptHMRUpdate, defineStore } from 'pinia'
import { ref } from 'vue'

export type FeedPanel = 'comments' | 'notes' | null

export const useFeedViewStore = defineStore('feed-view', () => {
  const activePanel = ref<FeedPanel>(null)
  const sidebarHidden = ref(false)

  const toggleSidebar = () => (sidebarHidden.value = !sidebarHidden.value)

  const closePanel = () => (activePanel.value = null)

  const togglePanel = (panel: Exclude<FeedPanel, null>) =>
    (activePanel.value = activePanel.value === panel ? null : panel)

  const $reset = () => {
    activePanel.value = null
    sidebarHidden.value = false
  }

  return {
    activePanel,
    sidebarHidden,
    toggleSidebar,
    closePanel,
    togglePanel,
    $reset,
  }
})

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useFeedViewStore, import.meta.hot))
}
