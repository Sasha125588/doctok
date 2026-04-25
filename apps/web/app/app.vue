<script setup lang="ts">
import TopicFeed from './components/feed/TopicFeed.vue'
import { Sonner } from './components/ui/sonner'
import { useDesktop } from '~/composables/useDesktop'

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
