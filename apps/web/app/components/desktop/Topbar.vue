<script setup lang="ts">
import LangSwitcher from '~/components/lang/LangSwitcher.vue'

const route = useRoute()

const routeTitles: Record<string, string> = {
  home: 'Home',
  feed: 'Discovery feed',
  search: 'Catalog',
  sections: 'Web Platform Index',
  section: 'Section',
  saved: 'Saved',
  map: 'Knowledge map',
  courses: 'Mini courses',
  profile: 'Profile',
  settings: 'Settings',
  'topic-slug': 'Topic',
}

const pageTitle = computed(() => routeTitles[route.name] ?? route.name)
</script>

<template>
  <header class="topbar">
    <span class="page-title">{{ pageTitle }}</span>
    <LangSwitcher />
  </header>
</template>

<style scoped>
.topbar {
  height: 46px;
  border-bottom: 1px solid #111;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 18px;
  gap: 10px;
  flex-shrink: 0;
}
.page-title {
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--dt-text-tertiary);
  letter-spacing: 0.1em;
}
</style>
