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
      '--normal-bg': '#0a0a0a',
      '--normal-text': '#c8c8c0',
      '--normal-border': '#1a1a1a',
      '--success-bg': '#0a0a0a',
      '--success-text': '#c8c8c0',
      '--success-border': '#1a1a1a',
      '--error-bg': '#0a0a0a',
      '--error-text': '#c8c8c0',
      '--error-border': '#1a1a1a',
      '--info-bg': '#0a0a0a',
      '--info-text': '#c8c8c0',
      '--info-border': '#1a1a1a',
      '--warning-bg': '#0a0a0a',
      '--warning-text': '#c8c8c0',
      '--warning-border': '#1a1a1a',
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
  font-family: var(--font-mono);
  font-size: 12px;
  letter-spacing: 0.2px;
  padding: 10px 14px 10px 12px;
  min-width: 60px;
  width: auto;
  border-left-width: 2px;
  border-left-style: solid;
  border-left-color: #2a2a2a;
  border-radius: 6px;
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
  border: 1px solid #1f3224;
  border-radius: 16px;
  background: #08140d;
  color: var(--kind-example);
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
  border-color: rgba(0, 232, 122, 0.34);
  background: #0b1f13;
  color: #9cffc6;
}

[data-sonner-toast][data-styled='true'] [data-button]:focus-visible {
  outline: 1px solid rgba(0, 232, 122, 0.46);
  outline-offset: 2px;
}

[data-sonner-toast][data-styled='true'][data-type='success'] {
  border-left-color: var(--kind-example);
}

[data-sonner-toast][data-styled='true'][data-type='success'] [data-icon] svg {
  color: var(--kind-example);
}

[data-sonner-toast][data-styled='true'][data-type='error'] {
  border-left-color: var(--destructive);
}

[data-sonner-toast][data-styled='true'][data-type='error'] [data-icon] svg {
  color: var(--destructive);
}

[data-sonner-toast][data-styled='true'][data-type='info'] {
  border-left-color: var(--kind-summary);
}

[data-sonner-toast][data-styled='true'][data-type='info'] [data-icon] svg {
  color: var(--kind-summary);
}

[data-sonner-toast][data-styled='true'][data-type='warning'] {
  border-left-color: var(--kind-fact);
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
  height: 12px;
  width: 12px;
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
  height: 12px;
  width: 12px;
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
