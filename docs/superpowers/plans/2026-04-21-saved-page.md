# Saved Page Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a desktop `/saved` route that lists every bookmarked post as a card grid, with click-to-focus-mode handoff and in-place remove.

**Architecture:** New Nuxt page `/saved` renders the existing `DesktopShell` with a new `SavedPage` inside its (newly-added) default slot. Data comes from the existing `useSavedPosts` localStorage composable (extended with `savedAt` for sorting). A new `pendingPostId` handoff marker in `useFeedView` lets the saved page click land on the exact post in `FocusMode` after navigation to `/`.

**Tech Stack:** Nuxt 4, Vue 3.5, `motion-v` for enter/leave animations, `@vueuse/core` `useLocalStorage`, Nuxt file-based routing, `@nuxt/icon` (lucide set). No unit test harness exists in this workspace; the automated gate is `oxlint` + `oxfmt`, and each task lists manual acceptance steps for behavior verification.

**Spec:** See [`docs/superpowers/specs/2026-04-21-saved-page-design.md`](../specs/2026-04-21-saved-page-design.md) — read it before starting.

**Working directory:** `apps/web/` (all commands assume this unless stated otherwise).

---

## File Structure

New files:

- `apps/web/app/pages/saved.vue` — route wrapper. Renders `<DesktopShell><SavedPage /></DesktopShell>` when on desktop; falls through (no UI) on non-desktop for now (mobile out of scope).
- `apps/web/app/components/desktop/SavedPage.vue` — main view: header with count, grid of `SavedCard`, empty state. Owns its own scroll container.
- `apps/web/app/components/desktop/SavedCard.vue` — one saved post: kind badge, title, topic label, hover-revealed remove button.

Modified files:

- `apps/web/app/composables/useSavedPosts.ts` — add `savedAt: number` to `SavedPost`, set `Date.now()` on add, expose sorted-desc derived list, stay read-safe for legacy entries missing `savedAt`.
- `apps/web/app/composables/useFeedView.ts` — add `pendingPostId: Ref<number | null>`.
- `apps/web/app/components/desktop/DesktopShell.vue` — content becomes a `default` slot. Shell keeps Rail + Sidebar + Topbar + `<main>` frame; the main body renders `<slot />` with no fallback.
- `apps/web/app/pages/index.vue` — pass `<FeedPage />` into the shell slot (was implicit child).
- `apps/web/app/components/desktop/Rail.vue` — nav items gain `path`, use `useRoute().path` for active state, call `router.push(path)` on click. Enable `saved` with `path: '/saved'`.
- `apps/web/app/components/desktop/Topbar.vue` — route-aware: on `/saved` show static `saved` title, hide focus/browse pills and read-mode toggle.
- `apps/web/app/components/desktop/FocusMode.vue` — add a `watchEffect` that consumes `pendingPostId` when posts are loaded, setting `activePostIndex` to the matching index (or 0 if not found), then clears the marker.

---

## Chunk 1: Foundation — state, types, shell slot

Scope: non-visual groundwork. After this chunk the app still behaves identically on `/` (no regressions), but `SavedPost` carries `savedAt`, `useFeedView` exposes `pendingPostId`, and `DesktopShell` is content-agnostic.

### Task 1.1: Extend `useSavedPosts` with `savedAt`

**Files:**
- Modify: `apps/web/app/composables/useSavedPosts.ts`

- [ ] **Step 1: Update the `SavedPost` type**

Open `apps/web/app/composables/useSavedPosts.ts`. Replace the `SavedPost` interface:

```ts
export interface SavedPost {
  postId: number
  topicSlug: string
  title: string
  kind: string
  // Optional to stay compatible with legacy localStorage entries written before
  // this field existed. New entries always set it via Date.now().
  savedAt?: number
}
```

- [ ] **Step 2: Set `savedAt` on add, expose sorted-desc list, expose direct `remove`, leave legacy entries readable**

Replace the whole exported function with:

