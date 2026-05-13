<script setup lang="ts">
import type { SectionTrack } from '../-types'

defineProps<{
  track: SectionTrack
  index: number
}>()
</script>

<template>
  <article class="track-card">
    <div class="track-index">{{ String(index + 1).padStart(2, '0') }}</div>
    <div class="track-body">
      <h3 class="track-title">{{ track.title }}</h3>
      <p class="track-description">{{ track.description }}</p>
      <div class="track-steps">
        <span
          v-for="step in track.steps"
          :key="step"
          class="track-step"
        >
          {{ step }}
        </span>
      </div>
    </div>
  </article>
</template>

<style scoped>
.track-card {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 12px;
  padding: 14px;
  border: 1px solid #181818;
  border-radius: 7px;
  background: #0b0b0b;
}
.track-index {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 30px;
  height: 30px;
  border: 1px solid rgba(126, 183, 124, 0.2);
  border-radius: 5px;
  color: #87a983;
  font-family: var(--font-mono);
  font-size: 10px;
}
.track-body {
  display: grid;
  min-width: 0;
  gap: 8px;
}
.track-title {
  color: #d1d1c7;
  font-family: var(--font-display);
  font-size: 15px;
  line-height: 1.25;
  font-weight: 400;
}
.track-description {
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 10px;
  line-height: 1.55;
}
.track-steps {
  display: flex;
  flex-wrap: wrap;
  gap: 5px;
}
.track-step {
  padding: 3px 7px;
  border: 1px solid #1b1b1b;
  border-radius: 999px;
  color: #777;
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.2;
}
</style>
