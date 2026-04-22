# Search Page Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a desktop `/search` route that matches the `doctok_desktop_v6.html` mockup's search view: search input, horizontal-scroll catalog of mini-courses and topic categories when empty, filtered post-result list when non-empty. All data is mocked. No backend changes. Clicks on catalog cards, course cards, "всі курси →", "показати всі →", and result rows are no-ops.

**Architecture:** New Nuxt route `/search` rendering `<DesktopShell><SearchPage /></DesktopShell>` (same pattern as `/saved`). All state lives in `SearchPage` (one `query` ref). Mock data is a plain module (no Vue, no refs) at `app/lib/searchMockData.ts`. Filter is a pure `computed` — three `.includes` across title/topic-name/related. Every child component is presentational (props in, nothing out).

**Tech Stack:** Nuxt 4, Vue 3.5, `@nuxt/icon` (lucide set), scoped CSS per component, tokens from `apps/web/app/assets/css/tailwind.css`. No test framework in this workspace — the automated gate is `oxlint` + `oxfmt`; each task lists manual browser acceptance steps.

**Spec:** See [`docs/superpowers/specs/2026-04-22-search-page-design.md`](../specs/2026-04-22-search-page-design.md) — read it before starting.

**Working directory:** `apps/web/` (all commands assume this unless stated otherwise).

**Package manager:** `bun` (not npm/pnpm). All commands below use `bun run <script>`.

---

## File Structure

New files:

- `apps/web/app/lib/searchMockData.ts` — types + mock content + flattened `mockAllPosts` index. Plain TS, no Vue imports. Single source of content for the whole feature.
- `apps/web/app/pages/search.vue` — Nuxt route wrapper. Renders `<DesktopShell><SearchPage /></DesktopShell>` when on desktop; renders nothing on non-desktop (mobile out of scope, matches `/saved`).
- `apps/web/app/components/desktop/search/SearchPage.vue` — stateful root. Owns the `query` ref and the `results` computed. Decides catalog vs results.
- `apps/web/app/components/desktop/search/SearchInput.vue` — search icon + `<input v-model>`. No other logic.
- `apps/web/app/components/desktop/search/SearchCatalog.vue` — empty-query view. Imports mock data; renders one row per section (1 courses row + N category rows).
- `apps/web/app/components/desktop/search/SearchCatalogRow.vue` — one horizontal-scroll section. Takes `title` + `actionLabel` props + default slot for cards. The action button is a `<button>` with no handler.
- `apps/web/app/components/desktop/search/SearchTopicCard.vue` — single topic card. Props: `topic: MockTopic`. Click does nothing.
- `apps/web/app/components/desktop/search/SearchCourseCard.vue` — single mini-course card (amber theme + progress bar). Props: `course: MockCourse`. Click does nothing.
- `apps/web/app/components/desktop/search/SearchResults.vue` — non-empty-query view. Props: `query: string`, `results: MockSearchHit[]`. Renders label + list or empty state.
- `apps/web/app/components/desktop/search/SearchResultRow.vue` — single result row with inline 3-color kind map (summary/example/fact). Props: `hit: MockSearchHit`. Click does nothing.

Modified files:

- `apps/web/app/components/desktop/Rail.vue` — flip the `search` nav item from disabled to enabled; give it `path: '/search'` and user-facing title `'Каталог'`.
- `apps/web/app/components/desktop/Topbar.vue` — add a `'/search' → 'search'` case to the `title` computed. No other changes (mode-toggle / read-toggle already gated on `isFeed`).

Untouched files to be aware of:

- `apps/web/app/components/desktop/DesktopShell.vue` — already hides `Sidebar` when `route.path !== '/'`. No change needed.
- `apps/web/app/pages/saved.vue` — reference pattern to copy for `search.vue`.
- `apps/web/app/assets/css/tailwind.css` — source of available CSS tokens. **Do not edit.** Relevant tokens for this feature:
  - `--font-mono` — every text element uses this
  - `--kind-summary` `#3b82f6`, `--kind-summary-bg` `#0d1f35` — blue (summary kind)
  - `--kind-example` `#22c55e`, `--kind-example-bg` `#001f10` — green (example kind)
  - `--kind-fact` `#f97316`, `--kind-fact-bg` `#1e1400` — orange (fact kind)
  - `--dt-rail-active-bg` `#001f0d` — active rail button bg (used by Rail already)
  - `--dt-sidebar-border` `#111` — 1px borders between shell sections
  - `--dt-panel-bg` `#060606` — card / input background
  - `--dt-panel-border` `#141414` — card / input border
  - `--dt-text-tertiary` `#7a7f86` — secondary labels
  - `--dt-text-quaternary` `#5f666f` — tertiary labels / counts
  - `--dt-border-subtle` `#141616` — subtler borders

  Hex literals we will introduce (no matching token exists and adding one is out of scope for this feature):
  - `#1a1400` (course card border), `#0e0c00` (course card bg), `#ffb830` (course amber) — course theme
  - `#1a1a1a` (2px scrollbar thumb, muted placeholder), `#1e1e1e` / `#2a2a2a` (small greys) — accents inside cards

