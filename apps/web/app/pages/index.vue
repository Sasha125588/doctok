<script setup lang="ts">
import DesktopShell from '~/components/desktop/DesktopShell.vue'
import TopicFeed from '~/components/feed/TopicFeed.vue'
import { useDesktop } from '~/composables/useDesktop'

const isDesktop = useDesktop()
</script>

<template>
  <DesktopShell v-if="isDesktop" />
  <TopicFeed v-else />
</template>
