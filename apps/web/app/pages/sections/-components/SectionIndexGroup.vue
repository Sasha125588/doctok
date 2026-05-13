<script setup lang="ts">
import SectionIndexCard from './SectionIndexCard.vue'

import type { SectionIndexGroup } from '../-types'

defineProps<{
  group: SectionIndexGroup
}>()
</script>

<template>
  <section class="index-group">
    <header class="index-group-header">
      <div>
        <div class="index-group-kicker">// {{ group.items.length }} sections</div>
        <h2 class="index-group-title">{{ group.title }}</h2>
      </div>
      <p class="index-group-description">{{ group.description }}</p>
    </header>

    <div class="index-group-grid">
      <SectionIndexCard
        v-for="item in group.items"
        :key="item.title"
        :item
      />
    </div>
  </section>
</template>

<style scoped>
.index-group {
  display: grid;
  gap: 12px;
}
.index-group-header {
  display: flex;
  align-items: end;
  justify-content: space-between;
  gap: 18px;
  padding-bottom: 1px;
}
.index-group-kicker {
  color: #6f756d;
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.2;
  letter-spacing: 0.08em;
}
.index-group-title {
  margin-top: 5px;
  color: #d0d0c7;
  font-family: var(--font-display);
  font-size: 20px;
  line-height: 1.2;
  font-weight: 400;
}
.index-group-description {
  max-width: 420px;
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 10px;
  line-height: 1.55;
  text-align: right;
}
.index-group-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(190px, 1fr));
  gap: 10px;
}
@media (max-width: 1180px) {
  .index-group-grid {
    grid-template-columns: repeat(3, minmax(190px, 1fr));
  }
}
@media (max-width: 820px) {
  .index-group-header {
    align-items: start;
    flex-direction: column;
  }
  .index-group-description {
    text-align: left;
  }
  .index-group-grid {
    grid-template-columns: repeat(2, minmax(180px, 1fr));
  }
}
@media (max-width: 540px) {
  .index-group-grid {
    grid-template-columns: 1fr;
  }
}
</style>