---

## Chunk 1: Foundation — mock data, route, rail wiring

Scope: groundwork. After this chunk, navigating to `/search` via the rail or direct URL lands on an empty shell with the correct topbar title. The mock data module exists and compiles, but is not rendered yet.

### Task 1.1: Create `searchMockData.ts`

Goal: Pure-data module that's the single source of truth for catalog + search content.

**Files:**
- Create: `apps/web/app/lib/searchMockData.ts`

- [ ] **Step 1: Create the file with types and content**

Content is trimmed from `apps/web/design/doctok_desktop_v6.html` (the `categories` and `miniCourses` arrays in the inline `<script>` block). Only fields rendered by the search page are kept — no `content`, `code`, `lang`, `quiz`.

Paste this exact content:

```ts
// Mock content for the /search page. Pure data — no Vue imports, no refs.
// Trimmed from apps/web/design/doctok_desktop_v6.html to only fields the
// search page renders (kind/title/related for posts; icon/name/category
// /source/posts.length for topics; name/icon/count/progress/category for
// mini-courses). The real feed uses its own data path.

export type MockPostKind = 'summary' | 'example' | 'fact'

export interface MockPost {
  title: string
  kind: MockPostKind
  related: string[]
}

export interface MockTopic {
  name: string
  icon: string
  category: string
  source: string // 'MDN' | 'web.dev' | 'javascript.info' | …
  posts: MockPost[]
}

export interface MockCategory {
  name: string
  icon: string
  topics: MockTopic[]
}

export interface MockCourse {
  name: string
  icon: string
  count: number
  progress: number // 0–100
  category: string
}

export interface MockSearchHit {
  topic: MockTopic
  post: MockPost
}

export const mockCategories: MockCategory[] = [
  {
    name: 'CSS',
    icon: '◈',
    topics: [
      {
        name: 'CSS Grid',
        icon: '▦',
        category: 'CSS',
        source: 'MDN',
        posts: [
          { kind: 'summary', title: 'CSS Grid', related: ['grid-template', 'Flexbox', '1fr', 'subgrid'] },
          { kind: 'example', title: 'Три рівні колонки', related: ['repeat()', 'minmax()', 'auto-fill', 'auto-fit'] },
          { kind: 'fact', title: 'IE вперше реалізував Grid', related: ['CSS history', 'IE prefixes', 'W3C'] },
        ],
      },
      {
        name: 'Flexbox',
        icon: '⇔',
        category: 'CSS',
        source: 'MDN',
        posts: [
          { kind: 'summary', title: 'CSS Flexbox', related: ['justify-content', 'align-items', 'flex-grow', 'CSS Grid'] },
          { kind: 'example', title: 'Класичне центрування', related: ['flex-direction', 'flex-wrap', 'gap'] },
          { kind: 'fact', title: 'Три різних синтаксиси', related: ['Can I Use', 'CSS history', '-webkit-'] },
        ],
      },
    ],
  },
  {
    name: 'JavaScript',
    icon: '◇',
    topics: [
      {
        name: 'Promises',
        icon: '⇢',
        category: 'JavaScript',
        source: 'MDN',
        posts: [
          { kind: 'summary', title: 'JavaScript Promises', related: ['async/await', 'мікрозадачі', 'Event Loop', 'fetch()'] },
          { kind: 'example', title: 'Ланцюжок .then()', related: ['.catch()', '.finally()', 'Promise.all', 'async/await'] },
          { kind: 'fact', title: 'Promises/A+ з 2012', related: ['ES2015', 'Bluebird', 'TC39'] },
        ],
      },
    ],
  },
  {
    name: 'Web APIs',
    icon: '◉',
    topics: [
      {
        name: 'Web APIs',
        icon: '◈',
        category: 'Web APIs',
        source: 'web.dev',
        posts: [
          { kind: 'summary', title: 'Web APIs', related: ['Navigator', 'Window', 'DOM', 'WHATWG'] },
          { kind: 'example', title: 'Intersection Observer', related: ['lazy loading', 'MutationObserver', 'ResizeObserver'] },
          { kind: 'fact', title: '5000+ інтерфейсів', related: ['MDN', 'Baseline', 'deprecated'] },
        ],
      },
    ],
  },
]

export const mockMiniCourses: MockCourse[] = [
  { name: 'Promises без болю', icon: '⇢', count: 8, progress: 62, category: 'JavaScript' },
  { name: 'CSS Grid за 10 карток', icon: '▦', count: 10, progress: 30, category: 'CSS' },
  { name: 'Web APIs: старт', icon: '◈', count: 6, progress: 0, category: 'Web APIs' },
  { name: 'Async/Await глибоко', icon: '↻', count: 7, progress: 0, category: 'JavaScript' },
]

export const mockAllPosts: MockSearchHit[] = mockCategories.flatMap((cat) =>
  cat.topics.flatMap((topic) => topic.posts.map((post) => ({ topic, post })))
)
```

