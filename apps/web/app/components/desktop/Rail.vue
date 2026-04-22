<script setup lang="ts">
const route = useRoute()
const router = useRouter()

interface NavItem {
  key: string
  icon: string
  title: string
  path?: string
  disabled: boolean
}

const navItems: NavItem[] = [
  { key: 'feed', icon: 'lucide:layout-grid', title: 'Стрічка', path: '/', disabled: false },
  { key: 'search', icon: 'lucide:search', title: 'Каталог (скоро)', disabled: true },
  { key: 'courses', icon: 'lucide:book-open', title: 'Міні-курси (скоро)', disabled: true },
  { key: 'saved', icon: 'lucide:bookmark', title: 'Збережене', path: '/saved', disabled: false },
]

const footerItem: NavItem = {
  key: 'profile',
  icon: 'lucide:user',
  title: 'Профіль (скоро)',
  disabled: true,
}

function isActive(item: NavItem) {
  return item.path != null && route.path === item.path
}

function go(item: NavItem) {
  if (item.disabled || !item.path || isActive(item)) return
  router.push(item.path)
}
</script>

<template>
  <nav class="rail">
    <div class="logo">DOC</div>
    <button
      v-for="item in navItems"
      :key="item.key"
      class="rail-btn"
      :class="{ 'is-active': isActive(item), 'is-disabled': item.disabled }"
      :title="item.title"
      :disabled="item.disabled"
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
      :disabled="footerItem.disabled"
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
.rail-btn.is-disabled {
  cursor: not-allowed;
  opacity: 0.6;
}
.icon {
  width: 16px;
  height: 16px;
}
.spacer {
  flex: 1;
}
</style>
