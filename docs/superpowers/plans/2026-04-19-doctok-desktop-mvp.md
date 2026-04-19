# DocTok Desktop MVP Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port the `doctok_desktop_v6.html` reference to a working desktop-only layout (≥1024px) inside the Nuxt 4 web app, without touching the existing mobile feed.

**Architecture:** `pages/index.vue` branches on viewport width. Below 1024px it renders the existing `<TopicFeed>`. At 1024px+ it renders a new `<DesktopShell>` with rail + sidebar + topbar + focus/browse feed page + slide-in comments/notes panels. Shared API composables stay; new desktop-only components live under `app/components/desktop/`. localStorage holds pinned/recent/saved/notes. No backend changes.

**Tech Stack:** Nuxt 4, Vue 3.5, Tailwind v4, `motion-v` (to be added), `@tanstack/vue-query`, `@vueuse/core`, existing generated `#api` client.

**Spec:** `docs/superpowers/specs/2026-04-19-doctok-desktop-mvp-design.md`
**Visual reference:** `apps/web/design/doctok_desktop_v6.html`

## Testing approach

This frontend has **no unit test infrastructure** (no vitest / @testing-library in `apps/web/package.json`). Adding one is out of scope. Therefore each task ends with **manual browser verification** on `http://localhost:3000` instead of a test run. Follow the system prompt's feature-testing policy: start the dev server, exercise the golden path and edge cases, watch the browser console for errors.

To start the dev server once, from `apps/web/`:
```bash
bun run dev
```
Keep it running across tasks; Nuxt HMR will pick up file changes.

The .NET backend must also be running on `:5005` (see repo `README.md`). If you don't have it locally, mock responses by hitting a deployed API via `NUXT_PUBLIC_API_BASE_URL`.

## Commit discipline

- One commit per completed task.
- Conventional prefixes: `feat(web):`, `chore(web):`, `refactor(web):`, `style(web):`.
- Include the task number in commit message body, e.g. `task 2.3`.
- Never `--no-verify`.

---

## Chunk 1: Foundation

Installs `motion-v`, adds design tokens, creates the viewport-switching entry, and a bare `DesktopShell` placeholder so layout swap is observable.

### Task 1.1: Install motion-v

**Files:**
- Modify: `apps/web/package.json`
- Modify: `bun.lock` (auto)

- [ ] **Step 1:** Run install

```bash
cd apps/web
bun add motion-v
```

- [ ] **Step 2:** Verify `motion-v` appears in `apps/web/package.json` `dependencies`. Version should be `^1.x` or newer.

- [ ] **Step 3:** Start (or restart) dev server to ensure it still boots:

```bash
cd apps/web && bun run dev
```

Expected: no errors, server on `:3000`.

- [ ] **Step 4:** Commit

```bash
cd <repo-root>
git add apps/web/package.json bun.lock
git commit -m "chore(web): add motion-v dependency (task 1.1)"
```

### Task 1.2: Add desktop design tokens and font scope

**Files:**
- Modify: `apps/web/app/assets/css/tailwind.css`

- [ ] **Step 1:** Append new tokens and scope class inside the existing `:root` block and at the end of the file. Apply the edit below.

Inside the `:root { ... }` block, add after the existing kind variables:

```css
  --dt-rail-active-bg: #001f0d;
  --dt-sidebar-border: #111;
  --dt-panel-bg: #060606;
  --dt-panel-border: #141414;
  --dt-text-tertiary: #2a2a2a;
  --dt-text-quaternary: #1a1a1a;
  --kind-summary-bg: #0d1f35;
  --kind-example-bg: #001f10;
  --kind-fact-bg: #1e1400;
```

Append at the end of the file:

```css
.desktop-scope {
  --font-display: Georgia, 'Palatino Linotype', serif;
}
```

- [ ] **Step 2:** Verify: in a browser devtools on `http://localhost:3000`, Computed styles on `:root` include `--dt-panel-bg: #060606`. (Will only apply once the scope is used in Task 1.4, but the variable should be present.)

- [ ] **Step 3:** Commit

```bash
git add apps/web/app/assets/css/tailwind.css
git commit -m "style(web): add desktop design tokens and Georgia scope (task 1.2)"
```

### Task 1.3: Create useDesktop composable

**Files:**
- Create: `apps/web/app/composables/useDesktop.ts`

- [ ] **Step 1:** Write the composable

```ts
import { useMediaQuery } from '@vueuse/core'

export function useDesktop() {
  return useMediaQuery('(min-width: 1024px)')
}
```

- [ ] **Step 2:** No direct test yet — consumed in Task 1.5.

- [ ] **Step 3:** Commit

```bash
git add apps/web/app/composables/useDesktop.ts
git commit -m "feat(web): add useDesktop media-query composable (task 1.3)"
```

### Task 1.4: Create DesktopShell placeholder

**Files:**
- Create: `apps/web/app/components/desktop/DesktopShell.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
// Shell is assembled incrementally in later tasks.
// For now it just proves the viewport switch works.
</script>

<template>
  <div class="desktop-scope bg-background flex h-dvh w-full items-center justify-center">
    <div class="font-mono text-sm text-[var(--text-secondary)]">desktop shell — в розробці</div>
  </div>
</template>
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/DesktopShell.vue
git commit -m "feat(web): add DesktopShell placeholder (task 1.4)"
```

### Task 1.5: Wire viewport switch into index page

**Files:**
- Modify: `apps/web/app/pages/index.vue`

- [ ] **Step 1:** Replace contents with:

```vue
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
```

- [ ] **Step 2:** Verify in browser:
  - Open `http://localhost:3000` at window width ≥1024px → see "desktop shell — в розробці" centered.
  - Shrink the window below 1024px → see existing mobile feed (topic cards, topbar).
  - Resize back and forth without reload; component swaps cleanly.

- [ ] **Step 3:** Commit

```bash
git add apps/web/app/pages/index.vue
git commit -m "feat(web): switch index page on viewport for desktop layout (task 1.5)"
```

---

## Chunk 2: State composables

All state primitives used by the rest of the app. Each is trivial in isolation — write, commit, move on. No visible UI change yet.

### Task 2.1: useFeedView — shared reactive view state

**Files:**
- Create: `apps/web/app/composables/useFeedView.ts`

- [ ] **Step 1:** Write

```ts
export type FeedMode = 'focus' | 'browse'
export type ReadMode = 'simplified' | 'standard' | 'detailed' | 'original'
export type FeedPanel = 'comments' | 'notes' | null

const activeTopicSlug = ref<string | null>(null)
const activePostIndex = ref(0)
const mode = ref<FeedMode>('focus')
const readMode = ref<ReadMode>('simplified')
const activePanel = ref<FeedPanel>(null)
const sidebarHidden = ref(false)

export function useFeedView() {
  return {
    activeTopicSlug,
    activePostIndex,
    mode,
    readMode,
    activePanel,
    sidebarHidden,
  }
}
```

