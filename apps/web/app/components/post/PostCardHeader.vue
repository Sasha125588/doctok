<script setup lang="ts">
import { usePostKind } from '~/composables/usePostKind'

const props = defineProps<{
  kind: string
  topicSlug: string
  totalInTopic?: number
  currentIndex?: number
}>()

const kindConfig = usePostKind(() => props.kind)
</script>

<template>
  <div class="flex items-center justify-between">
    <span class="flex items-center gap-1.5 font-mono text-[0.65rem] text-[var(--text-secondary)]">
      <span
        class="size-[3px] rounded-full"
        :style="{ background: 'currentColor' }"
      />
      {{ topicSlug }}
    </span>
    <div
      v-if="totalInTopic && totalInTopic > 1"
      class="flex items-center gap-1"
    >
      <span
        v-for="i in totalInTopic"
        :key="i"
        class="size-[5px] rounded-full transition-all duration-300"
        :class="i - 1 === currentIndex ? 'scale-140' : ''"
        :style="{
          background:
            i - 1 === currentIndex
              ? kindConfig.cssColor
              : i - 1 < (currentIndex ?? 0)
                ? 'rgba(255,255,255,0.5)'
                : 'rgba(255,255,255,0.15)',
        }"
      />
    </div>
  </div>
</template>
