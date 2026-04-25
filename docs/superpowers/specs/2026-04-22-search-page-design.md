# Search page — design spec

**Date:** 2026-04-22
**Scope:** Desktop search page (`/search`) built from the `doctok_desktop_v6.html` mockup, with fully mocked data and no backend changes.

## Goal

Add a search page that visually matches the mockup's search view: a search input, a catalog of mini-courses and topic categories shown when the input is empty, and a filtered list of post-level results when the input has content.

The page is a **pure-mock island** — it does not read from the real feed API, does not write to any store, and does not route anywhere else. Clicks on catalog cards, course cards, "all courses →", and search result rows are no-ops.

## Non-goals

- No backend search endpoint. No changes to `sdk.gen.ts` or `@tanstack/vue-query.gen`.
- No `/courses` page and no mini-courses data model in the real app. The rail button for courses stays disabled; only the search rail button is enabled.
- No mobile layout. Renders only inside `DesktopShell v-if="isDesktop"`, matching `/saved`.
- No URL query sync (`?q=…`), no debounce, no Enter-to-search.
- No interaction between the search page and the existing feed — clicking a topic card does not navigate to the feed.

## User-visible behavior

1. Rail has a "Каталог" (search) button that navigates to `/search`. Previously disabled — now enabled and routed.
2. On `/search`:
   - Topbar shows page title `search`. Mode toggle and read-mode toggle are hidden (already conditional on `isFeed`).
   - Sidebar is hidden (already conditional on `route.path === '/'`).
   - Body contains the search input and either the catalog or the results list.
3. Empty input → **catalog**:
   - A horizontal-scroll row of mini-course cards labeled `★ міні-курси`, with an `всі курси →` button. Both the cards and the button do nothing when clicked.
   - One horizontal-scroll row per category (CSS, JavaScript, Web APIs), each with a `показати всі →` button. Topic cards and the button do nothing when clicked.
4. Non-empty input → **results**:
   - A label `результати для "<query>"`.
   - Up to 20 result rows, each showing kind badge + post title + topic name. Clicks do nothing.
   - If no matches, an empty-state line `// нічого не знайдено`.
5. Clearing the input returns to the catalog.
6. The filter is case-insensitive and matches against `post.title`, `topic.name`, and `post.related[]`.

## File layout

```
app/
  pages/
    search.vue                                   # new route
  components/desktop/search/                     # new dir
    SearchPage.vue                               # container, owns query state
    SearchInput.vue                              # input + search icon
    SearchCatalog.vue                            # empty-query view
    SearchCatalogRow.vue                         # one horizontal-scroll section
    SearchTopicCard.vue                          # topic card (CSS Grid etc.)
    SearchCourseCard.vue                         # mini-course card
    SearchResults.vue                            # non-empty-query view
    SearchResultRow.vue                          # one post result row
  lib/
    searchMockData.ts                            # new — mock data + types + flattened index
```

Edits:
- `app/components/desktop/Rail.vue` — enable the search item and point it at `/search`.
- `app/components/desktop/Topbar.vue` — add `'/search' → 'search'` case to the `title` computed.

## Mock data (`app/lib/searchMockData.ts`)

Plain module exports — no Vue, no refs. Types inline.

```ts
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
  source: string // 'MDN' | 'web.dev' | …
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

export const mockCategories: MockCategory[] = /* CSS, JavaScript, Web APIs – trimmed from mockup */
export const mockMiniCourses: MockCourse[] = /* four courses from mockup */
export const mockAllPosts: MockSearchHit[] = /* mockCategories → topics → posts, flattened */
```

Content is trimmed to only fields the search page renders. The mockup's `content.{simplified,standard,detailed,original}`, `code`, `lang`, and `quiz` are **not** included — the feed uses its own data path and search doesn't render post bodies.

## Component responsibilities

### `SearchPage.vue` (stateful root)

Holds all state. Computes results. Chooses catalog vs results.

```ts
const query = ref('')
const trimmed = computed(() => query.value.trim().toLowerCase())
const isSearching = computed(() => trimmed.value.length > 0)

const results = computed<MockSearchHit[]>(() => {
  if (!isSearching.value) return []
  const q = trimmed.value
  return mockAllPosts
    .filter(({ topic, post }) =>
      post.title.toLowerCase().includes(q) ||
      topic.name.toLowerCase().includes(q) ||
      post.related.some((r) => r.toLowerCase().includes(q))
    )
    .slice(0, 20)
})
```

Template:
```vue
<section class="search">
  <SearchInput v-model="query" />
  <SearchCatalog v-if="!isSearching" />
  <SearchResults v-else :query="trimmed" :results="results" />
</section>
```

### `SearchInput.vue`
- `v-model` binding for query.
- Search icon (lucide) + `<input>`.
- Placeholder matches mockup: `Пошук тем, методів, API...`.