Module-level refs act as a singleton store — every `useFeedView()` call returns the same refs. This is intentional so Sidebar, Topbar, FocusMode all stay in sync without prop drilling or Pinia.

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/composables/useFeedView.ts
git commit -m "feat(web): add useFeedView shared state composable (task 2.1)"
```

### Task 2.2: useTopicHistory — pinned + recent topics

**Files:**
- Create: `apps/web/app/composables/useTopicHistory.ts`

- [ ] **Step 1:** Write

```ts
import { useLocalStorage } from '@vueuse/core'

const RECENT_LIMIT = 5

const pinned = useLocalStorage<string[]>('dt:pinned', [])
const recent = useLocalStorage<string[]>('dt:recent', [])

export function useTopicHistory() {
  function addRecent(slug: string) {
    const next = [slug, ...recent.value.filter((s) => s !== slug)]
    recent.value = next.slice(0, RECENT_LIMIT)
  }

  function togglePin(slug: string) {
    if (pinned.value.includes(slug)) {
      pinned.value = pinned.value.filter((s) => s !== slug)
    } else {
      pinned.value = [...pinned.value, slug]
    }
  }

  function isPinned(slug: string) {
    return pinned.value.includes(slug)
  }

  return { pinned, recent, addRecent, togglePin, isPinned }
}
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/composables/useTopicHistory.ts
git commit -m "feat(web): add useTopicHistory (pinned/recent localStorage) (task 2.2)"
```

### Task 2.3: useSavedPosts — bookmarks

**Files:**
- Create: `apps/web/app/composables/useSavedPosts.ts`

- [ ] **Step 1:** Write

```ts
import { useLocalStorage } from '@vueuse/core'

import type { TopicPostView } from '#api/types.gen'

export interface SavedPost {
  postId: number
  topicSlug: string
  title: string
  kind: string
}

const saved = useLocalStorage<SavedPost[]>('dt:saved', [])

export function useSavedPosts() {
  function isSaved(postId: number) {
    return saved.value.some((s) => s.postId === postId)
  }

  function toggle(post: TopicPostView) {
    const id = +post.id
    if (isSaved(id)) {
      saved.value = saved.value.filter((s) => s.postId !== id)
    } else {
      saved.value = [
        ...saved.value,
        { postId: id, topicSlug: post.topicSlug, title: post.title, kind: post.kind },
      ]
    }
  }

  return { saved, isSaved, toggle }
}
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/composables/useSavedPosts.ts
git commit -m "feat(web): add useSavedPosts localStorage composable (task 2.3)"
```

### Task 2.4: useNotes — per-post notes

**Files:**
- Create: `apps/web/app/composables/useNotes.ts`

- [ ] **Step 1:** Write

```ts
import { useLocalStorage } from '@vueuse/core'

const notes = useLocalStorage<Record<string, string>>('dt:notes', {})

export function useNotes() {
  function get(postId: number | string | null) {
    if (postId == null) return ''
    return notes.value[String(postId)] ?? ''
  }

  function set(postId: number | string, text: string) {
    const trimmed = text.trim()
    const key = String(postId)
    if (trimmed) {
      notes.value = { ...notes.value, [key]: trimmed }
    } else {
      const { [key]: _, ...rest } = notes.value
      notes.value = rest
    }
  }

  function has(postId: number | string | null) {
    if (postId == null) return false
    return !!notes.value[String(postId)]
  }

  return { notes, get, set, has }
}
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/composables/useNotes.ts
git commit -m "feat(web): add useNotes localStorage composable (task 2.4)"
```

### Task 2.5: useReadMode — stub

**Files:**
- Create: `apps/web/app/composables/useReadMode.ts`

- [ ] **Step 1:** Write

```ts
import { useFeedView } from './useFeedView'

export function useReadMode() {
  const { readMode } = useFeedView()
  return { readMode }
}
```

Thin accessor kept for symmetry and future real implementation. `useFeedView` already owns the ref.

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/composables/useReadMode.ts
git commit -m "feat(web): add useReadMode stub accessor (task 2.5)"
```

---

## Chunk 3: Shell chrome

Builds the visible frame (rail + sidebar + topbar) around an empty main area. After this chunk the desktop viewport shows the full chrome even though clicking a topic still does nothing (focus mode comes next).

### Task 3.1: Rail component

**Files:**
- Create: `apps/web/app/components/desktop/Rail.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
const navItems = [
  { key: 'feed', icon: 'lucide:layout-grid', title: 'Стрічка', active: true, disabled: false },
  { key: 'search', icon: 'lucide:search', title: 'Каталог (скоро)', active: false, disabled: true },
  { key: 'courses', icon: 'lucide:book-open', title: 'Міні-курси (скоро)', active: false, disabled: true },
  { key: 'saved', icon: 'lucide:bookmark', title: 'Збережене (скоро)', active: false, disabled: true },
]
const footerItem = {
  key: 'profile',
  icon: 'lucide:user',
  title: 'Профіль (скоро)',
  disabled: true,
}
</script>

<template>
  <nav class="rail">
    <div class="logo">DOC</div>
    <button
      v-for="item in navItems"
      :key="item.key"
      class="rail-btn"
      :class="{ 'is-active': item.active, 'is-disabled': item.disabled }"
      :title="item.title"
      :disabled="item.disabled"
    >
      <Icon :name="item.icon" class="icon" />
    </button>
    <div class="spacer" />
    <button
      class="rail-btn is-disabled"
      :title="footerItem.title"
      :disabled="footerItem.disabled"
    >
      <Icon :name="footerItem.icon" class="icon" />
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
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/Rail.vue
git commit -m "feat(web): add desktop Rail nav (task 3.1)"
```

### Task 3.2: Sidebar component

