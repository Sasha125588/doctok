<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'

import FocusCard from './FocusCard.vue'

import type { TopicPostView } from '#api/types.gen'

const props = defineProps<{
  activePost?: TopicPostView
  currentIndex: number
  isLoading: boolean
  totalPosts: number
}>()
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
            :total-posts
            :current-index
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
