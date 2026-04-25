<script setup lang="ts">
defineProps<{
  title: string
  actionLabel?: string
}>()
</script>

<template>
  <section class="section">
    <header class="header">
      <div class="title">{{ title }}</div>
      <button
        v-if="actionLabel"
        type="button"
        class="action"
      >
        {{ actionLabel }}
      </button>
    </header>

    <div class="row">
      <slot />
    </div>
  </section>
</template>

<style scoped>
.section {
  display: flex;
  flex-direction: column;
}
.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 9px;
}
.title {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-tertiary);
  letter-spacing: 0.14em;
  text-transform: uppercase;
}
.action {
  border: none;
  background: none;
  padding: 0;
  font-family: var(--font-mono);
  font-size: 8px;
  color: #1a1a1a;
  cursor: pointer;
  transition: color 0.15s ease;
}
.action:hover {
  color: #444;
}
.row {
  display: flex;
  gap: 8px;
  overflow-x: auto;
  padding-bottom: 4px;
}
.row::-webkit-scrollbar {
  height: 2px;
}
.row::-webkit-scrollbar-thumb {
  background: #1a1a1a;
}
</style>