**Files:**
- Create: `apps/web/app/components/desktop/Sidebar.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
import { motion, AnimatePresence } from 'motion-v'

import { useFeed } from '~/composables/useFeed'
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useTopicHistory } from '~/composables/useTopicHistory'

const { lang } = useLang()
const { state } = useFeed(lang)
const { pinned, recent } = useTopicHistory()
const { activeTopicSlug, sidebarHidden } = useFeedView()

function titleOf(slug: string) {
  return state.topics.value.find((t) => t.slug === slug)?.title ?? slug
}

const sections = computed(() => [
  { label: 'pinned', slugs: pinned.value },
  { label: 'recent', slugs: recent.value },
])

function selectTopic(slug: string) {
  activeTopicSlug.value = slug
}
</script>

<template>
  <motion.aside
    class="sidebar"
    :animate="{ width: sidebarHidden ? 0 : 168, opacity: sidebarHidden ? 0 : 1 }"
    :transition="{ duration: 0.22, ease: 'easeInOut' }"
  >
    <div class="header">
      <div class="title">Recent &amp; Pinned</div>
    </div>
    <div class="list">
      <template v-for="section in sections" :key="section.label">
        <div class="section">{{ section.label }}</div>
        <div
          v-if="!section.slugs.length"
          class="empty"
        >// порожньо</div>
        <div
          v-for="slug in section.slugs"
          :key="section.label + ':' + slug"
          class="item"
          :class="{ 'is-active': slug === activeTopicSlug }"
          @click="selectTopic(slug)"
        >
          <AnimatePresence>
            <motion.span
              v-if="slug === activeTopicSlug"
              layoutId="dt-active-topic-bar"
              class="active-bar"
              :transition="{ type: 'spring', stiffness: 500, damping: 35 }"
            />
          </AnimatePresence>
          <span class="name">{{ titleOf(slug) }}</span>
        </div>
      </template>
    </div>
  </motion.aside>
</template>

<style scoped>
.sidebar {
  border-right: 1px solid var(--dt-sidebar-border);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  flex-shrink: 0;
}
.header {
  padding: 12px 14px 8px;
  border-bottom: 1px solid #0e0e0e;
  flex-shrink: 0;
}
.title {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-tertiary);
  letter-spacing: 0.14em;
  text-transform: uppercase;
  white-space: nowrap;
}
.list {
  overflow-y: auto;
  flex: 1;
  padding: 6px 0;
}
.section {
  font-family: var(--font-mono);
  font-size: 7px;
  color: var(--dt-text-quaternary);
  letter-spacing: 0.12em;
  text-transform: uppercase;
  padding: 10px 12px 4px;
  white-space: nowrap;
}
.empty {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-quaternary);
  padding: 2px 12px 6px;
}
.item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 7px 12px;
  cursor: pointer;
  transition: background 0.12s;
  position: relative;
  white-space: nowrap;
}
.item:hover {
  background: #0c0c0c;
}
.item.is-active {
  background: #0f1a0f;
}
.active-bar {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 2px;
  background: var(--kind-example);
  border-radius: 0 2px 2px 0;
}
.name {
  font-family: var(--font-mono);
  font-size: 11px;
  color: #444;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
}
.item.is-active .name {
  color: #c8e8c0;
}
</style>
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/Sidebar.vue
git commit -m "feat(web): add desktop Sidebar with motion shared layout (task 3.2)"
```

### Task 3.3: Topbar component

**Files:**
- Create: `apps/web/app/components/desktop/Topbar.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
import { motion } from 'motion-v'

import { useFeedView, type FeedMode, type ReadMode } from '~/composables/useFeedView'

const { mode, readMode } = useFeedView()

const modes: FeedMode[] = ['focus', 'browse']
const readModes: ReadMode[] = ['simplified', 'standard', 'detailed', 'original']
</script>

<template>
  <header class="topbar">
    <span class="page-title">feed</span>

    <div class="toggle">
      <button
        v-for="m in modes"
        :key="m"
        class="toggle-btn"
        :class="{ 'is-active': mode === m }"
        @click="mode = m"
      >
        <motion.span
          v-if="mode === m"
          layoutId="dt-mode-active"
          class="active-pill"
          :transition="{ type: 'spring', stiffness: 500, damping: 35 }"
        />
        <span class="label">{{ m }}</span>
      </button>
    </div>

    <div class="read-toggle">
      <button
        v-for="rm in readModes"
        :key="rm"
        class="read-btn"
        :class="{ 'is-active': readMode === rm }"
        @click="readMode = rm"
      >
        <motion.span
          v-if="readMode === rm"
          layoutId="dt-rm-active"
          class="active-pill blue"
          :transition="{ type: 'spring', stiffness: 500, damping: 35 }"
        />
        <span class="label">{{ rm }}</span>
      </button>
    </div>
  </header>
</template>

<style scoped>
.topbar {
  height: 46px;
  border-bottom: 1px solid #111;
  display: flex;
  align-items: center;
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
.toggle,
.read-toggle {
  display: flex;
  border: 1px solid #161616;
  border-radius: 4px;
  overflow: hidden;
}
.read-toggle {
  margin-left: auto;
}
.toggle-btn,
.read-btn {
  font-family: var(--font-mono);
  font-size: 8px;
  padding: 4px 11px;
  border: none;
  background: none;
  cursor: pointer;
  letter-spacing: 0.08em;
  color: var(--dt-text-tertiary);
  position: relative;
  transition: color 0.15s;
}
.read-btn {
  padding: 4px 9px;
  letter-spacing: 0.06em;
  color: #222;
}
.toggle-btn:hover:not(.is-active) {
  color: #555;
}
.toggle-btn.is-active {
  color: var(--kind-example);
}
.read-btn.is-active {
  color: var(--kind-summary);
}
.active-pill {
  position: absolute;
  inset: 0;
  background: var(--dt-rail-active-bg);
  z-index: 0;
}
.active-pill.blue {
  background: var(--kind-summary-bg);
}
.label {
  position: relative;
  z-index: 1;
}
</style>
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/Topbar.vue
git commit -m "feat(web): add desktop Topbar with motion pills (task 3.3)"
```

### Task 3.4: Assemble DesktopShell

**Files:**
- Modify: `apps/web/app/components/desktop/DesktopShell.vue`

- [ ] **Step 1:** Replace with

```vue
<script setup lang="ts">
import Rail from './Rail.vue'
import Sidebar from './Sidebar.vue'
import Topbar from './Topbar.vue'

import { useFeed } from '~/composables/useFeed'
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useTopicHistory } from '~/composables/useTopicHistory'

const { lang } = useLang()
const { state } = useFeed(lang)
const { activeTopicSlug } = useFeedView()
const { addRecent } = useTopicHistory()

// Seed active topic once feed arrives.
watch(
  () => state.topics.value,
  (topics) => {
    if (!activeTopicSlug.value && topics.length) {
      activeTopicSlug.value = topics[0].slug
      addRecent(topics[0].slug)
    }
  },
  { immediate: true }
)

// Track recency whenever active topic changes.
watch(activeTopicSlug, (slug) => {
  if (slug) addRecent(slug)
})
</script>

<template>
  <div class="desktop-scope shell">
    <Rail />
    <Sidebar />
    <main class="main">
      <Topbar />
      <div class="body">
        <div class="placeholder">
          // feed content — наступні задачі
        </div>
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
.placeholder {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--dt-text-tertiary);
  font-size: 11px;
}
</style>
```

- [ ] **Step 2:** Verify in browser at ≥1024px:
  - Left rail (50px) with DOC logo and icons.
  - Sidebar (168px) with empty "pinned" and "recent" sections (first visit) or populated after a topic gets selected.
  - Topbar with "feed" title, focus/browse toggle, read-mode toggle.
  - Center placeholder text "// feed content …".
  - No console errors.
  - When `useFeed` returns topics, the first topic's slug auto-populates in `recent` (LocalStorage key `dt:recent`).

- [ ] **Step 3:** Commit

```bash
git add apps/web/app/components/desktop/DesktopShell.vue
git commit -m "feat(web): assemble DesktopShell with rail+sidebar+topbar (task 3.4)"
```

---

## Chunk 4: Focus mode

Adds the single-card reading view with meta row, title, body, related tags, and vertical action column.

### Task 4.1: CardMeta component