- [ ] **Step 2: Lint and format**

Run: `bun run lint apps/web/app/lib/searchMockData.ts`
Expected: no errors.
Run: `bun run fmt apps/web/app/lib/searchMockData.ts`
Expected: file is reformatted in place (silent on success).

- [ ] **Step 3: Commit**

```bash
git add apps/web/app/lib/searchMockData.ts
git commit -m "feat(web): add mock data for search page"
```

---

### Task 1.2: Create the `/search` route wrapper

**Files:**
- Create: `apps/web/app/pages/search.vue`

- [ ] **Step 1: Create the file**

Mirrors `apps/web/app/pages/saved.vue` one-for-one. Paste:

```vue
<script setup lang="ts">
import DesktopShell from '~/components/desktop/DesktopShell.vue'
import SearchPage from '~/components/desktop/search/SearchPage.vue'
import { useDesktop } from '~/composables/useDesktop'

const isDesktop = useDesktop()
</script>

<template>
  <DesktopShell v-if="isDesktop">
    <SearchPage />
  </DesktopShell>
</template>
```

Note: `SearchPage` does not exist yet — Task 1.3 creates it. This will fail the TypeScript/Vue import check until then, which is expected.

- [ ] **Step 2: Commit (deferred)**

Do not commit yet. Combine this commit with Task 1.3 so the route actually resolves.

---

### Task 1.3: Create the `SearchPage` shell component

Goal: Empty scaffolded component that satisfies Task 1.2's import. Filter logic and catalog/results rendering are added in later chunks.

**Files:**
- Create: `apps/web/app/components/desktop/search/SearchPage.vue`

- [ ] **Step 1: Create the file**

Paste:

```vue
<script setup lang="ts">
const query = ref('')
</script>

<template>
  <section class="search">
    <!-- SearchInput mounts here in Chunk 2 -->
    <!-- SearchCatalog / SearchResults mount here in Chunks 2 and 3 -->
    <pre class="placeholder">search page — query: "{{ query }}"</pre>
  </section>
</template>

<style scoped>
.search {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 20px 26px;
  overflow: hidden;
  font-family: var(--font-mono);
}
.placeholder {
  font-size: 11px;
  color: var(--dt-text-quaternary);
}
</style>
```

The `<pre>` placeholder is removed in Chunk 2, Task 2.6. It exists so the scaffolding is visibly correct during verification.

- [ ] **Step 2: Lint and format**