```ts
export function useSavedPosts() {
  function isSaved(postId: number) {
    return saved.value.some((s) => s.postId === postId)
  }

  function toggle(post: TopicPostView) {
    const id = +post.id
    if (isSaved(id)) {
      remove(id)
    } else {
      saved.value = [
        ...saved.value,
        {
          postId: id,
          topicSlug: post.topicSlug,
          title: post.title,
          kind: post.kind,
          savedAt: Date.now(),
        },
      ]
    }
  }

  function remove(postId: number) {
    saved.value = saved.value.filter((s) => s.postId !== postId)
  }

  // Newest first. Legacy entries (no savedAt) default to 0 and sink to the end.
  // No write-back to localStorage; legacy entries stay legacy until toggled.
  const sorted = computed<SavedPost[]>(() =>
    [...saved.value].sort((a, b) => (b.savedAt ?? 0) - (a.savedAt ?? 0))
  )

  return { saved, sorted, isSaved, toggle, remove }
}
```

- [ ] **Step 3: Verify lint + typecheck passes**

```bash
cd apps/web && pnpm lint && pnpm fmt:check
```

Expected: no errors. (Any call sites that used `saved` directly still work; `sorted` is opt-in for new consumers.)

- [ ] **Step 4: Manual smoke-check existing feed bookmark toggle still works**

```bash
cd apps/web && pnpm dev
```

In a browser: open `/`, bookmark a post via the existing ActionsColumn, reload, confirm it stays saved (unchanged behavior). Kill dev.

- [ ] **Step 5: Commit**

```bash
git add apps/web/app/composables/useSavedPosts.ts
git commit -m "feat(web): add savedAt + sorted-desc view to useSavedPosts"
```

---

### Task 1.2: Add `pendingPostId` to `useFeedView`

**Files:**
- Modify: `apps/web/app/composables/useFeedView.ts`

- [ ] **Step 1: Add the state and return it**

Open `apps/web/app/composables/useFeedView.ts`. Inside `useFeedView()` add before the `return`:

```ts
const pendingPostId = useState<number | null>('pendingPostId', () => null)
```

And include `pendingPostId` in the returned object:

```ts
return {
  activeTopicSlug,
  activePostIndex,
  mode,
  readMode,
  activePanel,
  sidebarHidden,
  activeTopicPostCount,
  pendingPostId,
}
```

- [ ] **Step 2: Verify lint**

```bash
cd apps/web && pnpm lint
```

Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add apps/web/app/composables/useFeedView.ts
git commit -m "feat(web): add pendingPostId handoff marker to useFeedView"
```

---

### Task 1.3: Make `DesktopShell` content-agnostic via default slot

Current state: `DesktopShell.vue` renders `<FeedPage />` directly inside its `.body` wrapper. We're swapping that for `<slot />` with no fallback, then updating `index.vue` to pass `<FeedPage />` explicitly.

**Files:**
- Modify: `apps/web/app/components/desktop/DesktopShell.vue`
- Modify: `apps/web/app/pages/index.vue`

- [ ] **Step 1: Remove `FeedPage` import and usage from `DesktopShell`, add `<slot />`**

In `DesktopShell.vue`:

Delete this import line:
```ts
import FeedPage from './FeedPage.vue'
```

Replace the `<div class="body"><FeedPage /></div>` block with:

```html
<div class="body">
  <slot />
</div>
```

Leave every other part of `DesktopShell.vue` untouched (the feed-specific composables and keyboard handler it currently owns stay — they're shared state consumers, not FeedPage-specific). They keep reading `useFeedView`, which is unchanged in behavior when the slot contains anything other than `FeedPage`.

- [ ] **Step 2: Update `index.vue` to pass `FeedPage` into the slot**

Replace the entire file content:

```vue
<script setup lang="ts">
import DesktopShell from '~/components/desktop/DesktopShell.vue'
import FeedPage from '~/components/desktop/FeedPage.vue'
import TopicFeed from '~/components/feed/TopicFeed.vue'
import { useDesktop } from '~/composables/useDesktop'

const isDesktop = useDesktop()
</script>

<template>
  <DesktopShell v-if="isDesktop">
    <FeedPage />
  </DesktopShell>
  <TopicFeed v-else />
