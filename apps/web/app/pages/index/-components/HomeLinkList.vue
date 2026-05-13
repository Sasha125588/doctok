<script setup lang="ts">
import type { HomeLinkGroup } from '../-types'

defineProps<{
  group: HomeLinkGroup
}>()
</script>

<template>
  <section class="link-group">
    <h3 class="link-group-title">{{ group.title }}</h3>
    <div class="link-list">
      <a
        v-for="link in group.links"
        :key="link.href"
        class="link-item"
        :href="link.href"
        rel="noreferrer"
        target="_blank"
      >
        <span>{{ link.label }}</span>
        <Icon
          name="lucide:arrow-up-right"
          class="link-icon"
        />
      </a>
    </div>
  </section>
</template>

<style scoped>
.link-group {
  display: grid;
  gap: 9px;
}
.link-group-title {
  color: #bdbdb2;
  font-family: var(--font-display);
  font-size: 13px;
  line-height: 1.2;
  font-weight: 400;
}
.link-list {
  display: grid;
  gap: 4px;
}
.link-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  min-height: 28px;
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 10px;
  line-height: 1.3;
  text-decoration: none;
  transition: color 0.15s;
}
.link-item:hover {
  color: var(--kind-example);
}
.link-icon {
  width: 10px;
  height: 10px;
  flex-shrink: 0;
}
</style>