Run: `bun run lint apps/web/app/components/desktop/search/SearchPage.vue apps/web/app/pages/search.vue`
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add apps/web/app/pages/search.vue apps/web/app/components/desktop/search/SearchPage.vue
git commit -m "feat(web): scaffold /search route and SearchPage shell"
```

---

### Task 1.4: Enable search in the Rail and Topbar

**Files:**
- Modify: `apps/web/app/components/desktop/Rail.vue`
- Modify: `apps/web/app/components/desktop/Topbar.vue`

- [ ] **Step 1: Flip the search rail item from disabled to enabled**

Open `apps/web/app/components/desktop/Rail.vue`. Replace the existing search entry in `navItems`:

```ts
{ key: 'search', icon: 'lucide:search', title: 'Каталог (скоро)', disabled: true },
```

with:

```ts
{ key: 'search', icon: 'lucide:search', title: 'Каталог', path: '/search', disabled: false },
```

- [ ] **Step 2: Add `'/search' → 'search'` to the Topbar title computed**

Open `apps/web/app/components/desktop/Topbar.vue`. Replace the `title` computed:

```ts
const title = computed(() => {
  if (route.path === '/saved') return 'saved'
  return 'feed'
})
```

with:

```ts
const title = computed(() => {
  if (route.path === '/saved') return 'saved'
  if (route.path === '/search') return 'search'
  return 'feed'
})
```

- [ ] **Step 3: Lint and format**

Run: `bun run lint apps/web/app/components/desktop/Rail.vue apps/web/app/components/desktop/Topbar.vue`
Expected: no errors.

- [ ] **Step 4: Manual verification**

Run: `bun run dev` (leave running)

Open the app in a browser:
- Click the second rail icon (magnifying glass). It becomes active (green). URL becomes `/search`.
- The topbar shows `search` (lowercase) as the page title.
- The sidebar ("Recent & Pinned") is hidden.
- The mode toggle and read-mode toggle are hidden (no `focus / browse` or `simplified / standard / …` buttons).
- The main area shows the `<pre>` placeholder text `search page — query: ""`.
- Click the first rail icon (grid) — returns to `/`, feed renders normally. No regressions.

Stop the dev server after verification: `Ctrl+C`.

- [ ] **Step 5: Commit**

```bash
git add apps/web/app/components/desktop/Rail.vue apps/web/app/components/desktop/Topbar.vue
git commit -m "feat(web): enable /search route in rail and topbar"
```

---

## Chunk 2: Catalog view (empty-query state)

Scope: Render the input + the catalog exactly like the mockup. After this chunk, `/search` shows the search input and — because the query is empty — horizontal-scroll rows of mini-course cards and topic cards per category. Clicks still do nothing (results list is added in Chunk 3).

### Task 2.1: `SearchInput` component

**Files:**
- Create: `apps/web/app/components/desktop/search/SearchInput.vue`

- [ ] **Step 1: Create the file**

Paste:

```vue
<script setup lang="ts">
defineProps<{ modelValue: string }>()
defineEmits<{ 'update:modelValue': [value: string] }>()
</script>

<template>
  <div class="wrap">
    <Icon
      name="lucide:search"
      class="icon"
    />
    <input
      class="input"
      type="text"
      placeholder="Пошук тем, методів, API..."
      :value="modelValue"
      @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
    />
  </div>
</template>

<style scoped>
.wrap {
  display: flex;
  align-items: center;
  gap: 10px;
  background: var(--dt-panel-bg);
  border: 1px solid var(--dt-border-subtle);
  border-radius: 6px;
  padding: 0 16px;
  flex-shrink: 0;
}
.icon {
  width: 13px;
  height: 13px;
  color: #333;
  flex-shrink: 0;
}
.input {
  flex: 1;
  background: none;
  border: none;
  outline: none;
  font-family: var(--font-mono);
  font-size: 13px;
  color: #c8c8c0;
  padding: 12px 0;
  caret-color: var(--kind-example);
}
.input::placeholder {
  color: #1a1a1a;
}
</style>
```

- [ ] **Step 2: Lint**

Run: `bun run lint apps/web/app/components/desktop/search/SearchInput.vue`
Expected: no errors.

---

### Task 2.2: `SearchTopicCard` component

**Files:**
- Create: `apps/web/app/components/desktop/search/SearchTopicCard.vue`

- [ ] **Step 1: Create the file**

Paste:

```vue
<script setup lang="ts">
import type { MockTopic } from '~/lib/searchMockData'

defineProps<{ topic: MockTopic }>()
</script>

<template>
  <div class="card">
    <div class="icon">{{ topic.icon }}</div>
    <div class="name">{{ topic.name }}</div>
    <div class="meta">{{ topic.posts.length }} posts · {{ topic.source }}</div>
  </div>
</template>

