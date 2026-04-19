<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'

import BrowseMode from './BrowseMode.vue'
import FocusMode from './FocusMode.vue'

import { useFeedView } from '~/composables/useFeedView'

const { mode, activeTopicSlug } = useFeedView()

// Keyed so child composables re-instantiate on slug change (see FocusMode note).
const slugKey = computed(() => `slug:${activeTopicSlug.value}`)
</script>

<template>
  <div class="feed-page">
    <div class="stack">
      <AnimatePresence mode="wait">
        <motion.div
          v-if="mode === 'focus'"
          key="focus"
          class="pane"
          :initial="{ opacity: 0 }"
          :animate="{ opacity: 1 }"
          :exit="{ opacity: 0 }"
          :transition="{ duration: 0.18 }"
        >
          <FocusMode :key="slugKey" />
        </motion.div>
        <motion.div
          v-else
          key="browse"
          class="pane"
          :initial="{ opacity: 0 }"
          :animate="{ opacity: 1 }"
          :exit="{ opacity: 0 }"
          :transition="{ duration: 0.18 }"
        >
          <BrowseMode :key="slugKey" />
        </motion.div>
      </AnimatePresence>
    </div>
    <!-- Side panels inserted in Chunk 6 -->
  </div>
</template>

<style scoped>
.feed-page {
  flex: 1;
  display: flex;
  flex-direction: row;
  overflow: hidden;
  min-width: 0;
}
.stack {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-width: 0;
  position: relative;
}
.pane {
  position: absolute;
  inset: 0;
  display: flex;
}
</style>
