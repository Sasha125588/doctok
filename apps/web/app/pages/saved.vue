<script setup lang="ts">
import DesktopShell from '~/components/desktop/DesktopShell.vue'
import SavedPage from '~/components/desktop/SavedPage.vue'
import { useDesktop } from '~/composables/useDesktop'

const isDesktop = useDesktop()
</script>

<template>
  <DesktopShell v-if="isDesktop">
    <SavedPage />
  </DesktopShell>
</template>