**Files:**
- Create: `apps/web/app/components/desktop/CardMeta.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
import { motion } from 'motion-v'

import { usePostKind } from '~/composables/usePostKind'

const props = defineProps<{
  topicTitle: string
  kind: string
  totalPosts: number
  currentIndex: number
}>()

const kindConfig = usePostKind(() => props.kind)
</script>

<template>
  <div class="meta">
    <span class="topic">{{ topicTitle }}</span>
    <span class="sep">·</span>
    <span
      class="kind-badge"
      :style="{
        color: kindConfig.cssColor,
        background: kindConfig.cssColor + '22',
        borderColor: kindConfig.cssColor + '55',
      }"
    >
      {{ kindConfig.label }}
    </span>
    <div class="dots">
      <motion.div
        v-for="i in totalPosts"
        :key="i"
        class="dot"
        :animate="{
          width: i - 1 === currentIndex ? 18 : 5,
          backgroundColor: i - 1 === currentIndex ? kindConfig.cssColor : '#1a1a1a',
        }"
        :transition="{ duration: 0.22 }"
      />
    </div>
    <span class="source">MDN</span>
  </div>
</template>

<style scoped>
.meta {
  display: flex;
  align-items: center;
  gap: 7px;
  flex-wrap: nowrap;
  overflow: hidden;
}
.topic {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-tertiary);
  white-space: nowrap;
}
.sep {
  font-family: var(--font-mono);
  color: #141414;
  font-size: 8px;
  margin: 0 2px;
}
.kind-badge {
  font-family: var(--font-mono);
  font-size: 8px;
  padding: 2px 8px;
  border-radius: 2px;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  font-weight: 600;
  border: 1px solid;
  flex-shrink: 0;
}
.dots {
  display: flex;
  gap: 3px;
  align-items: center;
  flex-shrink: 0;
}
.dot {
  height: 2px;
  border-radius: 1px;
}
.source {
  font-family: var(--font-mono);
  font-size: 7px;
  padding: 2px 7px;
  border-radius: 2px;
  border: 1px solid #161616;
  color: var(--dt-text-tertiary);
  flex-shrink: 0;
  margin-left: auto;
}
</style>
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/CardMeta.vue
git commit -m "feat(web): add desktop CardMeta with animated progress dots (task 4.1)"
```

### Task 4.2: RelatedTags component

**Files:**
- Create: `apps/web/app/components/desktop/RelatedTags.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useTopicLinks } from '~/composables/useTopicLinks'

const { lang } = useLang()
const { activeTopicSlug } = useFeedView()

const enabled = computed(() => !!activeTopicSlug.value)
const { state } = useTopicLinks(
  {
    query: {
      slug: activeTopicSlug.value ?? '',
      lang: lang.value,
    },
  },
  enabled
)

function go(slug: string) {
  activeTopicSlug.value = slug
}
</script>

<template>
  <div class="wrap">
    <div class="label">→ related</div>
    <div v-if="state.links.value.length" class="tags">
      <button
        v-for="link in state.links.value"
        :key="link.slug"
        class="tag"
        @click="go(link.slug)"
      >
        {{ link.title }}
      </button>
    </div>
    <div
      v-else-if="state.isLoading.value"
      class="loading"
    >…</div>
  </div>
</template>

<style scoped>
.wrap {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 5px;
  min-width: 0;
}
.label {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-quaternary);
  letter-spacing: 0.1em;
}
.tags {
  display: flex;
  gap: 4px;
  flex-wrap: wrap;
}
.tag {
  font-family: var(--font-mono);
  font-size: 9px;
  padding: 3px 8px;
  border-radius: 2px;
  border: 1px solid #141616;
  color: var(--dt-text-tertiary);
  cursor: pointer;
  background: none;
  transition: all 0.15s;
}
.tag:hover {
  border-color: color-mix(in oklab, var(--kind-example) 30%, transparent);
  color: var(--kind-example);
  background: var(--dt-rail-active-bg);
}
.loading {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-quaternary);
}
</style>
```

Note: `useTopicLinks` signature wants `query.slug` as a string — we pass `activeTopicSlug.value ?? ''` and gate with `enabled` ref so no request fires when empty.

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/RelatedTags.vue
git commit -m "feat(web): add desktop RelatedTags from topic links (task 4.2)"
```

### Task 4.3: ActionsColumn component

**Files:**
- Create: `apps/web/app/components/desktop/ActionsColumn.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
import { motion } from 'motion-v'

import type { ReactionValue } from '#api/types.gen'

defineProps<{
  myVote: ReactionValue
  likeCount: number | string
  commentCount: number | string
  isSaved: boolean
  hasNote: boolean
}>()

const emit = defineEmits<{
  onVote: [ReactionValue]
  onToggleSave: []
  onOpenNote: []
  onOpenComments: []
  onShare: []
}>()
</script>

<template>
  <div class="actions">
    <motion.button
      class="btn"
      :class="{ 'is-liked': myVote === 'like' }"
      :while-tap="{ scale: 0.9 }"
      :while-hover="{ scale: 1.08 }"
      :transition="{ type: 'spring', stiffness: 400, damping: 22 }"
      @click="emit('onVote', 'like')"
    >
      <Icon name="lucide:heart" class="icon" />
      <span class="count">{{ likeCount }}</span>
    </motion.button>

    <motion.button
      class="btn"
      :class="{ 'is-saved': isSaved }"
      :while-tap="{ scale: 0.9 }"
      :while-hover="{ scale: 1.08 }"
      :transition="{ type: 'spring', stiffness: 400, damping: 22 }"
      @click="emit('onToggleSave')"
    >
      <Icon name="lucide:bookmark" class="icon" />
      <span class="label">save</span>
    </motion.button>

    <motion.button
      class="btn"
      :class="{ 'is-noted': hasNote }"
      :while-tap="{ scale: 0.9 }"
      :while-hover="{ scale: 1.08 }"
      :transition="{ type: 'spring', stiffness: 400, damping: 22 }"
      @click="emit('onOpenNote')"
    >
      <Icon name="lucide:pencil" class="icon" />
      <span class="label">note</span>
    </motion.button>

    <motion.button
      class="btn"
      :while-tap="{ scale: 0.9 }"
      :while-hover="{ scale: 1.08 }"
      :transition="{ type: 'spring', stiffness: 400, damping: 22 }"
      @click="emit('onOpenComments')"
    >
      <Icon name="lucide:message-square" class="icon" />
      <span class="count">{{ commentCount }}</span>
    </motion.button>

    <motion.button
      class="btn"
      :while-tap="{ scale: 0.9 }"
      :while-hover="{ scale: 1.08 }"
      :transition="{ type: 'spring', stiffness: 400, damping: 22 }"
      @click="emit('onShare')"
    >
      <Icon name="lucide:share-2" class="icon" />
      <span class="label">share</span>
    </motion.button>
  </div>
</template>