### `SearchCatalog.vue`
- Imports `mockMiniCourses`, `mockCategories`.
- Renders one `SearchCatalogRow` per section (1 courses row + N category rows).
- Passes section metadata (title, action label) and a slot for the cards.

### `SearchCatalogRow.vue`
- Props: `title: string`, `actionLabel?: string`.
- Emits nothing; the action button has no handler (no-op).
- Slot hosts the cards; CSS handles horizontal scroll.

### `SearchTopicCard.vue`
- Props: `topic: MockTopic`.
- Shows icon, name, `${posts.length} posts · ${source}`.

### `SearchCourseCard.vue`
- Props: `course: MockCourse`.
- Shows icon, name, `${count} карток`, progress bar (amber).

### `SearchResults.vue`
- Props: `query: string`, `results: MockSearchHit[]`.
- Renders `результати для "<query>"` label.
- If empty → `// нічого не знайдено`.
- Else → list of `SearchResultRow`.

### `SearchResultRow.vue`
- Props: `hit: MockSearchHit`.
- Shows kind badge + post title + topic name.

All cards are `<div>` or `<article>` with `cursor: pointer` styling but no click handler — matching the "dead clicks" decision.

## Data flow

```
SearchPage (query ref, results computed)
 ├─ SearchInput (v-model query)
 ├─ SearchCatalog (static mock imports)
 │    └─ SearchCatalogRow × N
 │         └─ SearchCourseCard | SearchTopicCard
 └─ SearchResults (query, results props)
      └─ SearchResultRow
```

Props flow down; nothing flows up except the `v-model` on the input.

## Styling

Scoped CSS per component, reusing existing CSS custom properties (`--font-mono`, `--kind-example`, `--kind-summary`, `--dt-text-*`, `--dt-sidebar-border`, `--dt-rail-active-bg`, etc.).

Before writing each component's styles, check `app/assets/` for already-defined tokens. Introduce hex literals only where no token exists (e.g. the amber palette for course cards: `#1a1400`, `#0e0c00`, `#ffb830` — reuse if a token exists).

Layout dimensions taken from the mockup:
- Outer padding `20px 26px`, gap `16px`.
- Search input wrap: `padding: 0 16px`, `border-radius: 6px`, input font-size `13px`.
- Catalog: `gap: 18px` between sections.
- Rows: `overflow-x: auto`, 2px scrollbar.
- Topic card: `width: 120px`, `padding: 11px`.
- Course card: `width: 150px`, amber theme.
- Result row: `padding: 9px 12px`, horizontal layout.

No animation on this page (mockup has none).

### Kind badge colors
`usePostKind` composable exists but is coupled to real `PostKind` types. If its output shape doesn't drop in cleanly for mock data, duplicate the three-color map inline in `SearchResultRow.vue` — it's small and self-contained:

```ts
const kindStyle = {
  summary: { bg: '#0d1f35', fg: '#6ab4ff', border: '#2a5f9f' },
  example: { bg: '#001f10', fg: '#00e87a', border: '#006b38' },
  fact:    { bg: '#1e1400', fg: '#ffb830', border: '#7a5200' },
}
```

## Rail + Topbar edits

`Rail.vue`: flip the search entry from disabled to enabled.
```ts
{ key: 'search', icon: 'lucide:search', title: 'Каталог', path: '/search', disabled: false },
```

`Topbar.vue`: extend the `title` computed.
```ts
const title = computed(() => {
  if (route.path === '/saved') return 'saved'
  if (route.path === '/search') return 'search'
  return 'feed'
})
```

`DesktopShell.vue`: no change. The `Sidebar` is already conditional on `route.path === '/'`.

## `app/pages/search.vue`

Mirrors `saved.vue`:
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

## Error handling

None to consider. No network, no external dependencies, no mutations. The only failure surface is "no matches", handled by the empty-state row.

## Testing

Manual verification via `bun dev`:

1. Navigate to `/search` via rail click; also via direct URL.
2. Catalog renders: mini-courses row first, then CSS / JavaScript / Web APIs rows.
3. Horizontal scroll works within each row.
4. Type `grid` → results list replaces catalog; shows posts whose title/topic/related include "grid".
5. Clear the input → catalog returns.
6. Type `zzzzzz` → `// нічого не знайдено` shows.
7. Click a topic card, course card, "всі курси →", "показати всі →", and a result row → nothing happens, no console errors.
8. Navigate away and back → state resets (no persistence expected).
9. Open on narrow viewport (`isDesktop = false`) → blank shell, matching `/saved`.

No unit tests. The filter logic is three `.includes` calls; a test would be noise.

## Out of scope — explicitly deferred

- Recent searches / search history.
- Keyboard shortcut to focus the input (e.g. `/`).
- URL state (`?q=…`) so search is shareable.
- Real catalog of real topics from the API.
- The `/courses` page and mini-course progress state.
- Enabling the profile rail button.
- Mobile layout.

Any of these can be layered on later without restructuring the page.
