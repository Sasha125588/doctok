<script setup lang="ts">
import type { SectionCluster } from '../-types'

defineProps<{
  cluster: SectionCluster
}>()
</script>

<template>
  <article class="cluster-card">
    <h3 class="cluster-title">{{ cluster.title }}</h3>
    <p class="cluster-description">{{ cluster.description }}</p>
    <div class="cluster-topics">
      <NuxtLink
        v-for="topic in cluster.topics"
        :key="topic"
        class="cluster-topic"
        :to="{ name: 'search', query: { q: topic } }"
      >
        {{ topic }}
      </NuxtLink>
    </div>
  </article>
</template>

<style scoped>
.cluster-card {
  display: grid;
  gap: 9px;
  min-height: 150px;
  padding: 14px;
  border: 1px solid #161616;
  border-radius: 7px;
  background: #090909;
}
.cluster-title {
  color: #d1d1c7;
  font-family: var(--font-display);
  font-size: 15px;
  line-height: 1.25;
  font-weight: 400;
}
.cluster-description {
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 10px;
  line-height: 1.55;
}
.cluster-topics {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: auto;
}
.cluster-topic {
  padding: 4px 8px;
  border: 1px solid #1c1c1c;
  border-radius: 999px;
  color: #888;
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.2;
  text-decoration: none;
  transition:
    border-color 0.15s,
    color 0.15s,
    background 0.15s;
}
.cluster-topic:hover {
  border-color: rgba(126, 183, 124, 0.26);
  background: rgba(126, 183, 124, 0.06);
  color: #a8cba3;
}
</style>
