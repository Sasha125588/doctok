<script setup lang="ts">
const route = useRoute()

type RouteName = typeof route.name

interface NavItem {
  key: string
  icon: string
  title: string
  path: RouteName
}

const navItems: NavItem[] = [
  { key: 'feed', icon: 'lucide:layout-grid', title: 'Стрічка', path: 'feed' },
  { key: 'search', icon: 'lucide:search', title: 'Каталог', path: 'search' },
  {
    key: 'courses',
    icon: 'lucide:book-open',
    title: 'Міні-курси (скоро)',
    path: 'courses',
  },
  { key: 'saved', icon: 'lucide:bookmark', title: 'Збережене', path: 'saved' },
]

const footerItem: NavItem = {
  key: 'profile',
  icon: 'lucide:user',
  title: 'Профіль (скоро)',
  path: 'profile',
}

const isActive = (item: NavItem) => route.name === item.path

const go = (item: NavItem) => {
  if (isActive(item)) return
  navigateTo({
    name: item.path,
  })
}
</script>

<template>
  <nav class="rail">
    <div class="logo">DOC</div>
    <button
      v-for="item in navItems"
      :key="item.key"
      class="rail-btn"
      :class="{ 'is-active': isActive(item) }"
      :title="item.title"
      @click="go(item)"
    >
      <Icon
        :name="item.icon"
        class="icon"
      />
    </button>
    <div class="spacer" />
    <button
      class="rail-btn is-disabled"
      :title="footerItem.title"
      @click="go(footerItem)"
    >
      <Icon
        :name="footerItem.icon"
        class="icon"
      />
    </button>
  </nav>
</template>

<style scoped>
.rail {
  width: 50px;
  border-right: 1px solid var(--dt-sidebar-border);
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 14px 0;
  gap: 2px;
  flex-shrink: 0;
}
.logo {
  font-family: var(--font-mono);
  font-size: 10px;
  font-weight: 700;
  color: var(--kind-example);
  letter-spacing: 0.15em;
  writing-mode: vertical-rl;
  transform: rotate(180deg);
  margin-bottom: 6px;
  padding: 8px 0;
}
.rail-btn {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  border: none;
  background: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s;
  color: #333;
}
.rail-btn:hover:not(.is-active):not(.is-disabled) {
  background: #111;
  color: #555;
}
.rail-btn.is-active {
  background: var(--dt-rail-active-bg);
  color: var(--kind-example);
}
.icon {
  width: 16px;
  height: 16px;
}
.spacer {
  flex: 1;
}
</style>
