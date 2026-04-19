<script setup lang="ts">
import Rail from './Rail.vue'
import Sidebar from './Sidebar.vue'
import Topbar from './Topbar.vue'

import { useFeed } from '~/composables/useFeed'
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useTopicHistory } from '~/composables/useTopicHistory'

const { lang } = useLang()
const { state } = useFeed(lang)
const { activeTopicSlug } = useFeedView()
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
</script>

<template>
  <div class="desktop-scope shell">
    <Rail />
    <Sidebar />
    <main class="main">
      <Topbar />
      <div class="body">
        <div class="placeholder">
          // feed content — наступні задачі
        </div>
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
.placeholder {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--dt-text-tertiary);
  font-size: 11px;
}
</style>
