<script setup lang="ts">
import { motion } from 'motion-v'

import type { ReactionValue } from '#api/types.gen'

defineProps<{
  myVote: ReactionValue
  likeCount: number | string
  commentCount: number | string
  isSaved: boolean
  hasNote: boolean
}>()

const emit = defineEmits<{
  onVote: [ReactionValue]
  onToggleSave: []
  onOpenNote: []
  onOpenComments: []
  onShare: []
}>()
</script>

<template>
  <div class="actions">
    <motion.button
      class="btn"
      :class="{ 'is-liked': myVote === 'like' }"
      :while-tap="{ scale: 0.9 }"
      :while-hover="{ scale: 1.08 }"
      :transition="{ type: 'spring', stiffness: 400, damping: 22 }"
      @click="emit('onVote', 'like')"
    >
      <Icon name="lucide:heart" class="icon" />
      <span class="count">{{ likeCount }}</span>
    </motion.button>

    <motion.button
      class="btn"
      :class="{ 'is-saved': isSaved }"
      :while-tap="{ scale: 0.9 }"
      :while-hover="{ scale: 1.08 }"
      :transition="{ type: 'spring', stiffness: 400, damping: 22 }"
      @click="emit('onToggleSave')"
    >
      <Icon name="lucide:bookmark" class="icon" />
      <span class="label">save</span>
    </motion.button>

    <motion.button
      class="btn"
      :class="{ 'is-noted': hasNote }"
      :while-tap="{ scale: 0.9 }"
      :while-hover="{ scale: 1.08 }"
      :transition="{ type: 'spring', stiffness: 400, damping: 22 }"
      @click="emit('onOpenNote')"
    >
      <Icon name="lucide:pencil" class="icon" />
      <span class="label">note</span>
    </motion.button>

    <motion.button
      class="btn"
      :while-tap="{ scale: 0.9 }"
      :while-hover="{ scale: 1.08 }"
      :transition="{ type: 'spring', stiffness: 400, damping: 22 }"
      @click="emit('onOpenComments')"
    >
      <Icon name="lucide:message-square" class="icon" />
      <span class="count">{{ commentCount }}</span>
    </motion.button>

    <motion.button
      class="btn"
      :while-tap="{ scale: 0.9 }"
      :while-hover="{ scale: 1.08 }"
      :transition="{ type: 'spring', stiffness: 400, damping: 22 }"
      @click="emit('onShare')"
    >
      <Icon name="lucide:share-2" class="icon" />
      <span class="label">share</span>
    </motion.button>
  </div>
</template>

<style scoped>
.actions {
  display: flex;
  flex-direction: column;
  gap: 5px;
  align-items: center;
  flex-shrink: 0;
}
.btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  border: none;
  background: none;
  cursor: pointer;
  padding: 2px;
  color: var(--dt-text-tertiary);
  transition: color 0.15s;
}
.btn:hover {
  color: #888;
}
.icon {
  width: 14px;
  height: 14px;
}
.count,
.label {
  font-family: var(--font-mono);
  font-size: 7px;
  color: var(--dt-text-quaternary);
}
.btn.is-liked {
  color: var(--kind-example);
}
.btn.is-liked .count {
  color: var(--kind-example);
}
.btn.is-saved {
  color: var(--kind-fact);
}
.btn.is-noted {
  color: var(--chart-4);
}
</style>