</template>
```

- [ ] **Step 3: Verify lint + fmt**

```bash
cd apps/web && pnpm lint && pnpm fmt:check
```

Expected: no errors.

- [ ] **Step 4: Manual verification — feed still works end-to-end**

```bash
cd apps/web && pnpm dev
```

In browser on `/`:
1. Sidebar topics render, selecting topics still works.
2. Focus mode: arrow keys cycle posts; ↑↓ cycles topics.
3. Browse mode switch from topbar still works.
4. Comments / notes panels still open via ActionsColumn.

If any of these regressed: the shell still owns state the old code assumed was local to `FeedPage`. Check `DesktopShell.vue` still wires its keyboard handler and watchers.

Kill dev.

- [ ] **Step 5: Commit**

```bash
git add apps/web/app/components/desktop/DesktopShell.vue apps/web/app/pages/index.vue
git commit -m "refactor(web): make DesktopShell content-agnostic via default slot"
```

---

## Chunk 2: The saved page — card, page, route

Scope: render the actual `/saved` page end-to-end with click-to-navigate and remove, but with the Rail still pointing nowhere. Navigation from Rail comes in Chunk 3.

### Task 2.1: Build `SavedCard.vue`

**Files:**
- Create: `apps/web/app/components/desktop/SavedCard.vue`

- [ ] **Step 1: Create the component**

Create `apps/web/app/components/desktop/SavedCard.vue` with:

```vue
<script setup lang="ts">
import PostKindBadge from '~/components/post/PostKindBadge.vue'
import { useFeedView } from '~/composables/useFeedView'
import { useSavedPosts, type SavedPost } from '~/composables/useSavedPosts'

const props = defineProps<{ post: SavedPost }>()

const router = useRouter()
const { activeTopicSlug, pendingPostId, mode } = useFeedView()
const { remove } = useSavedPosts()

function open() {
  activeTopicSlug.value = props.post.topicSlug
  pendingPostId.value = props.post.postId
  mode.value = 'focus'
  router.push('/')
}

function onRemove() {
  remove(props.post.postId)
}
</script>

<template>
  <button
    class="card"
    type="button"
    @click="open"
  >
    <PostKindBadge :kind="post.kind" />
    <div class="title">{{ post.title }}</div>
    <div class="topic">// topic: {{ post.topicSlug }}</div>
    <button
      class="remove"
      type="button"
      title="видалити зі збережених"
      @click.stop="onRemove"
    >
      <Icon
        name="lucide:bookmark-minus"
        class="remove-icon"
      />
    </button>
  </button>
</template>

<style scoped>
.card {
  position: relative;
  padding: 12px;
  border: 1px solid #0e0e0e;
  border-radius: 6px;
  background: var(--dt-panel-bg);
  cursor: pointer;
  transition: all 0.15s;
  display: flex;
  flex-direction: column;
  gap: 7px;
  min-height: 140px;
  min-width: 0;
  text-align: left;
  font-family: inherit;
  word-break: break-word;
}
.card:hover {
  border-color: #161616;
  background: #0c0c0c;
}
.card:hover .remove {
  opacity: 1;
}
.title {
  font-family: var(--font-display);
  font-size: 13px;
  color: #dadacf;
  line-height: 1.3;
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 3;
  line-clamp: 3;
  overflow: hidden;
}
.topic {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-tertiary);
  margin-top: auto;
}
.remove {
  position: absolute;
  top: 8px;
  right: 8px;
  width: 22px;
  height: 22px;
  border-radius: 4px;
  border: none;
  background: rgba(0, 0, 0, 0.4);
  color: var(--dt-text-tertiary);
  cursor: pointer;
  opacity: 0;
  transition: opacity 0.15s, color 0.15s;
  display: flex;
  align-items: center;
  justify-content: center;
}
.remove:hover {
  color: #d88;
}
.remove-icon {
  width: 12px;
  height: 12px;
}
</style>
```

- [ ] **Step 2: Verify lint + fmt**

```bash
cd apps/web && pnpm lint && pnpm fmt:check
```

Expected: no errors. If `oxlint` complains about the `as never` cast, acceptable to replace with a narrow helper; the goal is not to re-shape `toggle`'s signature in this task.

- [ ] **Step 3: Commit**

```bash
git add apps/web/app/components/desktop/SavedCard.vue
git commit -m "feat(web): add desktop SavedCard with click-handoff and remove"
```

---

### Task 2.2: Build `SavedPage.vue`

**Files:**
- Create: `apps/web/app/components/desktop/SavedPage.vue`

- [ ] **Step 1: Create the component**

```vue
<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'

import SavedCard from './SavedCard.vue'
import { useSavedPosts } from '~/composables/useSavedPosts'

const { sorted } = useSavedPosts()
</script>

