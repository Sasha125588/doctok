<script setup lang="ts">
import SearchResultRow from './SearchResultRow.vue'

import type { MockSearchHit } from '~/lib/searchMockData'

defineProps<{
  query: string
  results: MockSearchHit[]
}>()
</script>

<template>
  <section class="results">
    <div class="label">результати для "{{ query }}"</div>

    <div
      v-if="results.length"
      class="list"
    >
      <SearchResultRow
        v-for="hit in results"
        :key="`${hit.topic.name}:${hit.post.title}`"
        :hit="hit"
      />
    </div>

    <div
      v-else
      class="empty"
    >
      // нічого не знайдено
    </div>
  </section>
</template>

<style scoped>
.results {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-height: 0;
}
.label {
  margin-bottom: 8px;
  font-family: var(--font-mono);
  font-size: 8px;
  color: #1e1e1e;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  flex-shrink: 0;
}
.list {
  display: flex;
  flex-direction: column;
  gap: 5px;
  overflow-y: auto;
  min-height: 0;
}
.list::-webkit-scrollbar {
  width: 2px;
}
.list::-webkit-scrollbar-thumb {
  background: #1a1a1a;
}
.empty {
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--dt-text-quaternary);
  line-height: 1.8;
}
</style>