<style scoped>
.actions {
  display: flex;
  flex-direction: column;
  gap: 5px;
  align-items: center;
  flex-shrink: 0;
}
.btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  border: none;
  background: none;
  cursor: pointer;
  padding: 2px;
  color: var(--dt-text-tertiary);
  transition: color 0.15s;
}
.btn:hover {
  color: #888;
}
.icon {
  width: 14px;
  height: 14px;
}
.count,
.label {
  font-family: var(--font-mono);
  font-size: 7px;
  color: var(--dt-text-quaternary);
}
.btn.is-liked {
  color: var(--kind-example);
}
.btn.is-liked .count {
  color: var(--kind-example);
}
.btn.is-saved {
  color: var(--kind-fact);
}
.btn.is-noted {
  color: var(--chart-4);
}
</style>
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/ActionsColumn.vue
git commit -m "feat(web): add desktop ActionsColumn with spring taps (task 4.3)"
```

### Task 4.4: FocusMode component

**Files:**
- Create: `apps/web/app/components/desktop/FocusMode.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'

import ActionsColumn from './ActionsColumn.vue'
import CardMeta from './CardMeta.vue'
import RelatedTags from './RelatedTags.vue'
import PostCardBody from '~/components/post/PostCardBody.vue'
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useNotes } from '~/composables/useNotes'
import { useSavedPosts } from '~/composables/useSavedPosts'
import { useTopicPosts } from '~/composables/useTopicPosts'
import { useVote } from '~/composables/useVote'

import type { ReactionValue } from '#api/types.gen'

const { lang } = useLang()
const { activeTopicSlug, activePostIndex, activePanel } = useFeedView()
const { isSaved, toggle: toggleSave } = useSavedPosts()
const { has: hasNote } = useNotes()

const { state: postsState } = useTopicPosts({
  query: {
    slug: activeTopicSlug.value ?? '',
    lang: lang.value,
  },
})

// Refetch when slug changes (query key already includes slug, but the composable
// was built with a snapshot call — we re-instantiate via key on parent).
const activePost = computed(() => postsState.posts.value[activePostIndex.value])

// Local reaction mirror — same pattern as PostCardActions.vue
const localMyVote = ref<ReactionValue>('none')
const localLikeCount = ref(0)
const localDislikeCount = ref(0)

watch(
  activePost,
  (post) => {
    if (!post) return
    localMyVote.value = post.myVote
    localLikeCount.value = +post.likeCount
    localDislikeCount.value = +post.dislikeCount
  },
  { immediate: true }
)

const voteFns = computed(() => {
  if (!activePost.value) return null
  return useVote({
    postId: +activePost.value.id,
    topicSlug: activePost.value.topicSlug,
    localMyVote,
    localLikeCount,
    localDislikeCount,
  }).functions
})

function onVote(value: ReactionValue) {
  voteFns.value?.onVote(value)
}

function onToggleSave() {
  if (activePost.value) toggleSave(activePost.value)
}

function onOpenComments() {
  activePanel.value = activePanel.value === 'comments' ? null : 'comments'
}

function onOpenNote() {
  activePanel.value = activePanel.value === 'notes' ? null : 'notes'
}

async function onShare() {
  if (!activePost.value) return
  const url = `${window.location.origin}/topic/${activePost.value.topicSlug}`
  if (navigator.share) {
    try {
      await navigator.share({ url })
      return
    } catch {
      /* fall through to clipboard */
    }
  }
  try {
    await navigator.clipboard.writeText(url)
  } catch {
    /* ignored */
  }
}

const cardKey = computed(() => `${activeTopicSlug.value}:${activePostIndex.value}`)
</script>

<template>
  <section class="focus">
    <div class="card-area">
      <AnimatePresence mode="wait">
        <motion.article
          v-if="activePost"
          :key="cardKey"
          class="card"
          :initial="{ opacity: 0, x: 14 }"
          :animate="{ opacity: 1, x: 0 }"
          :exit="{ opacity: 0, x: -14 }"
          :transition="{ duration: 0.18, ease: 'easeOut' }"
        >
          <CardMeta
            :topic-title="activePost.topicTitle"
            :kind="activePost.kind"
            :total-posts="postsState.posts.value.length"
            :current-index="activePostIndex"
          />

          <h1 class="title">{{ activePost.title }}</h1>

          <PostCardBody :body-html="activePost.bodyHtml" class="body" />

          <div class="spacer" />

          <div class="bottom">
            <RelatedTags />
            <ActionsColumn
              :my-vote="localMyVote"
              :like-count="localLikeCount"
              :comment-count="activePost.commentCount"
              :is-saved="isSaved(+activePost.id)"
              :has-note="hasNote(activePost.id)"
              @on-vote="onVote"
              @on-toggle-save="onToggleSave"
              @on-open-note="onOpenNote"
              @on-open-comments="onOpenComments"
              @on-share="onShare"
            />
          </div>
        </motion.article>
        <div
          v-else-if="postsState.isLoading.value"
          key="loading"
          class="loading"
        >// завантаження...</div>
        <div
          v-else
          key="empty"
          class="loading"
        >// оберіть тему</div>
      </AnimatePresence>
    </div>
  </section>
</template>

<style scoped>
.focus {
  display: flex;
  flex-direction: column;
  flex: 1;
  overflow: hidden;
}
.card-area {
  flex: 1;
  padding: 18px 28px 18px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  position: relative;
}
.card {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 9px;
  min-height: 0;
}
.title {
  font-family: var(--font-display);
  font-size: 27px;
  font-weight: 700;
  color: #eeece4;
  line-height: 1.2;
  letter-spacing: -0.02em;
  flex-shrink: 0;
}
.body {
  flex: 1;
  min-height: 0;
}
.spacer {
  flex: 0 0 4px;
}
.bottom {
  display: flex;
  align-items: flex-end;
  gap: 12px;
  flex-shrink: 0;
}
.loading {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  color: var(--dt-text-quaternary);
}
</style>
```

**Note on useTopicPosts reactivity:** The composable is instantiated once per mount with a snapshot of `activeTopicSlug.value`. To refresh when slug changes, `FocusMode` must be re-mounted. We handle that by keying the component in the parent (`FeedPage`, Task 5.1). That keeps the composable logic simple and matches how `PostCarousel` is already used in `topic/[...slug].vue`.

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/FocusMode.vue
git commit -m "feat(web): add desktop FocusMode card with reactions/share (task 4.4)"
```

---

## Chunk 5: Browse mode + FeedPage

Adds the grid view and wires the mode switcher so the body area finally renders content.

### Task 5.1: BrowseMode component

