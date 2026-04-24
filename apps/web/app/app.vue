<script setup lang="ts">
import TopicFeed from './components/feed/TopicFeed.vue'
import { Sonner } from './components/ui/sonner'
import { useSavedPostsMigration } from './composables/useSavedPostsMigration'
import { useDesktop } from '~/composables/useDesktop'

useSavedPostsMigration()

const isDesktop = useDesktop()
</script>

<template>
  <Sonner
    position="bottom-right"
    closeButton
  />
  <DesktopShell v-if="isDesktop">
    <NuxtPage />
  </DesktopShell>
  <TopicFeed v-else />
</template>
