<script setup lang="ts">
import { usePostKind } from '~/composables/usePostKind'

const props = defineProps<{ kind: string }>()
const kindConfig = usePostKind(() => props.kind)
</script>

<template>
  <div
    class="inline-flex w-max items-center gap-1.5 rounded border px-2.5 py-1 font-mono text-[0.65rem] font-medium tracking-widest uppercase"
    :style="{
      color: kindConfig.cssColor,
      borderColor: kindConfig.cssColor + '44',
      background: kindConfig.cssColor + '11',
    }"
  >
    <span
      class="size-[5px] rounded-full"
      :style="{
        background: kindConfig.cssColor,
        animation: 'pulse-dot 2s infinite',
      }"
    />
    {{ kindConfig.label }}
  </div>
</template>