**Files:**
- Create: `apps/web/app/components/desktop/BrowseMode.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
import PostKindBadge from '~/components/post/PostKindBadge.vue'
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useTopicPosts } from '~/composables/useTopicPosts'

const { lang } = useLang()
const { activeTopicSlug, activePostIndex, mode } = useFeedView()

const { state } = useTopicPosts({
  query: {
    slug: activeTopicSlug.value ?? '',
    lang: lang.value,
  },
})

function open(index: number) {
  activePostIndex.value = index
  mode.value = 'focus'
}
</script>

<template>
  <section class="browse">
    <div class="area">
      <div class="grid">
        <button
          v-for="(post, i) in state.posts.value"
          :key="post.id"
          class="card"
          :class="{ 'is-current': i === activePostIndex }"
          @click="open(i)"
        >
          <PostKindBadge :kind="post.kind" />
          <div class="title">{{ post.title }}</div>
          <div class="body">{{ post.body }}</div>
        </button>
      </div>
      <div v-if="!state.posts.value.length" class="empty">// нема постів</div>
    </div>
  </section>
</template>

<style scoped>
.browse {
  display: flex;
  flex-direction: column;
  flex: 1;
  overflow: hidden;
}
.area {
  flex: 1;
  padding: 20px 24px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px;
}
.card {
  padding: 12px;
  border: 1px solid #0e0e0e;
  border-radius: 6px;
  background: var(--dt-panel-bg);
  cursor: pointer;
  transition: all 0.15s;
  display: flex;
  flex-direction: column;
  gap: 7px;
  text-align: left;
  font-family: inherit;
}
.card:hover {
  border-color: #161616;
  background: #0c0c0c;
}
.card.is-current {
  border-color: color-mix(in oklab, var(--kind-example) 35%, transparent);
  background: var(--dt-rail-active-bg);
}
.title {
  font-family: var(--font-display);
  font-size: 13px;
  color: #c8c8be;
  line-height: 1.3;
}
.body {
  font-family: var(--font-mono);
  font-size: 9px;
  color: #2e2e2e;
  line-height: 1.6;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.empty {
  font-size: 11px;
  color: var(--dt-text-quaternary);
  padding: 80px 0;
  text-align: center;
}
</style>
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/BrowseMode.vue
git commit -m "feat(web): add desktop BrowseMode grid (task 5.1)"
```

### Task 5.2: FeedPage container

**Files:**
- Create: `apps/web/app/components/desktop/FeedPage.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'

import BrowseMode from './BrowseMode.vue'
import FocusMode from './FocusMode.vue'

import { useFeedView } from '~/composables/useFeedView'

const { mode, activeTopicSlug } = useFeedView()

// Keyed so child composables re-instantiate on slug change (see FocusMode note).
const slugKey = computed(() => `slug:${activeTopicSlug.value}`)
</script>

<template>
  <div class="feed-page">
    <div class="stack">
      <AnimatePresence mode="wait">
        <motion.div
          v-if="mode === 'focus'"
          key="focus"
          class="pane"
          :initial="{ opacity: 0 }"
          :animate="{ opacity: 1 }"
          :exit="{ opacity: 0 }"
          :transition="{ duration: 0.18 }"
        >
          <FocusMode :key="slugKey" />
        </motion.div>
        <motion.div
          v-else
          key="browse"
          class="pane"
          :initial="{ opacity: 0 }"
          :animate="{ opacity: 1 }"
          :exit="{ opacity: 0 }"
          :transition="{ duration: 0.18 }"
        >
          <BrowseMode :key="slugKey" />
        </motion.div>
      </AnimatePresence>
    </div>
    <!-- Side panels inserted in Chunk 6 -->
  </div>
</template>

<style scoped>
.feed-page {
  flex: 1;
  display: flex;
  flex-direction: row;
  overflow: hidden;
  min-width: 0;
}
.stack {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-width: 0;
  position: relative;
}
.pane {
  position: absolute;
  inset: 0;
  display: flex;
}
</style>
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/FeedPage.vue
git commit -m "feat(web): add desktop FeedPage with mode fade transition (task 5.2)"
```

### Task 5.3: Wire FeedPage into DesktopShell

**Files:**
- Modify: `apps/web/app/components/desktop/DesktopShell.vue`

- [ ] **Step 1:** Replace the `<div class="body">` content:

Replace

```vue
      <div class="body">
        <div class="placeholder">
          // feed content — наступні задачі
        </div>
      </div>
```

with

```vue
      <div class="body">
        <FeedPage />
      </div>
```

and add the import:

```ts
import FeedPage from './FeedPage.vue'
```

Delete the `.placeholder` CSS block (no longer used).

- [ ] **Step 2:** Verify in browser at ≥1024px:
  - Focus mode renders the first topic's first post: meta row, serif title, body HTML, related tags, action column.
  - Switching focus → browse in topbar fades to 3-col grid of posts in the active topic.
  - Clicking a grid card returns to focus at that post.
  - Clicking a sidebar topic (after first pinning one — or after `recent` gets populated by any visit) switches the active topic and the focus card slides in.
  - Like button fires `useVote`; the count updates (check network tab for `POST /api/posts/{id}/reactions`).
  - Share button copies/shares the topic URL.
  - No console errors.

- [ ] **Step 3:** Commit

```bash
git add apps/web/app/components/desktop/DesktopShell.vue
git commit -m "feat(web): mount FeedPage inside DesktopShell (task 5.3)"
```

---

## Chunk 6: Side panels + keyboard

Brings comments and notes into sliding side panels and enables keyboard navigation.

### Task 6.1: DesktopSidePanel wrapper

**Files:**
- Create: `apps/web/app/components/desktop/DesktopSidePanel.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
import { motion } from 'motion-v'

defineProps<{
  open: boolean
  title: string
}>()

const emit = defineEmits<{
  close: []
}>()
</script>

<template>
  <motion.aside
    class="panel"
    :animate="{ width: open ? 210 : 0 }"
    :transition="{ duration: 0.22, ease: 'easeInOut' }"
  >
    <div class="inner" :class="{ 'is-hidden': !open }">
      <header class="header">
        <span class="title">// {{ title }}</span>
        <button class="close" @click="emit('close')">×</button>
      </header>
      <slot />
    </div>
  </motion.aside>
</template>

<style scoped>
.panel {
  border-left: 1px solid var(--dt-panel-border);
  background: #080808;
  overflow: hidden;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
}
.inner {
  width: 210px;
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
  opacity: 1;
  transition: opacity 0.18s;
}
.inner.is-hidden {
  opacity: 0;
  pointer-events: none;
}
.header {
  padding: 11px 13px;
  border-bottom: 1px solid #0e0e0e;
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-shrink: 0;
}
.title {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-tertiary);
  letter-spacing: 0.1em;
}
.close {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--dt-text-quaternary);
  font-size: 16px;
  line-height: 1;
  padding: 0;
  transition: color 0.15s;
}
.close:hover {
  color: #555;
}
</style>
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/DesktopSidePanel.vue
git commit -m "feat(web): add DesktopSidePanel slide-in wrapper (task 6.1)"
```

### Task 6.2: NotesPanel

