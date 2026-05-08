<script setup lang="ts">
import Rail from './Rail.vue'
import Topbar from './Topbar.vue'
</script>

<template>
  <div class="desktop-scope shell">
    <Rail />

    <main class="main">
      <Topbar />
      <div class="body">
        <slot />
      </div>
    </main>
  </div>
</template>

<style scoped>
.shell {
  background: #080808;
  display: flex;
  height: 100dvh;
  width: 100%;
  overflow: hidden;
  font-family: var(--font-mono);
}
.main {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-width: 0;
}
.body {
  flex: 1;
  display: flex;
  overflow: hidden;
}
</style>
