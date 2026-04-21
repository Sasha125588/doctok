<script setup lang="ts">
import { motion } from 'motion-v'

import { usePostKind } from '~/composables/usePostKind'

const props = defineProps<{
  topicTitle: string
  kind: string
  totalPosts: number
  currentIndex: number
}>()

const kindConfig = usePostKind(() => props.kind)
</script>

<template>
  <div class="meta">
    <span class="topic">{{ topicTitle }}</span>
    <span class="sep">·</span>
    <span
      class="kind-badge"
      :style="{
        color: kindConfig.cssColor,
        background: kindConfig.cssColor + '22',
        borderColor: kindConfig.cssColor + '55',
      }"
    >
      {{ kindConfig.label }}
    </span>
    <div class="dots">
      <motion.div
        v-for="i in totalPosts"
        :key="i"
        class="dot"
        :animate="{
          width: i - 1 === currentIndex ? 18 : 5,
          backgroundColor: i - 1 === currentIndex ? kindConfig.cssColor : '#1a1a1a',
        }"
        :transition="{ duration: 0.22 }"
      />
    </div>
    <span class="source">MDN</span>
  </div>
</template>

<style scoped>
.meta {
  display: flex;
  align-items: center;
  gap: 7px;
  flex-wrap: nowrap;
  overflow: hidden;
}
.topic {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-tertiary);
  white-space: nowrap;
}
.sep {
  font-family: var(--font-mono);
  color: #141414;
  font-size: 8px;
  margin: 0 2px;
}
.kind-badge {
  font-family: var(--font-mono);
  font-size: 8px;
  padding: 2px 8px;
  border-radius: 2px;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  font-weight: 600;
  border: 1px solid;
  flex-shrink: 0;
}
.dots {
  display: flex;
  gap: 3px;
  align-items: center;
  flex-shrink: 0;
}
.dot {
  height: 2px;
  border-radius: 1px;
}
.source {
  font-family: var(--font-mono);
  font-size: 7px;
  padding: 2px 7px;
  border-radius: 2px;
  border: 1px solid #161616;
  color: var(--dt-text-tertiary);
  flex-shrink: 0;
  margin-left: auto;
}
</style>