<style scoped>
.card {
  flex-shrink: 0;
  width: 120px;
  padding: 11px;
  border: 1px solid #0e0e0e;
  border-radius: 6px;
  background: var(--dt-panel-bg);
  cursor: pointer;
  transition: border-color 0.15s, background 0.15s;
  display: flex;
  flex-direction: column;
  gap: 5px;
  font-family: var(--font-mono);
}
.card:hover {
  border-color: var(--dt-border-subtle);
  background: #0a0a0a;
}
.icon {
  font-size: 15px;
  color: #2a2a2a;
}
.name {
  font-size: 10px;
  color: #4a4a4a;
  line-height: 1.3;
}
.meta {
  font-size: 8px;
  color: #1a1a1a;
}
</style>
```

- [ ] **Step 2: Lint**

Run: `bun run lint apps/web/app/components/desktop/search/SearchTopicCard.vue`
Expected: no errors.

---

### Task 2.3: `SearchCourseCard` component

**Files:**
- Create: `apps/web/app/components/desktop/search/SearchCourseCard.vue`

- [ ] **Step 1: Create the file**

Paste:

```vue
<script setup lang="ts">
import type { MockCourse } from '~/lib/searchMockData'

defineProps<{ course: MockCourse }>()
</script>

<template>
  <div class="card">
    <div class="icon">{{ course.icon }}</div>
    <div class="name">{{ course.name }}</div>
    <div class="foot">
      <span class="count">{{ course.count }} карток</span>
      <div class="bar-bg">
        <div
          class="bar-fill"
          :style="{ width: `${course.progress}%` }"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.card {
  flex-shrink: 0;
  width: 150px;
  padding: 11px;
  border: 1px solid #1a1400;
  border-radius: 6px;
  background: #0e0c00;
  cursor: pointer;
  transition: border-color 0.15s, background 0.15s;
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-family: var(--font-mono);
}
.card:hover {
  border-color: #2a2200;
  background: #141000;
}
.icon {
  font-size: 14px;
  color: #2e2200;
}
.name {
  font-size: 10px;
  color: #8a7a40;
  line-height: 1.3;
}
.foot {
  display: flex;
  align-items: center;
  gap: 6px;
}
.count {
  font-size: 8px;
  color: #3a3000;
}
.bar-bg {
  flex: 1;
  height: 2px;
  background: #1e1800;
  border-radius: 1px;
  overflow: hidden;
}
.bar-fill {
  height: 100%;
  border-radius: 1px;
  background: #ffb830;
}
</style>
```

- [ ] **Step 2: Lint**

Run: `bun run lint apps/web/app/components/desktop/search/SearchCourseCard.vue`
Expected: no errors.

---

### Task 2.4: `SearchCatalogRow` component

**Files:**
- Create: `apps/web/app/components/desktop/search/SearchCatalogRow.vue`

- [ ] **Step 1: Create the file**

Paste:

```vue
<script setup lang="ts">
defineProps<{
  title: string
  actionLabel?: string
}>()
</script>

<template>
  <section class="row-wrap">
    <header class="header">
      <span class="title">{{ title }}</span>
      <button
        v-if="actionLabel"
        type="button"
        class="action"
      >
        {{ actionLabel }}
      </button>
    </header>
    <div class="row">
      <slot />
    </div>
  </section>
</template>

