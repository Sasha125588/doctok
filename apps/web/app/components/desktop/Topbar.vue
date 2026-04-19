<script setup lang="ts">
import { motion } from 'motion-v'

import { type FeedMode, type ReadMode, useFeedView } from '~/composables/useFeedView'

const { mode, readMode } = useFeedView()

const modes: FeedMode[] = ['focus', 'browse']
const readModes: ReadMode[] = ['simplified', 'standard', 'detailed', 'original']
</script>

<template>
  <header class="topbar">
    <span class="page-title">feed</span>

    <div class="toggle">
      <button
        v-for="m in modes"
        :key="m"
        class="toggle-btn"
        :class="{ 'is-active': mode === m }"
        @click="mode = m"
      >
        <motion.span
          v-if="mode === m"
          layoutId="dt-mode-active"
          class="active-pill"
          :transition="{ type: 'spring', stiffness: 500, damping: 35 }"
        />
        <span class="label">{{ m }}</span>
      </button>
    </div>

    <div class="read-toggle">
      <button
        v-for="rm in readModes"
        :key="rm"
        class="read-btn"
        :class="{ 'is-active': readMode === rm }"
        @click="readMode = rm"
      >
        <motion.span
          v-if="readMode === rm"
          layoutId="dt-rm-active"
          class="active-pill blue"
          :transition="{ type: 'spring', stiffness: 500, damping: 35 }"
        />
        <span class="label">{{ rm }}</span>
      </button>
    </div>
  </header>
</template>

<style scoped>
.topbar {
  height: 46px;
  border-bottom: 1px solid #111;
  display: flex;
  align-items: center;
  padding: 0 18px;
  gap: 10px;
  flex-shrink: 0;
}
.page-title {
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--dt-text-tertiary);
  letter-spacing: 0.1em;
}
.toggle,
.read-toggle {
  display: flex;
  border: 1px solid #161616;
  border-radius: 4px;
  overflow: hidden;
}
.read-toggle {
  margin-left: auto;
}
.toggle-btn,
.read-btn {
  font-family: var(--font-mono);
  font-size: 8px;
  padding: 4px 11px;
  border: none;
  background: none;
  cursor: pointer;
  letter-spacing: 0.08em;
  color: var(--dt-text-tertiary);
  position: relative;
  transition: color 0.15s;
}
.read-btn {
  padding: 4px 9px;
  letter-spacing: 0.06em;
  color: #222;
}
.toggle-btn:hover:not(.is-active) {
  color: #555;
}
.toggle-btn.is-active {
  color: var(--kind-example);
}
.read-btn.is-active {
  color: var(--kind-summary);
}
.active-pill {
  position: absolute;
  inset: 0;
  background: var(--dt-rail-active-bg);
  z-index: 0;
}
.active-pill.blue {
  background: var(--kind-summary-bg);
}
.label {
  position: relative;
  z-index: 1;
}
</style>