<template>
  <section class="saved">
    <header class="header">
      <div class="title">// saved</div>
      <div class="count">{{ sorted.length }} posts</div>
    </header>
    <div
      v-if="!sorted.length"
      class="empty"
    >
      <div>// тут порожньо</div>
      <NuxtLink
        to="/"
        class="empty-link"
      >
        // збережи свій перший пост у стрічці →
      </NuxtLink>
    </div>
    <div
      v-else
      class="grid"
    >
      <AnimatePresence>
        <motion.div
          v-for="(post, i) in sorted"
          :key="post.postId"
          :initial="{ opacity: 0, y: 4 }"
          :animate="{ opacity: 1, y: 0 }"
          :exit="{ opacity: 0, y: -4 }"
          :transition="{ duration: 0.18, delay: Math.min(i, 10) * 0.02 }"
        >
          <SavedCard :post="post" />
        </motion.div>
      </AnimatePresence>
    </div>
  </section>
</template>

<style scoped>
.saved {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  padding: 20px 24px;
}
.header {
  display: flex;
  align-items: baseline;
  gap: 12px;
  margin-bottom: 16px;
  flex-shrink: 0;
}
.title {
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--dt-text-tertiary);
  letter-spacing: 0.1em;
}
.count {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-quaternary);
}
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 12px;
}
.empty {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--dt-text-quaternary);
}
.empty-link {
  color: var(--dt-text-quaternary);
  text-decoration: none;
  transition: color 0.15s;
}
.empty-link:hover {
  color: var(--kind-example);
}
</style>
```

- [ ] **Step 2: Verify lint + fmt**

```bash
cd apps/web && pnpm lint && pnpm fmt:check
```

- [ ] **Step 3: Commit**

```bash
git add apps/web/app/components/desktop/SavedPage.vue
git commit -m "feat(web): add SavedPage grid with empty state"
```

---

### Task 2.3: Create `/saved` route

**Files:**
- Create: `apps/web/app/pages/saved.vue`

- [ ] **Step 1: Create the page**

```vue
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
```

- [ ] **Step 2: Verify lint + fmt**

```bash
cd apps/web && pnpm lint && pnpm fmt:check
```

- [ ] **Step 3: Manual verification**

```bash
cd apps/web && pnpm dev
```

Visit `http://localhost:3000/saved` directly in the browser (Rail still can't navigate there yet):

1. Page renders `DesktopShell` frame (Rail + Sidebar + Topbar + main).
2. If no saved posts exist → empty state with working link to `/`.
3. If saved posts exist (save a few from `/` first, then navigate back via URL bar) → grid renders, newest first, correct titles + topic labels + kind badges.
4. Hover a card → remove icon appears.
5. Click remove → card fades out, count decreases.
6. Click a card body → lands on `/`, `FocusMode` opens for the correct topic, but the post index lands on 0 (handoff consumer is not wired yet — Task 3.3 fixes this). This is the expected intermediate state.

Kill dev.

- [ ] **Step 4: Commit**

```bash
git add apps/web/app/pages/saved.vue
git commit -m "feat(web): add /saved route"
```

---

## Chunk 3: Shell integration and handoff

Scope: wire Rail + Topbar for `/saved`, then wire the `pendingPostId` consumer so clicking a saved card lands on the exact post.

### Task 3.1: Route-aware `Rail.vue`

**Files:**
- Modify: `apps/web/app/components/desktop/Rail.vue`

- [ ] **Step 1: Replace the component with the route-aware version**

```vue
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

const footerItem = {
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
/* unchanged — keep the existing <style scoped> block verbatim */
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

- [ ] **Step 2: Verify lint + fmt**

```bash
cd apps/web && pnpm lint && pnpm fmt:check
```

- [ ] **Step 3: Manual verification**

```bash
cd apps/web && pnpm dev
```

1. On `/`: Rail `feed` is highlighted.
2. Click Rail `saved` → URL becomes `/saved`, `saved` highlighted, `feed` no longer highlighted.
3. Click Rail `feed` → back to `/`.
4. On `/topic/<slug>` (open any topic via sidebar): neither `feed` nor `saved` is highlighted — documented MVP fallback.

Kill dev.

- [ ] **Step 4: Commit**

```bash
git add apps/web/app/components/desktop/Rail.vue
git commit -m "feat(web): make Rail route-aware and enable /saved navigation"
```

---

### Task 3.2: Route-aware `Topbar.vue`

**Files:**
- Modify: `apps/web/app/components/desktop/Topbar.vue`

- [ ] **Step 1: Gate mode/read-mode controls and page title on route**

Replace `Topbar.vue` with:

```vue
<script setup lang="ts">
import { motion } from 'motion-v'

