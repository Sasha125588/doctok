<script setup lang="ts">
import type { MockCourse } from '~/lib/searchMockData'

const props = defineProps<{
  course: MockCourse
}>()

const progressWidth = computed(() => `${Math.min(Math.max(props.course.progress, 0), 100)}%`)
</script>

<template>
  <button
    type="button"
    class="card"
  >
    <div class="header">
      <span class="icon">{{ course.icon }}</span>
      <div class="head-copy">
        <div class="name">{{ course.name }}</div>
        <div class="category">{{ course.category }}</div>
      </div>
    </div>

    <div>
      <div class="meta">{{ course.count }} карток</div>

      <div class="progress-row">
        <div class="progress-bar">
          <span
            class="progress-fill"
            :style="{ width: progressWidth }"
          />
        </div>
        <span class="progress-value">{{ course.progress }}%</span>
      </div>
    </div>
  </button>
</template>

<style scoped>
.card {
  width: 150px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 8px;
  padding: 11px;
  border: 1px solid #1a1400;
  border-radius: 6px;
  background: #0e0c00;
  cursor: pointer;
  text-align: left;
  transition:
    border-color 0.15s ease,
    background 0.15s ease;
}
.card:hover {
  border-color: #2a2200;
  background: #141000;
}
.header {
  display: flex;
  align-items: flex-start;
  gap: 8px;
}
.icon {
  font-size: 16px;
  color: #8a6f18;
  line-height: 1;
}
.head-copy {
  min-width: 0;
}
.name {
  font-family: var(--font-mono);
  font-size: 10px;
  color: #9a8442;
  line-height: 1.35;
}
.category {
  font-family: var(--font-mono);
  font-size: 8px;
  color: #58460c;
  letter-spacing: 0.06em;
  text-transform: uppercase;
}
.meta {
  font-family: var(--font-mono);
  font-size: 8px;
  color: #3a3000;
}
.progress-row {
  display: flex;
  align-items: center;
  gap: 8px;
}
.progress-bar {
  flex: 1;
  height: 2px;
  background: #1e1800;
  border-radius: 999px;
  overflow: hidden;
}
.progress-fill {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: #ffb830;
}
.progress-value {
  font-family: var(--font-mono);
  font-size: 8px;
  color: #5d4c10;
}
</style>
