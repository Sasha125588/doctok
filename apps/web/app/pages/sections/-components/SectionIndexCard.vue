<script setup lang="ts">
import type { SectionIndexItem } from '../-types'

const props = defineProps<{
  item: SectionIndexItem
}>()

const target = computed(() =>
  props.item.workspaceSlug
    ? `/sections/${props.item.workspaceSlug}`
    : { name: 'search', query: { q: props.item.query } }
)
</script>

<template>
  <NuxtLink
    class="index-card"
    :class="`is-${item.tone}`"
    :to="target"
  >
    <span class="index-card-rail" />
    <span class="index-card-top">
      <span class="index-card-icon">
        <Icon
          :name="item.icon"
          class="index-card-icon-svg"
        />
      </span>
      <span class="index-card-status">{{ item.workspaceSlug ? 'workspace' : 'search' }}</span>
    </span>
    <span class="index-card-title">{{ item.title }}</span>
    <span class="index-card-description">{{ item.description }}</span>
  </NuxtLink>
</template>

<style scoped>
.index-card {
  --section-color: var(--kind-example);
  position: relative;
  display: grid;
  min-height: 142px;
  gap: 9px;
  overflow: hidden;
  padding: 13px;
  border: 1px solid #161616;
  border-radius: 7px;
  background: #090909;
  color: inherit;
  text-decoration: none;
  transition:
    border-color 0.15s,
    background 0.15s,
    transform 0.15s;
}
.index-card:hover {
  border-color: #242424;
  background: #0e0e0e;
  transform: translateY(-1px);
}
.index-card.is-api {
  --section-color: var(--kind-summary);
}
.index-card.is-js {
  --section-color: var(--kind-tip);
}
.index-card.is-css {
  --section-color: var(--kind-example);
}
.index-card.is-html {
  --section-color: var(--kind-fact);
}
.index-card.is-http {
  --section-color: #38bdf8;
}
.index-card.is-a11y {
  --section-color: #f472b6;
}
.index-card.is-perf {
  --section-color: #14b8a6;
}
.index-card.is-privacy {
  --section-color: #a78bfa;
}
.index-card.is-security {
  --section-color: #fb7185;
}
.index-card.is-media {
  --section-color: #60a5fa;
}
.index-card.is-graphics {
  --section-color: #f59e0b;
}
.index-card.is-data {
  --section-color: #94a3b8;
}
.index-card.is-tooling {
  --section-color: #22d3ee;
}
.index-card.is-runtime {
  --section-color: #c084fc;
}
.index-card-rail {
  position: absolute;
  top: 12px;
  bottom: 12px;
  left: 0;
  width: 2px;
  border-radius: 0 999px 999px 0;
  background: var(--section-color);
  opacity: 0.48;
}
.index-card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}
.index-card-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 30px;
  height: 30px;
  border: 1px solid color-mix(in srgb, var(--section-color) 24%, transparent);
  border-radius: 5px;
  background: color-mix(in srgb, var(--section-color) 7%, transparent);
  color: var(--section-color);
}
.index-card-icon-svg {
  width: 15px;
  height: 15px;
}
.index-card-status {
  color: var(--dt-text-quaternary);
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.2;
}
.index-card-title {
  color: #d2d2c8;
  font-family: var(--font-display);
  font-size: 16px;
  line-height: 1.2;
}
.index-card-description {
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 10px;
  line-height: 1.55;
}
</style>