import { type FeedMode, type ReadMode, useFeedView } from '~/composables/useFeedView'

const route = useRoute()
const { mode, readMode } = useFeedView()

const modes: FeedMode[] = ['focus', 'browse']
const readModes: ReadMode[] = ['simplified', 'standard', 'detailed', 'original']

const isFeed = computed(() => route.path === '/')

const title = computed(() => {
  if (route.path === '/saved') return 'saved'
  return 'feed'
})
</script>

<template>
  <header class="topbar">
    <span class="page-title">{{ title }}</span>

    <template v-if="isFeed">
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
    </template>
  </header>
</template>

<style scoped>
/* unchanged — keep existing styles verbatim */
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

- [ ] **Step 2: Verify lint + fmt**

```bash
cd apps/web && pnpm lint && pnpm fmt:check
```

- [ ] **Step 3: Manual verification**

```bash
cd apps/web && pnpm dev
```

1. On `/`: Topbar shows `feed`, focus/browse pills, read-mode toggle — unchanged.
2. On `/saved`: Topbar shows `saved`, no pills, no read-mode toggle.
3. Navigate `/` ↔ `/saved` via Rail: Topbar updates correctly each time, no flashing / layout jumps.

Kill dev.

- [ ] **Step 4: Commit**

```bash
git add apps/web/app/components/desktop/Topbar.vue
git commit -m "feat(web): route-aware Topbar (hide pills on /saved)"
```

---

### Task 3.3: `FocusMode` consumes `pendingPostId`

**Note on scope:** `FocusMode.vue` already calls `useTopicPosts` itself and destructures `const { state } = useTopicPosts(...)`. The existing `watch(() => state.posts.value.length, ...)` is your precedent — `state.posts` is a reactive ref in scope inside this component. No prop-drilling needed.

**Files:**
- Modify: `apps/web/app/components/desktop/FocusMode.vue`

- [ ] **Step 1: Add `pendingPostId` to the destructure and add the consumer effect**

In `FocusMode.vue`, change the `useFeedView` destructure:

```ts
const { activeTopicSlug, activePostIndex, activePanel, activeTopicPostCount, pendingPostId } =
  useFeedView()
```

Add this `watchEffect` after the existing `watch` that publishes `activeTopicPostCount` (right before the `cardKey` computed, or right before the `toastMessage` ref — wherever reads cleanly; order inside `<script setup>` does not affect behavior):

```ts
// Consume a pendingPostId handoff (e.g. from SavedPage). Runs on both fresh mount
// (slug change) and same-slug re-entry, since watchEffect re-evaluates whenever any
// reactive read changes — pendingPostId flipping from null → number is enough.
watchEffect(() => {
  const target = pendingPostId.value
  if (target == null) return
  const posts = state.posts.value
  if (!posts.length) return
  const idx = posts.findIndex((p) => +p.id === target)
  activePostIndex.value = idx >= 0 ? idx : 0
  pendingPostId.value = null
})
```

- [ ] **Step 2: Verify lint + fmt**

```bash
cd apps/web && pnpm lint && pnpm fmt:check
```

- [ ] **Step 3: Manual verification — full handoff flow**

```bash
cd apps/web && pnpm dev
```

1. Starting fresh: visit `/`, open a topic, scroll a few posts deep (e.g. index 3), bookmark post #3 via ActionsColumn.
2. Navigate to `/saved` via Rail. The card for post #3 is visible.
3. Click the card. Expected: land on `/`, FocusMode opens, `activePostIndex` is 3 (the post you bookmarked), not 0.
4. Repeat with multiple topics: bookmark post #5 in topic A, post #2 in topic B. From `/saved`, click topic-A-card → lands on topic A index 5. Click Rail `saved` → click topic-B-card → lands on topic B index 2. Both hops land correctly.
5. Same-slug regression check: on `/` with topic A active at index 5, click Rail `saved`, click topic-A-card (same topic already active in memory) → lands on topic A index that matches the bookmarked post, not the previously-focused index.
6. Stale-bookmark check: manually edit localStorage `dt:saved` in devtools to include a non-existent `postId` for a valid `topicSlug`. Click that card → lands on topic, index defaults to 0, no console errors, `pendingPostId` is null after.
7. Arrow keys still work after every handoff (verifies the keyboard handler in `DesktopShell` still sees a valid `activeTopicPostCount`).

