<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'

import FocusCard from './FocusCard.vue'
import { useFeedView } from '~/composables/useFeedView'

import type { TopicPostView } from '#api/types.gen'

const props = defineProps<{
  activePost?: TopicPostView
  isLoading: boolean
  totalPosts: number
}>()

const { activePostIndex, activePanel, activeTopicPostCount } = useFeedView()

watch(
  () => props.totalPosts,
  (len) => (activeTopicPostCount.value = len),
  { immediate: true }
)

const openNotes = () => {
  activePanel.value = activePanel.value === 'notes' ? null : 'notes'
}

const openComments = () => {
  activePanel.value = activePanel.value === 'comments' ? null : 'comments'
}
</script>

<template>
  <section class="focus">
    <div class="card-area">
      <AnimatePresence
        :initial="false"
        mode="wait"
      >
        <motion.div
          :key="activePost ? `card-${activePost.id}` : props.isLoading ? 'loading' : 'empty'"
          class="card-slot"
          :initial="{ opacity: 0, x: 10 }"
          :animate="{ opacity: 1, x: 0 }"
          :exit="{ opacity: 0, x: -10 }"
          :transition="{ duration: 0.16, ease: 'easeOut' }"
        >
          <FocusCard
            v-if="activePost"
            :post="activePost"
            :total-posts="props.totalPosts"
            :current-index="activePostIndex"
            @open-notes="openNotes"
            @open-comments="openComments"
          />
          <div
            v-else-if="props.isLoading"
            class="loading"
          >
            // завантаження...
          </div>
          <div
            v-else
            class="loading"
          >
            // оберіть тему
          </div>
        </motion.div>
      </AnimatePresence>
    </div>
  </section>
</template>

<style scoped>
.focus {
  display: flex;
  flex-direction: column;
  flex: 1;
  overflow: hidden;
}
.card-area {
  flex: 1;
  padding: 18px 28px 18px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  position: relative;
}
.loading {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  color: var(--dt-text-quaternary);
}
.card-slot {
  flex: 1;
  min-height: 0;
  display: flex;
}
</style>