<style scoped>
.row-wrap {
  display: flex;
  flex-direction: column;
}
.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 9px;
}
.title {
  font-family: var(--font-mono);
  font-size: 8px;
  color: #2a2a2a;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}
.action {
  font-family: var(--font-mono);
  font-size: 8px;
  color: #1a1a1a;
  background: none;
  border: none;
  cursor: pointer;
  transition: color 0.15s;
}
.action:hover {
  color: #444;
}
.row {
  display: flex;
  gap: 8px;
  overflow-x: auto;
  padding-bottom: 4px;
}
.row::-webkit-scrollbar {
  height: 2px;
}
.row::-webkit-scrollbar-thumb {
  background: #1a1a1a;
}
</style>
```

The action button has no `@click` handler — matches the "dead clicks" constraint from the spec.

- [ ] **Step 2: Lint**

Run: `bun run lint apps/web/app/components/desktop/search/SearchCatalogRow.vue`
Expected: no errors.

---

### Task 2.5: `SearchCatalog` component

**Files:**
- Create: `apps/web/app/components/desktop/search/SearchCatalog.vue`

- [ ] **Step 1: Create the file**

Paste:

```vue
<script setup lang="ts">
import SearchCatalogRow from './SearchCatalogRow.vue'
import SearchCourseCard from './SearchCourseCard.vue'
import SearchTopicCard from './SearchTopicCard.vue'
import { mockCategories, mockMiniCourses } from '~/lib/searchMockData'
</script>

<template>
  <div class="catalog">
    <SearchCatalogRow
      title="★ міні-курси"
      action-label="всі курси →"
    >
      <SearchCourseCard
        v-for="course in mockMiniCourses"
        :key="course.name"
        :course="course"
      />
    </SearchCatalogRow>

    <SearchCatalogRow
      v-for="cat in mockCategories"
      :key="cat.name"
      :title="`${cat.icon} ${cat.name}`"
      action-label="показати всі →"
    >
      <SearchTopicCard
        v-for="topic in cat.topics"
        :key="topic.name"
        :topic="topic"
      />
    </SearchCatalogRow>
  </div>
</template>

<style scoped>
.catalog {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 18px;
}
.catalog::-webkit-scrollbar {
  width: 2px;
}
</style>
```

- [ ] **Step 2: Lint**

Run: `bun run lint apps/web/app/components/desktop/search/SearchCatalog.vue`
Expected: no errors.

---

### Task 2.6: Wire `SearchInput` + `SearchCatalog` into `SearchPage`; verify

**Files:**
- Modify: `apps/web/app/components/desktop/search/SearchPage.vue`

- [ ] **Step 1: Replace the placeholder shell with real structure**

Open `apps/web/app/components/desktop/search/SearchPage.vue`. Replace the whole file with:

```vue
<script setup lang="ts">
import SearchCatalog from './SearchCatalog.vue'
import SearchInput from './SearchInput.vue'

const query = ref('')
const trimmed = computed(() => query.value.trim().toLowerCase())
const isSearching = computed(() => trimmed.value.length > 0)
</script>

<template>
  <section class="search">
    <SearchInput v-model="query" />
    <SearchCatalog v-if="!isSearching" />
    <!-- SearchResults mounts here in Chunk 3 -->
  </section>
</template>

<style scoped>
.search {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 20px 26px;
  overflow: hidden;
  font-family: var(--font-mono);
}
</style>
```

`trimmed` and `isSearching` are declared now even though only `isSearching` is consumed by the template in this chunk — they will both be consumed by `SearchResults` wiring in Chunk 3, and declaring them now avoids a churn-diff next chunk.

- [ ] **Step 2: Lint and format**

Run: `bun run lint apps/web/app/components/desktop/search/`
Expected: no errors.
Run: `bun run fmt apps/web/app/components/desktop/search/`
Expected: silent.

- [ ] **Step 3: Manual verification**

Run: `bun run dev` (leave running)

On `/search`:
- Search input is visible at the top with the lucide magnifying-glass icon and placeholder `Пошук тем, методів, API...`
- Below it: a mini-courses row with 4 amber cards (progress bars: 62%, 30%, 0%, 0%), with an `всі курси →` button on the right of the section header.
- Below that: a CSS row with 2 topic cards (CSS Grid, Flexbox), a JavaScript row with 1 card (Promises), a Web APIs row with 1 card (Web APIs). Each row has `показати всі →` on the right.
- If the browser window is narrow enough, rows scroll horizontally (hover over a row, scroll sideways with trackpad / shift+wheel).
- Clicking any topic card, course card, `всі курси →`, or `показати всі →` does **nothing** — no navigation, no console error. Cards only have a subtle hover border/background change.
- Typing any text in the input → the catalog disappears (empty white/black space below the input). Clearing the input → catalog returns. This is intentional; results list lands in Chunk 3.

Stop the dev server: `Ctrl+C`.

- [ ] **Step 4: Commit**

```bash
git add apps/web/app/components/desktop/search/
git commit -m "feat(web): render search catalog with mock courses and topics"
```

---

## Chunk 3: Results view + final verification

Scope: Implement the filter + results UI. After this chunk, the feature is complete.

### Task 3.1: `SearchResultRow` component

**Files:**
- Create: `apps/web/app/components/desktop/search/SearchResultRow.vue`

- [ ] **Step 1: Create the file with inline kind-color map**

The existing `usePostKind` / `PostKindBadge` don't know about the `'fact'` kind (they handle `summary / example / concept / tip`). For the search page we duplicate the small 3-color map inline — the spec's "pure-mock island" decision.

Paste:

```vue
<script setup lang="ts">
import type { MockPostKind, MockSearchHit } from '~/lib/searchMockData'

defineProps<{ hit: MockSearchHit }>()

const kindStyle: Record<MockPostKind, { bg: string; fg: string; border: string }> = {
  summary: { bg: '#0d1f35', fg: '#6ab4ff', border: '#2a5f9f' },
  example: { bg: '#001f10', fg: '#00e87a', border: '#006b38' },
  fact: { bg: '#1e1400', fg: '#ffb830', border: '#7a5200' },
}
</script>

<template>
  <div class="row">
    <span
      class="kind"
      :style="{
        background: kindStyle[hit.post.kind].bg,
        color: kindStyle[hit.post.kind].fg,
        borderColor: `${kindStyle[hit.post.kind].border}33`,
      }"
    >
      {{ hit.post.kind }}
    </span>
    <span class="title">{{ hit.post.title }}</span>
    <span class="topic">{{ hit.topic.name }}</span>
  </div>