**Files:**
- Create: `apps/web/app/components/desktop/NotesPanel.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
import DesktopSidePanel from './DesktopSidePanel.vue'

import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useNotes } from '~/composables/useNotes'
import { useTopicPosts } from '~/composables/useTopicPosts'

const { lang } = useLang()
const { activeTopicSlug, activePostIndex, activePanel } = useFeedView()
const { get, set } = useNotes()

const { state } = useTopicPosts({
  query: {
    slug: activeTopicSlug.value ?? '',
    lang: lang.value,
  },
})

const activePostId = computed(() => state.posts.value[activePostIndex.value]?.id ?? null)
const text = ref('')

watch(
  () => [activePostId.value, activePanel.value === 'notes'] as const,
  ([id, open]) => {
    if (open && id != null) text.value = get(id)
  },
  { immediate: true }
)

function save() {
  if (activePostId.value != null) {
    set(activePostId.value, text.value)
  }
}

const isOpen = computed(() => activePanel.value === 'notes')

function close() {
  activePanel.value = null
}
</script>

<template>
  <DesktopSidePanel :open="isOpen" title="note" @close="close">
    <div class="area">
      <textarea
        v-model="text"
        class="textarea"
        placeholder="Твої нотатки до цього поста..."
      />
      <div class="hint">// зберігається локально</div>
      <button class="save" @click="save">зберегти →</button>
    </div>
  </DesktopSidePanel>
</template>

<style scoped>
.area {
  flex: 1;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  overflow: hidden;
}
.textarea {
  background: var(--dt-panel-bg);
  border: 1px solid var(--dt-panel-border);
  border-radius: 4px;
  outline: none;
  font-family: var(--font-mono);
  font-size: 10px;
  color: #8ab8e8;
  padding: 7px 9px;
  flex: 1;
  min-height: 120px;
  caret-color: var(--kind-example);
  resize: none;
}
.textarea::placeholder {
  color: var(--dt-text-quaternary);
}
.hint {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-quaternary);
  line-height: 1.6;
}
.save {
  font-family: var(--font-mono);
  font-size: 9px;
  padding: 5px 12px;
  border-radius: 2px;
  cursor: pointer;
  letter-spacing: 0.06em;
  color: var(--chart-4);
  background: #1a0f35;
  border: 1px solid color-mix(in oklab, var(--chart-4) 20%, transparent);
  transition: all 0.15s;
}
.save:hover {
  background: #22143f;
}
</style>
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/NotesPanel.vue
git commit -m "feat(web): add desktop NotesPanel with localStorage (task 6.2)"
```

### Task 6.3: useComments composable

**Files:**
- Create: `apps/web/app/composables/useComments.ts`

- [ ] **Step 1:** Write

```ts
import {
  postsCommentsCreateMutation,
  postsCommentsListOptions,
  postsCommentsListQueryKey,
} from '#api/@tanstack/vue-query.gen'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'

export function useComments(postId: Ref<number | null>) {
  const enabled = computed(() => postId.value != null)
  const queryClient = useQueryClient()

  const listOptions = computed(() => ({
    ...postsCommentsListOptions({
      path: { postId: postId.value ?? 0 },
    }),
    enabled: enabled.value,
  }))

  const query = useQuery(() => listOptions.value)

  const createMutation = useMutation({
    ...postsCommentsCreateMutation(),
    onSuccess: () => {
      if (postId.value != null) {
        queryClient.invalidateQueries({
          queryKey: postsCommentsListQueryKey({ path: { postId: postId.value } }),
        })
      }
    },
  })

  function send(body: string) {
    if (postId.value == null || !body.trim()) return
    createMutation.mutate({
      path: { postId: postId.value },
      body: { body: body.trim() },
    })
  }

  return {
    comments: computed(() => query.data.value ?? []),
    isLoading: query.isLoading,
    isSending: createMutation.isPending,
    send,
  }
}
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/composables/useComments.ts
git commit -m "feat(web): add useComments query+mutation composable (task 6.3)"
```

### Task 6.4: CommentsPanel

**Files:**
- Create: `apps/web/app/components/desktop/CommentsPanel.vue`

- [ ] **Step 1:** Write

```vue
<script setup lang="ts">
import DesktopSidePanel from './DesktopSidePanel.vue'

import { useComments } from '~/composables/useComments'
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useTopicPosts } from '~/composables/useTopicPosts'

const { lang } = useLang()
const { activeTopicSlug, activePostIndex, activePanel } = useFeedView()

const { state } = useTopicPosts({
  query: {
    slug: activeTopicSlug.value ?? '',
    lang: lang.value,
  },
})

const activePostId = computed(() => {
  const post = state.posts.value[activePostIndex.value]
  return post ? +post.id : null
})

const { comments, isLoading, isSending, send } = useComments(activePostId)

const draft = ref('')
const isOpen = computed(() => activePanel.value === 'comments')

function submit() {
  if (!draft.value.trim()) return
  send(draft.value)
  draft.value = ''
}

function close() {
  activePanel.value = null
}

function initial(str: string | undefined) {
  return (str?.[0] ?? '?').toUpperCase()
}

function formatTime(iso: string | undefined) {
  if (!iso) return ''
  try {
    const d = new Date(iso)
    return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  } catch {
    return ''
  }
}
</script>

<template>
  <DesktopSidePanel :open="isOpen" title="comments" @close="close">
    <div class="list">
      <div v-if="isLoading" class="empty">// завантаження...</div>
      <div v-else-if="!comments.length" class="empty">// поки немає коментарів</div>
      <div v-for="c in comments" :key="String(c.id)" class="comment">
        <div class="meta">
          <div class="avatar">{{ initial(c.userId) }}</div>
          <span class="author">{{ c.userId ?? 'user' }}</span>
          <span class="time">{{ formatTime(c.createdAt) }}</span>
        </div>
        <div class="text">{{ c.body }}</div>
      </div>
    </div>
    <div class="input-wrap">
      <textarea
        v-model="draft"
        class="textarea"
        rows="1"
        placeholder="написати..."
        @keydown.enter.exact.prevent="submit"
      />
      <button class="send" :disabled="isSending" @click="submit">→</button>
    </div>
  </DesktopSidePanel>
</template>

<style scoped>
.list {
  flex: 1;
  overflow-y: auto;
  padding: 10px 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.empty {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-quaternary);
}
.comment {
  display: flex;
  flex-direction: column;
  gap: 3px;
}
.meta {
  display: flex;
  align-items: center;
  gap: 6px;
}
.avatar {
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: var(--dt-rail-active-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: var(--font-mono);
  font-size: 7px;
  color: var(--kind-example);
  flex-shrink: 0;
}
.author {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-tertiary);
}
.time {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-quaternary);
  margin-left: auto;
}
.text {
  font-family: var(--font-mono);
  font-size: 10px;
  color: #3a3a3a;
  line-height: 1.6;
  padding-left: 24px;
}
.input-wrap {
  padding: 10px 12px;
  border-top: 1px solid #0e0e0e;
  display: flex;
  gap: 6px;
  flex-shrink: 0;
}
.textarea {
  background: var(--dt-panel-bg);
  border: 1px solid var(--dt-panel-border);
  border-radius: 4px;
  outline: none;
  font-family: var(--font-mono);
  font-size: 10px;
  color: #c8c8c0;
  padding: 7px 9px;
  flex: 1;
  caret-color: var(--kind-example);
  resize: none;
}
.textarea::placeholder {
  color: var(--dt-text-quaternary);
}
.send {
  background: var(--dt-rail-active-bg);
  border: 1px solid color-mix(in oklab, var(--kind-example) 20%, transparent);
  border-radius: 3px;
  color: var(--kind-example);
  font-family: var(--font-mono);
  font-size: 9px;
  padding: 0 9px;
  cursor: pointer;
  transition: all 0.15s;
}
.send:hover:not(:disabled) {
  background: #002a12;
}
.send:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
```

