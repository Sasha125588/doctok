<script lang="ts" setup>
import {
  CircleCheckIcon,
  InfoIcon,
  Loader2Icon,
  OctagonXIcon,
  TriangleAlertIcon,
  XIcon,
} from 'lucide-vue-next'
import { Toaster as Sonner } from 'vue-sonner'

import { cn } from '@/lib/utils'

import type { ToasterProps } from 'vue-sonner'

const props = defineProps<ToasterProps>()
</script>

<template>
  <Sonner
    :class="cn('toaster group', props.class)"
    :style="{
      '--normal-bg': '#0b0b0b',
      '--normal-text': '#c8c8c0',
      '--normal-border': '#202020',
      '--success-bg': '#0b0b0b',
      '--success-text': '#c8c8c0',
      '--success-border': '#202020',
      '--error-bg': '#0b0b0b',
      '--error-text': '#c8c8c0',
      '--error-border': '#202020',
      '--info-bg': '#0b0b0b',
      '--info-text': '#c8c8c0',
      '--info-border': '#202020',
      '--warning-bg': '#0b0b0b',
      '--warning-text': '#c8c8c0',
      '--warning-border': '#202020',
    }"
    v-bind="props"
  >
    <template #success-icon>
      <CircleCheckIcon class="size-3.5" />
    </template>
    <template #info-icon>
      <InfoIcon class="size-3.5" />
    </template>
    <template #warning-icon>
      <TriangleAlertIcon class="size-3.5" />
    </template>
    <template #error-icon>
      <OctagonXIcon class="size-3.5" />
    </template>
    <template #loading-icon>
      <div>
        <Loader2Icon class="size-3.5 animate-spin" />
      </div>
    </template>
    <template #close-icon>
      <XIcon class="size-3" />
    </template>
  </Sonner>
</template>

<style>
[data-sonner-toast][data-styled='true'] {
  --toast-rail-color: #2a2a2a;
  --toast-rail-glow: rgba(255, 255, 255, 0.018);
  position: relative;
  overflow: hidden;
  font-family: var(--font-mono);
  font-size: 12px;
  line-height: 1.45;
  letter-spacing: 0.2px;
  padding: 12px 14px 12px 15px;
  min-width: 60px;
  min-height: 48px;
  width: auto;
  border-radius: 6px;
  background:
    linear-gradient(90deg, var(--toast-rail-glow), transparent 46px), var(--normal-bg) !important;
}

[data-sonner-toast][data-styled='true']::before {
  position: absolute;
  top: 10px;
  bottom: 10px;
  left: 0;
  width: 2px;
  border-radius: 0 999px 999px 0;
  background: var(--toast-rail-color);
  content: '';
  opacity: 0.72;
}

[data-sonner-toast][data-styled='true'] [data-title],
[data-sonner-toast][data-styled='true'] [data-description] {
  color: #c8c8c0;
}

[data-sonner-toast][data-styled='true'] [data-content] {
  min-width: 0;
}

[data-sonner-toast][data-styled='true'] [data-button] {
  height: 26px;
  margin-inline: 4px;
  padding: 0 12px;
  border: 1px solid rgba(126, 183, 124, 0.24);
  border-radius: 16px;
  background: rgba(126, 183, 124, 0.075);
  color: #93bd8f;
  cursor: pointer;
  flex-shrink: 0;
  font-family: var(--font-mono);
  font-size: 10px;
  font-weight: 400;
  line-height: 1;
  white-space: nowrap;
  transition:
    border-color 0.15s,
    background 0.15s,
    color 0.15s;
}

[data-sonner-toast][data-styled='true'] [data-button]:hover {
  border-color: rgba(126, 183, 124, 0.38);
  background: rgba(126, 183, 124, 0.12);
  color: #b5d7b0;
}

[data-sonner-toast][data-styled='true'] [data-button]:focus-visible {
  outline: 1px solid rgba(126, 183, 124, 0.46);
  outline-offset: 2px;
}

[data-sonner-toast][data-styled='true'][data-type='success'] {
  --toast-rail-color: var(--kind-example);
  --toast-rail-glow: rgba(34, 197, 94, 0.052);
}

[data-sonner-toast][data-styled='true'][data-type='success'] [data-icon] svg {
  color: var(--kind-example);
}

[data-sonner-toast][data-styled='true'][data-type='error'] {
  --toast-rail-color: var(--destructive);
  --toast-rail-glow: rgba(248, 113, 113, 0.06);
}

[data-sonner-toast][data-styled='true'][data-type='error'] [data-icon] svg {
  color: var(--destructive);
}

[data-sonner-toast][data-styled='true'][data-type='info'] {
  --toast-rail-color: var(--kind-summary);
  --toast-rail-glow: rgba(59, 130, 246, 0.052);
}

[data-sonner-toast][data-styled='true'][data-type='info'] [data-icon] svg {
  color: var(--kind-summary);
}

[data-sonner-toast][data-styled='true'][data-type='warning'] {
  --toast-rail-color: var(--kind-fact);
  --toast-rail-glow: rgba(249, 115, 22, 0.052);
}

[data-sonner-toast][data-styled='true'][data-type='warning'] [data-icon] svg {
  color: var(--kind-fact);
}

[data-sonner-toast][data-styled='true'][data-type='loading'] [data-icon] svg {
  color: var(--dt-text-tertiary);
}

[data-sonner-toast][data-styled='true'] [data-close-button] {
  position: static;
  order: 3;
  margin-left: auto;
  height: 22px;
  width: 22px;
  padding: 0;
  border: 0;
  border-radius: 0;
  background: transparent;
  box-shadow: none;
  transform: none;
  opacity: 0.55;
  color: var(--dt-text-quaternary);
  flex-shrink: 0;
  pointer-events: auto;
  cursor: pointer;
  transition:
    opacity 120ms ease-out,
    color 120ms ease-out;
}

[data-sonner-toast][data-styled='true'] [data-close-button] svg {
  height: 14px;
  width: 14px;
}

[data-sonner-toast][data-styled='true']:hover [data-close-button],
[data-sonner-toast][data-styled='true'] [data-close-button]:hover {
  opacity: 1;
  color: var(--dt-text-tertiary);
}

[data-sonner-toast][data-styled='true']:hover [data-close-button]:hover {
  background: transparent;
  border-color: transparent;
}

[data-sonner-toast][data-styled='true'] [data-close-button]:focus-visible {
  opacity: 1;
  color: var(--dt-text-tertiary);
  background: transparent;
  border-color: transparent;
  outline: 1px solid rgba(200, 200, 192, 0.28);
  outline-offset: 3px;
}
</style>
