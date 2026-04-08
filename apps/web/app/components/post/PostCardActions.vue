<script setup lang="ts">
import type { VoteValue } from '#api/types.gen'

defineProps<{
  myVote: string
  likeCount: number | string
  dislikeCount: number | string
  commentCount: number | string
}>()

const emit = defineEmits<{
  onVote: [VoteValue]
}>()
</script>

<template>
  <div class="flex items-center gap-2">
    <button
      class="action-pill"
      :class="{ 'action-pill--liked': myVote === 'like' }"
      @click="emit('onVote', 'like')"
    >
      <Icon
        name="lucide:heart"
        class="size-[13px]"
      />
      {{ likeCount }}
    </button>
    <button
      class="action-pill"
      :class="{ 'action-pill--disliked': myVote === 'dislike' }"
      @click="emit('onVote', 'dislike')"
    >
      <Icon
        name="lucide:thumbs-down"
        class="size-[13px]"
      />
      {{ dislikeCount }}
    </button>
    <button class="action-pill">
      <Icon
        name="lucide:message-square"
        class="size-[13px]"
      />
      {{ commentCount }}
    </button>
    <button class="action-pill ml-auto">
      <Icon
        name="lucide:share"
        class="size-[13px]"
      />
    </button>
  </div>
</template>

<style scoped>
.action-pill {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: rgba(255, 255, 255, 0.06);
  border: 1px solid rgba(255, 255, 255, 0.07);
  border-radius: 30px;
  padding: 8px 14px;
  font-family: var(--font-mono);
  font-size: 0.75rem;
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.2s ease;
  -webkit-tap-highlight-color: transparent;
}
.action-pill:hover {
  background: rgba(255, 255, 255, 0.1);
  color: var(--foreground);
}
.action-pill--liked {
  color: #f87171;
  background: rgba(248, 113, 113, 0.12);
  border-color: rgba(248, 113, 113, 0.25);
}
.action-pill--disliked {
  color: #60a5fa;
  background: rgba(96, 165, 250, 0.12);
  border-color: rgba(96, 165, 250, 0.25);
}
</style>