- [ ] **Step 2:** Commit

```bash
git add apps/web/app/components/desktop/CommentsPanel.vue
git commit -m "feat(web): add desktop CommentsPanel with real comments API (task 6.4)"
```

### Task 6.5: Mount side panels in FeedPage

**Files:**
- Modify: `apps/web/app/components/desktop/FeedPage.vue`

- [ ] **Step 1:** Add imports:

```ts
import CommentsPanel from './CommentsPanel.vue'
import NotesPanel from './NotesPanel.vue'
```

And add panels inside the root `.feed-page` after `.stack`:

```vue
    <CommentsPanel />
    <NotesPanel />
```

(Remove the `<!-- Side panels inserted in Chunk 6 -->` comment.)

- [ ] **Step 2:** Verify:
  - Click the comment icon in ActionsColumn → comments panel slides in from the right (210px).
  - List loads real comments via `GET /api/posts/{id}/comments` (check network).
  - Typing and pressing Enter posts via `POST` — the new comment appears.
  - Click the note icon → notes panel replaces comments (only one can be open).
  - Textarea seeds from localStorage (`dt:notes`), save button persists, reloading the page preserves text.
  - Close `×` hides the panel; animation is smooth.

- [ ] **Step 3:** Commit

```bash
git add apps/web/app/components/desktop/FeedPage.vue
git commit -m "feat(web): mount comments/notes panels in FeedPage (task 6.5)"
```

### Task 6.6: Keyboard navigation

**Files:**
- Modify: `apps/web/app/components/desktop/DesktopShell.vue`

- [ ] **Step 1:** Add inside `<script setup lang="ts">`, after existing watches:

```ts
import { useTopicPosts } from '~/composables/useTopicPosts'
import { useFeedView } from '~/composables/useFeedView' // already imported above; keep single import

const { activePostIndex, activePanel, mode: feedMode } = useFeedView()

// Posts for current topic (for ←→ bounds)
const { state: postsState } = useTopicPosts({
  query: {
    slug: activeTopicSlug.value ?? '',
    lang: lang.value,
  },
})

function nextTopic(direction: 1 | -1) {
  const topics = state.topics.value
  const idx = topics.findIndex((t) => t.slug === activeTopicSlug.value)
  if (idx === -1) return
  const next = idx + direction
  if (next >= 0 && next < topics.length) {
    activeTopicSlug.value = topics[next].slug
    activePostIndex.value = 0
    return
  }
  if (direction === 1 && state.hasNextPage.value && !state.isFetchingNextPage.value) {
    const lenBefore = topics.length
    functions.fetchNextPage().then(() => {
      const updated = state.topics.value
      if (updated.length > lenBefore) {
        activeTopicSlug.value = updated[lenBefore].slug
        activePostIndex.value = 0
      }
    })
  }
}

function onKeydown(e: KeyboardEvent) {
  if (!feedMode.value || feedMode.value !== 'focus') return
  if (activePanel.value !== null) return
  const tag = (document.activeElement?.tagName ?? '').toUpperCase()
  if (tag === 'INPUT' || tag === 'TEXTAREA') return

  const len = postsState.posts.value.length

  if (e.key === 'ArrowRight') {
    e.preventDefault()
    activePostIndex.value = Math.min(activePostIndex.value + 1, Math.max(0, len - 1))
  } else if (e.key === 'ArrowLeft') {
    e.preventDefault()
    activePostIndex.value = Math.max(0, activePostIndex.value - 1)
  } else if (e.key === 'ArrowDown') {
    e.preventDefault()
    nextTopic(1)
  } else if (e.key === 'ArrowUp') {
    e.preventDefault()
    nextTopic(-1)
  }
}

onMounted(() => window.addEventListener('keydown', onKeydown))
onBeforeUnmount(() => window.removeEventListener('keydown', onKeydown))
```

Also update the destructure of `useFeed` at the top to include `functions`:

```ts
const { state, functions } = useFeed(lang)
```

- [ ] **Step 2:** Verify:
  - With focus-mode active and no side panel open, `→` / `←` navigates posts inside current topic (stops at ends).
  - `↓` / `↑` switches to next/previous topic (in feed order), reset post index to 0.
  - When on the last loaded topic, pressing `↓` triggers `fetchNextPage` (observe network tab) and jumps into the first newly loaded topic.
  - Pressing arrow keys while typing in a textarea (Notes or Comments) does **not** navigate — the event is consumed by the input naturally; our handler bails out on tag check.
  - Opening comments panel and pressing arrows: no navigation.

- [ ] **Step 3:** Commit

```bash
git add apps/web/app/components/desktop/DesktopShell.vue
git commit -m "feat(web): add keyboard navigation (←→ posts, ↑↓ topics) (task 6.6)"
```

### Task 6.7: Final polish pass

**Files:**
- Modify: `apps/web/app/components/desktop/*.vue` (as needed)

- [ ] **Step 1:** Visual QA checklist in browser at viewport ≥1280px:

  - Rail icons align vertically; active icon has `#001f0d` background.
  - Sidebar scrolls if pinned+recent overflow; active bar animates smoothly between items.
  - Topbar: both toggles have sliding active pill on switch; read-mode clicks change visual only (body unchanged — stub confirmed).
  - Focus card: title uses Georgia serif; meta row progress dots grow/shrink on post change; card fade+slide left-right works on ← / →.
  - Related tags: hover glows green; click navigates to linked topic (if in feed).
  - Actions column: like button toggles to green with server-persisted count; bookmark toggles orange (verify `dt:saved` in localStorage); note icon toggles violet once a note is saved.
  - Comments panel: real API roundtrip; pressing Enter posts; panel closes on ×; opening while another panel is open swaps cleanly.
  - Browse mode: 3-col grid fills; clicking a card returns to focus at that index.
  - No console warnings/errors.
  - Shrink viewport below 1024px → mobile feed remains fully functional.

- [ ] **Step 2:** Fix any regressions discovered. Typical suspects: missed `motion-v` imports, misaligned flex wrappers, scroll clipping on body.

- [ ] **Step 3:** Commit (only if files changed — otherwise skip)

```bash
git add -u apps/web/app/components/desktop/
git commit -m "style(web): desktop MVP polish pass (task 6.7)"
```

---

## Completion

After all tasks:

1. Confirm `git status` is clean.
2. Confirm `bun run lint` and `bun run fmt:check` pass (`apps/web/`). Run `bun run lint:fix` and `bun run fmt` if they don't.
3. Final manual sanity check on both desktop (≥1024px) and mobile (<1024px) viewports.
4. Invoke `superpowers:verification-before-completion` and `superpowers:requesting-code-review` skills before considering the MVP shippable.

**Out-of-scope reminders (do not implement):** Quiz panel, Courses / Saved / Search / Progress pages, term tooltips, read-mode real variants, PWA, deep-link-to-post hash.
