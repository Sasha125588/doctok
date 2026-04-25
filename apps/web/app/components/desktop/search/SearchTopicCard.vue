<script setup lang="ts">
import type { MockTopic } from '~/lib/searchMockData'

const props = defineProps<{
  topic: MockTopic
}>()

const meta = computed(() => `${props.topic.posts.length} posts · ${props.topic.source}`)
</script>

<template>
  <button
    type="button"
    class="card"
  >
    <span class="icon">{{ topic.icon }}</span>
    <span class="name">{{ topic.name }}</span>
    <span class="meta">{{ meta }}</span>
  </button>
</template>

<style scoped>
.card {
  width: 120px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 5px;
  padding: 11px;
  border: 1px solid #0e0e0e;
  border-radius: 6px;
  background: var(--dt-panel-bg);
  cursor: pointer;
  text-align: left;
  transition:
    border-color 0.15s ease,
    background 0.15s ease;
}
.card:hover {
  border-color: #161616;
  background: #0a0a0a;
}
.icon {
  font-size: 15px;
  color: #2a2a2a;
}
.name {
  font-family: var(--font-mono);
  font-size: 10px;
  color: #4a4a4a;
  line-height: 1.3;
}
.meta {
  font-family: var(--font-mono);
  font-size: 8px;
  color: #1a1a1a;
  line-height: 1.4;
}
</style>
