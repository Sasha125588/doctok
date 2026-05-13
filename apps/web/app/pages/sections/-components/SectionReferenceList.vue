<script setup lang="ts">
import type { SectionReferenceItem } from '../-types'

defineProps<{
  items: SectionReferenceItem[]
}>()
</script>

<template>
  <div class="reference-list">
    <NuxtLink
      v-for="item in items"
      :key="item.title"
      class="reference-item"
      :to="{ name: 'search', query: { q: item.title } }"
    >
      <span class="reference-title">{{ item.title }}</span>
      <span class="reference-meta">{{ item.meta }}</span>
      <Icon
        name="lucide:arrow-right"
        class="reference-icon"
      />
    </NuxtLink>
  </div>
</template>

<style scoped>
.reference-list {
  display: grid;
  border: 1px solid #161616;
  border-radius: 7px;
  overflow: hidden;
  background: #090909;
}
.reference-item {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto auto;
  align-items: center;
  gap: 12px;
  min-height: 40px;
  padding: 0 12px;
  color: inherit;
  text-decoration: none;
  transition:
    background 0.15s,
    color 0.15s;
}
.reference-item + .reference-item {
  border-top: 1px solid #121212;
}
.reference-item:hover {
  background: #0e0e0e;
}
.reference-title {
  overflow: hidden;
  color: #d0d0c7;
  font-family: var(--font-mono);
  font-size: 11px;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.reference-meta {
  color: var(--dt-text-quaternary);
  font-family: var(--font-mono);
  font-size: 9px;
  white-space: nowrap;
}
.reference-icon {
  width: 11px;
  height: 11px;
  color: #555;
}
.reference-item:hover .reference-icon {
  color: var(--kind-example);
}
</style>
