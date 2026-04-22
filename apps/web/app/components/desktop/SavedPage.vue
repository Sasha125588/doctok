<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'

import SavedCard from './SavedCard.vue'
import { useSavedPosts } from '~/composables/useSavedPosts'

const { sorted } = useSavedPosts()
</script>

<template>
  <section class="saved">
    <header class="header">
      <div class="title">// saved</div>
      <div class="count">{{ sorted.length }} posts</div>
    </header>
    <div
      v-if="!sorted.length"
      class="empty"
    >
      <div>// тут порожньо</div>
      <NuxtLink
        to="/"
        class="empty-link"
      >
        // збережи свій перший пост у стрічці →
      </NuxtLink>
    </div>
    <div
      v-else
      class="grid"
    >
      <AnimatePresence>
        <motion.div
          v-for="(post, i) in sorted"
          :key="post.postId"
          :initial="{ opacity: 0, y: 4 }"
          :animate="{ opacity: 1, y: 0 }"
          :exit="{ opacity: 0, y: -4 }"
          :transition="{ duration: 0.18, delay: Math.min(i, 10) * 0.02 }"
        >
          <SavedCard :post="post" />
        </motion.div>
      </AnimatePresence>
    </div>
  </section>
</template>

<style scoped>
.saved {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  padding: 20px 24px;
}
.header {
  display: flex;
  align-items: baseline;
  gap: 12px;
  margin-bottom: 16px;
  flex-shrink: 0;
}
.title {
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--dt-text-tertiary);
  letter-spacing: 0.1em;
}
.count {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-quaternary);
}
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 12px;
}
.empty {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--dt-text-quaternary);
}
.empty-link {
  color: var(--dt-text-quaternary);
  text-decoration: none;
  transition: color 0.15s;
}
.empty-link:hover {
  color: var(--kind-example);
}
</style>
