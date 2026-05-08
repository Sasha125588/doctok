<script setup lang="ts">
import { motion } from 'motion-v'

import VariantSelector from '~/components/desktop/VariantSelector.vue'
import { type FeedMode, useFeedRouteState } from '~/composables/useFeedRouteState'

const route = useRoute()
const { mode, setMode } = useFeedRouteState()

const modes: FeedMode[] = ['focus', 'browse']

const isFeed = computed(() => route.name === 'feed')
</script>

<template>
  <header class="topbar">
    <span class="page-title">{{ route.name }}</span>

    <div
      v-if="isFeed"
      class="flex w-full justify-between"
    >
      <div class="toggle">
        <button
          v-for="m in modes"
          :key="m"
          class="toggle-btn"
          :class="{ 'is-active': mode === m }"
          @click="setMode(m)"
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

      <VariantSelector />
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
.toggle {
  display: flex;
  border: 1px solid #161616;
  border-radius: 4px;
  overflow: hidden;
}
.toggle-btn {
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
.toggle-btn:hover:not(.is-active) {
  color: #555;
}
.toggle-btn.is-active {
  color: var(--kind-example);
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