Kill dev.

- [ ] **Step 4: Commit**

```bash
git add apps/web/app/components/desktop/FocusMode.vue
git commit -m "feat(web): consume pendingPostId in FocusMode for saved-card handoff"
```

---

### Task 3.4: Guard shell-level keyboard handler from firing on non-feed routes

Currently `DesktopShell.vue` attaches a global `keydown` listener that reads `useFeedView` state and mutates `activePostIndex` / `activeTopicSlug`. On `/saved` this handler is still attached (shell is mounted) and would cycle posts of whatever topic was last active in memory — invisible to the user but surprising. Guard it on route.

**Files:**
- Modify: `apps/web/app/components/desktop/DesktopShell.vue`

- [ ] **Step 1: Add a route guard at the top of `onKeydown`**

In `DesktopShell.vue`, at the top of the script (near other imports/composables) add:

```ts
const route = useRoute()
```

Then modify the `onKeydown` function — add this as the first check inside the body, before any other guard:

```ts
function onKeydown(e: KeyboardEvent) {
  if (route.path !== '/') return
  if (feedMode.value !== 'focus') return
  if (activePanel.value !== null) return
  // ... rest unchanged
}
```

- [ ] **Step 2: Verify lint + fmt**

```bash
cd apps/web && pnpm lint && pnpm fmt:check
```

- [ ] **Step 3: Manual verification**

```bash
cd apps/web && pnpm dev
```

1. On `/`: arrow keys cycle posts (unchanged).
2. Navigate to `/saved`: press arrow keys repeatedly → nothing happens, `activePostIndex` in devtools/Vue devtools stays fixed.
3. Navigate back to `/`: arrows work again.

Kill dev.

- [ ] **Step 4: Commit**

```bash
git add apps/web/app/components/desktop/DesktopShell.vue
git commit -m "fix(web): guard DesktopShell keyboard handler to feed route"
```

---

### Task 3.5: Final pass — lint, fmt, acceptance

**Files:** none (verification + final polish only)

- [ ] **Step 1: Full lint + fmt + build**

```bash
cd apps/web && pnpm lint && pnpm fmt:check && pnpm build
```

Expected: all green. If `pnpm build` fails, diagnose type errors now, not after merge.

- [ ] **Step 2: Run every acceptance case from the spec's Testing section**

Reference: `docs/superpowers/specs/2026-04-21-saved-page-design.md` §Testing. Run all 9 acceptance cases end-to-end in a single dev session:

```bash
cd apps/web && pnpm dev
```

1. Rail `saved` → `/saved` lands correctly.
2. Empty state: clear `dt:saved` in devtools → reload `/saved` → "тут порожньо" + link works.
3. Non-empty: bookmark several posts → grid renders newest-first with correct visuals.
4. Click a saved card → focus mode on the exact post.
5. Click saved card for currently-active topic → pendingPostId still wins.
6. Hover + click remove icon → card fades out, count decreases.
7. Refresh `/saved` → order and count preserved.
8. Arrow-key nav works after handoff.
9. Rail active state: `/` → feed lit, `/saved` → saved lit, `/topic/foo` → neither lit.

- [ ] **Step 3: If anything fails, fix in a dedicated follow-up commit (not amend)**

Preserve TDD-friendly history; each fix stands alone.

- [ ] **Step 4: Final commit (if any polish was needed) or no-op**

```bash
git status
```

If clean → nothing to commit; proceed to `superpowers:finishing-a-development-branch`.

If dirty:

```bash
git add -A
git commit -m "fix(web): saved page final acceptance polish"
```

---

## After the plan

When all three chunks are complete and green, invoke `superpowers:finishing-a-development-branch` to decide between merge / PR / keep / discard. (The same branch already holds the desktop MVP + the in-flight PR; this work either gets pushed onto it, or branched off and PR'd separately — decide at finishing time based on what's already open upstream.)
