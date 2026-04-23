<script setup lang="ts">
import type { MockPostKind, MockSearchHit } from '~/lib/searchMockData'

const props = defineProps<{
  hit: MockSearchHit
}>()

const kindStyle: Record<MockPostKind, { bg: string; fg: string; border: string }> = {
  summary: { bg: '#0d1f35', fg: '#6ab4ff', border: '#2a5f9f' },
  example: { bg: '#001f10', fg: '#00e87a', border: '#006b38' },
  fact: { bg: '#1e1400', fg: '#ffb830', border: '#7a5200' },
}

const badgeStyle = computed(() => {
  const style = kindStyle[props.hit.post.kind]

  return {
    backgroundColor: style.bg,
    color: style.fg,
    borderColor: style.border,
  }
})
</script>

<template>
  <button
    type="button"
    class="row"
  >
    <span
      class="kind"
      :style="badgeStyle"
    >
      {{ hit.post.kind }}
    </span>
    <span class="title">{{ hit.post.title }}</span>
    <span class="topic">{{ hit.topic.name }}</span>
  </button>
</template>

<style scoped>
.row {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 9px 12px;
  border: 1px solid #0e0e0e;
  border-radius: 4px;
  background: var(--dt-panel-bg);
  cursor: pointer;
  text-align: left;
  transition:
    background 0.12s ease,
    border-color 0.12s ease;
}
.row:hover {
  background: #0c0c0c;
  border-color: #161616;
}
.kind {
  flex-shrink: 0;
  border: 1px solid transparent;
  border-radius: 2px;
  padding: 2px 6px;
  font-family: var(--font-mono);
  font-size: 7px;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}
.title {
  min-width: 0;
  flex: 1;
  font-family: var(--font-mono);
  font-size: 11px;
  color: #444;
}
.topic {
  flex-shrink: 0;
  font-family: var(--font-mono);
  font-size: 8px;
  color: #1a1a1a;
}
</style>
