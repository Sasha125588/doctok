<script setup lang="ts">
import { type PostContentVariant, usePostContentVariant } from '~/composables/usePostContentVariant'

const variant = usePostContentVariant()

const options: { code: PostContentVariant; label: string }[] = [
  { code: 'original', label: 'original' },
  { code: 'ai_simple', label: 'simplified' },
  { code: 'ai_senior', label: 'senior' },
]
</script>

<template>
  <div class="variant-toggle">
    <button
      v-for="opt in options"
      :key="opt.code"
      class="variant-btn"
      :class="{ 'is-active': variant === opt.code }"
      @click="variant = opt.code"
    >
      <span class="label">{{ opt.label }}</span>
    </button>
  </div>
</template>

<style scoped>
.variant-toggle {
  display: flex;
  border: 1px solid #161616;
  border-radius: 4px;
  overflow: hidden;
}
.variant-btn {
  font-family: var(--font-mono);
  font-size: 8px;
  padding: 4px 11px;
  border: none;
  background: none;
  cursor: pointer;
  letter-spacing: 0.08em;
  color: var(--dt-text-tertiary);
  transition: color 0.15s;
}
.variant-btn:hover:not(.is-active) {
  color: #555;
}
.variant-btn.is-active {
  color: var(--kind-summary);
  background: var(--kind-summary-bg);
}
.label {
  position: relative;
}
</style>
