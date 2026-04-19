<script setup lang="ts">
import { motion } from 'motion-v'

defineProps<{
  open: boolean
  title: string
}>()

const emit = defineEmits<{
  close: []
}>()
</script>

<template>
  <motion.aside
    class="panel"
    :animate="{ width: open ? 210 : 0 }"
    :transition="{ duration: 0.22, ease: 'easeInOut' }"
  >
    <div class="inner" :class="{ 'is-hidden': !open }">
      <header class="header">
        <span class="title">// {{ title }}</span>
        <button class="close" @click="emit('close')">×</button>
      </header>
      <slot />
    </div>
  </motion.aside>
</template>

<style scoped>
.panel {
  border-left: 1px solid var(--dt-panel-border);
  background: #080808;
  overflow: hidden;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
}
.inner {
  width: 210px;
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
  opacity: 1;
  transition: opacity 0.18s;
}
.inner.is-hidden {
  opacity: 0;
  pointer-events: none;
}
.header {
  padding: 11px 13px;
  border-bottom: 1px solid #0e0e0e;
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-shrink: 0;
}
.title {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-tertiary);
  letter-spacing: 0.1em;
}
.close {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--dt-text-quaternary);
  font-size: 16px;
  line-height: 1;
  padding: 0;
  transition: color 0.15s;
}
.close:hover {
  color: #555;
}
</style>
