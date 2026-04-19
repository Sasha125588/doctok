<script setup lang="ts">
import FeedPage from './FeedPage.vue'
import Rail from './Rail.vue'
import Sidebar from './Sidebar.vue'
import Topbar from './Topbar.vue'

import { useFeed } from '~/composables/useFeed'
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useTopicHistory } from '~/composables/useTopicHistory'

const { lang } = useLang()
const { state, functions } = useFeed(lang)
const { activeTopicSlug, activePostIndex, activePanel, mode: feedMode, activeTopicPostCount } = useFeedView()
const { addRecent } = useTopicHistory()

// Seed active topic once feed arrives. addRecent is handled by the recency watcher below.
watch(
  () => state.topics.value,
  (topics) => {
    if (!activeTopicSlug.value && topics.length) {
      activeTopicSlug.value = topics[0].slug
    }
  },
  { immediate: true }
)

// Track recency whenever active topic changes.
watch(activeTopicSlug, (slug) => {
  if (slug) addRecent(slug)
})

function nextTopic(direction: 1 | -1) {
  const topics = state.topics.value
  const idx = topics.findIndex((t) => t.slug === activeTopicSlug.value)
  if (idx === -1) return
  const next = idx + direction
  if (next >= 0 && next < topics.length) {
    activeTopicSlug.value = topics[next].slug
    activePostIndex.value = 0
    return
  }
  if (direction === 1 && state.hasNextPage.value && !state.isFetchingNextPage.value) {
    const lenBefore = topics.length
    functions
      .fetchNextPage()
      .then(() => {
        const updated = state.topics.value
        if (updated.length > lenBefore) {
          activeTopicSlug.value = updated[lenBefore].slug
          activePostIndex.value = 0
        }
      })
      .catch(() => {
        /* swallow — user can retry */
      })
  }
}

function onKeydown(e: KeyboardEvent) {
  if (feedMode.value !== 'focus') return
  if (activePanel.value !== null) return
  const tag = (document.activeElement?.tagName ?? '').toUpperCase()
  if (tag === 'INPUT' || tag === 'TEXTAREA') return

  const len = activeTopicPostCount.value

  if (e.key === 'ArrowRight') {
    e.preventDefault()
    activePostIndex.value = Math.min(activePostIndex.value + 1, Math.max(0, len - 1))
  } else if (e.key === 'ArrowLeft') {
    e.preventDefault()
    activePostIndex.value = Math.max(0, activePostIndex.value - 1)
  } else if (e.key === 'ArrowDown') {
    e.preventDefault()
    nextTopic(1)
  } else if (e.key === 'ArrowUp') {
    e.preventDefault()
    nextTopic(-1)
  }
}

onMounted(() => window.addEventListener('keydown', onKeydown))
onBeforeUnmount(() => window.removeEventListener('keydown', onKeydown))
</script>

<template>
  <div class="desktop-scope shell">
    <Rail />
    <Sidebar />
    <main class="main">
      <Topbar />
      <div class="body">
        <FeedPage />
      </div>
    </main>
  </div>
</template>

<style scoped>
.shell {
  background: #080808;
  display: flex;
  height: 100dvh;
  width: 100%;
  overflow: hidden;
  font-family: var(--font-mono);
}
.main {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-width: 0;
}
.body {
  flex: 1;
  display: flex;
  overflow: hidden;
}
</style>
