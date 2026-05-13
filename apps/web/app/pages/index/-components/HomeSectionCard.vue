<script setup lang="ts">
import type { HomeSection } from '../-types'

defineProps<{
  section: HomeSection
}>()
</script>

<template>
  <NuxtLink
    class="section-card"
    :class="`is-${section.tone}`"
    :to="`/sections/${section.slug}`"
  >
    <span class="section-card-accent" />
    <span class="section-card-top">
      <span class="section-card-icon">
        <Icon
          :name="section.icon"
          class="section-card-icon-svg"
        />
      </span>
      <span class="section-card-eyebrow">{{ section.eyebrow }}</span>
    </span>
    <span class="section-card-title">{{ section.title }}</span>
    <span class="section-card-description">{{ section.description }}</span>
    <span class="section-card-tracks">
      <span
        v-for="track in section.tracks"
        :key="track"
        class="section-card-track"
      >
        {{ track }}
      </span>
    </span>
  </NuxtLink>
</template>

<style scoped>
.section-card {
  --section-color: var(--kind-example);
  position: relative;
  display: grid;
  min-height: 172px;
  gap: 10px;
  overflow: hidden;
  padding: 14px;
  border: 1px solid #181818;
  border-radius: 7px;
  background: #0b0b0b;
  color: inherit;
  text-decoration: none;
  transition:
    border-color 0.16s,
    background 0.16s,
    transform 0.16s;
}
.section-card:hover {
  border-color: #252525;
  background: #101010;
  transform: translateY(-1px);
}
.section-card.is-api {
  --section-color: var(--kind-summary);
}
.section-card.is-js {
  --section-color: var(--kind-tip);
}
.section-card.is-css {
  --section-color: var(--kind-example);
}
.section-card.is-html {
  --section-color: var(--kind-fact);
}
.section-card.is-http {
  --section-color: #38bdf8;
}
.section-card.is-a11y {
  --section-color: #f472b6;
}
.section-card.is-perf {
  --section-color: #14b8a6;
}
.section-card-accent {
  position: absolute;
  top: 12px;
  bottom: 12px;
  left: 0;
  width: 2px;
  border-radius: 0 999px 999px 0;
  background: var(--section-color);
  opacity: 0.52;
}
.section-card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}
.section-card-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 30px;
  height: 30px;
  border: 1px solid color-mix(in srgb, var(--section-color) 26%, transparent);
  border-radius: 5px;
  background: color-mix(in srgb, var(--section-color) 8%, transparent);
  color: var(--section-color);
}
.section-card-icon-svg {
  width: 15px;
  height: 15px;
}
.section-card-eyebrow {
  color: var(--dt-text-quaternary);
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.2;
}
.section-card-title {
  color: #d4d4ca;
  font-family: var(--font-display);
  font-size: 18px;
  line-height: 1.2;
}
.section-card-description {
  color: #787f76;
  font-family: var(--font-mono);
  font-size: 11px;
  line-height: 1.55;
}
.section-card-tracks {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 5px;
  margin-top: auto;
}
.section-card-track {
  padding: 3px 7px;
  border: 1px solid #1a1a1a;
  border-radius: 999px;
  color: #777;
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.2;
}
</style>
