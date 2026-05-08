<script setup lang="ts">
import { Sonner } from './components/ui/sonner'
import { useDesktop } from '~/composables/useDesktop'

const isDesktop = useDesktop()
</script>

<template>
  <Sonner
    position="bottom-right"
    closeButton
  />
  <DesktopLayout v-if="isDesktop">
    <NuxtPage />
  </DesktopLayout>
</template>