</template>

<style scoped>
.row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 9px 12px;
  border: 1px solid #0e0e0e;
  border-radius: 4px;
  cursor: pointer;
  transition: border-color 0.12s, background 0.12s;
  background: var(--dt-panel-bg);
  font-family: var(--font-mono);
}
.row:hover {
  background: #0c0c0c;
  border-color: var(--dt-border-subtle);
}
.kind {
  font-size: 7px;
  padding: 2px 6px;
  border-radius: 2px;
  letter-spacing: 0.1em;
  flex-shrink: 0;
  border: 1px solid transparent;
  text-transform: lowercase;
}
.title {
  font-size: 11px;
  color: #444;
  flex: 1;
}
.topic {
  font-size: 8px;
  color: #1a1a1a;
}
</style>
```

- [ ] **Step 2: Lint**

Run: `bun run lint apps/web/app/components/desktop/search/SearchResultRow.vue`
Expected: no errors.

---

### Task 3.2: `SearchResults` component

**Files:**
- Create: `apps/web/app/components/desktop/search/SearchResults.vue`

- [ ] **Step 1: Create the file**

Paste:

```vue
<script setup lang="ts">
import SearchResultRow from './SearchResultRow.vue'
import type { MockSearchHit } from '~/lib/searchMockData'

defineProps<{
  query: string
  results: MockSearchHit[]
}>()
</script>

<template>
  <section class="results">
    <div class="label">результати для "{{ query }}"</div>
    <div
      v-if="!results.length"
      class="empty"
    >
      // нічого не знайдено
    </div>
    <div
      v-else
      class="list"
    >
      <SearchResultRow
        v-for="hit in results"
        :key="`${hit.topic.name}::${hit.post.title}`"
        :hit="hit"
      />
    </div>
  </section>
</template>

<style scoped>
.results {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-height: 0;
  font-family: var(--font-mono);
}
.label {
  font-size: 8px;
  color: #1e1e1e;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  margin-bottom: 8px;
  flex-shrink: 0;
}
.list {
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 5px;
}
.list::-webkit-scrollbar {
  width: 2px;
}
.list::-webkit-scrollbar-thumb {
  background: #1a1a1a;
}
.empty {
  font-size: 11px;
  color: #1a1a1a;
  padding: 20px 0;
}
</style>
```

Note: `topic.name::post.title` is the `:key`. The mock dataset has no title collisions within a single topic, so this is unique.

- [ ] **Step 2: Lint**

Run: `bun run lint apps/web/app/components/desktop/search/SearchResults.vue`
Expected: no errors.

---

### Task 3.3: Wire results into `SearchPage`

**Files:**
- Modify: `apps/web/app/components/desktop/search/SearchPage.vue`

- [ ] **Step 1: Add results computed and mount `SearchResults`**

Replace the whole file with:

```vue
<script setup lang="ts">
import SearchCatalog from './SearchCatalog.vue'
import SearchInput from './SearchInput.vue'
import SearchResults from './SearchResults.vue'
import { mockAllPosts, type MockSearchHit } from '~/lib/searchMockData'

const query = ref('')
const trimmed = computed(() => query.value.trim().toLowerCase())
const isSearching = computed(() => trimmed.value.length > 0)

const results = computed<MockSearchHit[]>(() => {
  if (!isSearching.value) return []
  const q = trimmed.value
  return mockAllPosts
    .filter(
      ({ topic, post }) =>
        post.title.toLowerCase().includes(q) ||
        topic.name.toLowerCase().includes(q) ||
        post.related.some((r) => r.toLowerCase().includes(q))
    )
    .slice(0, 20)
})
</script>

<template>
  <section class="search">
    <SearchInput v-model="query" />
    <SearchCatalog v-if="!isSearching" />
    <SearchResults
      v-else
      :query="trimmed"
      :results="results"
    />
  </section>
</template>

<style scoped>
.search {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 20px 26px;
  overflow: hidden;
  font-family: var(--font-mono);
}
</style>
```

- [ ] **Step 2: Lint and format**

Run: `bun run lint apps/web/app/components/desktop/search/`
Expected: no errors.
Run: `bun run fmt apps/web/app/components/desktop/search/`
Expected: silent.

---

### Task 3.4: Full manual verification + final commit

**Files:** none — verification only.

- [ ] **Step 1: Run dev server**

Run: `bun run dev` (leave running)

- [ ] **Step 2: Walk through the acceptance checklist**

On `/search`:

1. **Navigation** — direct URL `/search` works. Rail button (magnifying glass) navigates to `/search`; becomes active (green). Clicking the grid rail button returns to `/`; feed renders with no regressions.

2. **Topbar** — shows `search` title. No mode toggle, no read-mode toggle.

3. **Sidebar** — hidden.

4. **Empty input** — catalog visible: 1 courses row (4 amber cards) + 3 category rows (CSS / JavaScript / Web APIs). All rows scroll horizontally on narrow viewports.

5. **Query `grid`** (case-insensitive) — catalog hidden; label reads `результати для "grid"`; list shows:
   - CSS Grid (summary), Три рівні колонки (example), IE вперше реалізував Grid (fact) — from the CSS Grid topic
   - CSS Flexbox (summary) — because its `related` contains `CSS Grid`
   - Any other posts whose title/topic/related contains `grid` (e.g. the mini-course name row does not appear — only post-level results).
   Verify each row renders a kind badge (blue / green / orange) + title + topic name on the right.

6. **Query `GRID`** (uppercase) — same results as step 5 (case-insensitive).

7. **Query `promise`** — results include JavaScript Promises posts (summary/example/fact) plus anything with `promise` in related tags.

8. **Query `observer`** — result: Intersection Observer (example).

9. **Query `zzzzz`** — empty state: `// нічого не знайдено` in muted grey.

10. **Clear input** — catalog returns immediately.

11. **Dead clicks** — click a course card, topic card, result row, `всі курси →`, `показати всі →`. Nothing happens. No errors in browser devtools console.

12. **Non-desktop** — resize the browser window below the mobile breakpoint (or set the viewport to ~400px wide). `/search` renders blank (no shell) — matches `/saved`.

13. **Keyboard** — pressing arrow keys while focused on the search input types in it. Pressing arrow keys while focused outside the input does nothing on `/search` (the global keyboard nav in `DesktopShell.vue` is gated on `route.path === '/'`).

- [ ] **Step 3: Lint and format the whole touched surface**

Run: `bun run lint`
Expected: no errors.
Run: `bun run fmt:check`
Expected: no files reported as needing formatting. If any do, run `bun run fmt` and re-verify.

- [ ] **Step 4: Stop dev server**

`Ctrl+C` in the dev terminal.

- [ ] **Step 5: Commit**

```bash
git add apps/web/app/components/desktop/search/
git commit -m "feat(web): implement search filter and results list"
```

- [ ] **Step 6: Review the full diff**

Run: `git log --oneline -- apps/web/ docs/superpowers/`
Expected: five new commits on this branch (mock data, scaffold, rail/topbar, catalog, results) plus the pre-existing spec commit.

Run: `git diff main -- apps/web/`
Skim: only new files under `apps/web/app/lib/searchMockData.ts`, `apps/web/app/pages/search.vue`, `apps/web/app/components/desktop/search/*.vue`, and small edits to `Rail.vue` + `Topbar.vue`. No changes to `DesktopShell.vue`, `FeedPage.vue`, `SavedPage.vue`, composables, or `tailwind.css`.

---

## Done

The search page is a pure-mock island. No API calls. No cross-page handoffs. Clicks on cards and action buttons are no-ops. The only interactive surface is the search input. Everything else is visual.

Out-of-scope follow-ups (explicitly deferred, do not add in this plan):
- Recent searches / search history
- Keyboard `/` shortcut to focus input
- URL query sync (`?q=…`)
- Real catalog from the feed API
- `/courses` page and mini-course data model
- Mobile layout
