# Saved Page — Design Spec

**Date:** 2026-04-21
**Scope:** Desktop `/saved` page. Mobile out of scope.
**Parent:** Builds on the Desktop MVP shell (see `2026-04-19-doctok-desktop-mvp-design.md`).

## Goal

Give users a dedicated desktop page at `/saved` listing every post they've bookmarked. Clicking a card returns them to the feed in focus mode positioned on that exact post. Users can also remove a bookmark from the saved page without navigating away.

Data source is the existing `useSavedPosts` localStorage composable — no backend work. `search` and `courses` pages are explicitly out of scope for this spec; they get their own specs later.

## Non-goals

- No mobile layout (desktop Rail is the entry point; mobile navigation is a separate future problem).
- No grouping by topic, no sorting controls, no drag-to-reorder, no "mark as read" — YAGNI.
- No backend sync. Saved state stays in localStorage.
- No loading state — localStorage is synchronous.

## User flow

1. User on `/` clicks the `saved` icon in the Rail → navigates to `/saved`.
2. Saved page renders a grid of cards, one per saved post, newest first.
3. User clicks a card → app sets the active topic + a pending-post marker, navigates to `/`, FocusMode resolves the exact post once topic data loads.
4. Alternatively, user hovers a card and clicks the remove icon → card disappears from the grid; saved count decreases.
5. If saved list is empty, an empty-state message is shown with a link back to `/`.

## Architecture

### Routes

- `apps/web/app/pages/saved.vue` — new Nuxt page. Renders `DesktopShell` with a `<SavedPage />` inside the main slot.
- `pages/index.vue` and `pages/topic/[...slug].vue` unchanged.

### Components

New:

- `apps/web/app/components/desktop/SavedPage.vue` — grid + empty state + header with count.
- `apps/web/app/components/desktop/SavedCard.vue` — one card: kind badge, title, topic label, hover-revealed remove button.

Modified:

- `DesktopShell.vue` — content becomes a `default` slot. Callers supply the main content explicitly. `index.vue` is updated to pass `<FeedPage />` into the slot. No fallback content inside the shell — the shell is content-agnostic, each route owns its main view. This is the only acceptable option; fallback content would hide coupling between the shell and the feed, which is exactly what this change removes.
- `Topbar.vue` — becomes route-aware: on `/saved` it shows a static title (`saved`) and hides the focus/browse pills. On `/` it behaves as today.
- `Rail.vue` — becomes route-aware and actually navigates: each enabled nav item has a target path, active state is derived from `useRoute().path`, clicks call `router.push(path)`. `saved` moves from disabled to enabled with `path: '/saved'`. `search` and `courses` remain disabled.
- `useFeedView.ts` — adds `pendingPostId: Ref<number | null>`, a one-shot handoff marker written by SavedCard, consumed by FocusMode (see §Data flow for the consume contract).
- `useSavedPosts.ts` — `SavedPost` gains a required `savedAt: number` field. Writes always set `savedAt: Date.now()`. Reads expose a derived default of `0` for legacy entries that predate the field — no write-back to localStorage on read (no migration side effect; legacy entries stay legacy until the user toggles them). Sort order is always `savedAt desc`, so legacy entries sink to the bottom.

`SavedPost` type shape (before → after):

```ts
// Before
interface SavedPost {
  postId: number
  topicSlug: string
  title: string
  kind: string
}

// After
interface SavedPost {
  postId: number
  topicSlug: string
  title: string
  kind: string
  savedAt: number  // Date.now() at toggle-on time; 0 for legacy entries
}
```

### Data flow: click a saved card

1. `SavedCard` click handler (not the remove button). All four mutations fire synchronously in this order — the `router.push` comes last so FocusMode sees consistent state when it mounts or re-evaluates:
   - `activeTopicSlug.value = post.topicSlug`
   - `pendingPostId.value = post.postId`
   - `mode.value = 'focus'`
   - `router.push('/')`
2. On `/`, `FeedPage` renders `FocusMode` for the active slug. `FocusMode` is keyed on slug (via `FeedPage`), so changing slug remounts it; same-slug re-entry does not remount.
3. **Consumer lives in `FocusMode.vue`** (not in `useFeedView`). It's a `watchEffect` with both `pendingPostId` and `state.posts.value` as reactive reads:
   - If `pendingPostId.value == null`: no-op.
   - Else if `state.posts.value.length === 0`: wait (the effect re-runs when posts arrive).
   - Else: find `idx = posts.findIndex((p) => +p.id === pendingPostId.value)`. Set `activePostIndex.value = idx >= 0 ? idx : 0`. Then set `pendingPostId.value = null` to consume the marker.
   - The effect runs in both paths — fresh mount (slug changed, posts load async) and same-slug re-entry (posts already loaded, runs on next tick after `pendingPostId` flips).
4. Stale bookmark (post missing from loaded list): still consumed, index defaults to 0 — user lands on the topic's first post rather than in a broken state.

**Why `watchEffect` instead of a plain `watch`:** same-slug re-entry has no slug-change trigger; `FocusMode` stays mounted and `state.posts.value` is already populated. A `watchEffect` fires on `pendingPostId` flipping from `null → number` without needing slug to change.

### Data flow: remove from saved page

1. `SavedCard` remove button click (with `@click.stop` to prevent card navigation).
2. Calls `useSavedPosts().toggle(post)` — the existing toggle already handles both add and remove.
3. The grid re-renders. `AnimatePresence` fades the card out.

## UI spec

### SavedPage layout

- Header row: `// saved` label (monospace, tertiary color) + count badge (`{n} posts`).
- Grid: `grid-template-columns: repeat(auto-fill, minmax(220px, 1fr))`, gap 12px, padding 20px.
- Scroll ownership: `SavedPage` root is the scroll container (`flex: 1; overflow-y: auto`). The shell's main slot stays non-scrolling — each page owns its own overflow behavior. This matches how `FeedPage` composes inside the current shell.
- Cards enter with `motion.div` `initial={opacity:0, y:4} / animate={opacity:1, y:0}`, staggered by `index * 20ms`. Removal wrapped in `AnimatePresence` for fade-out.

### SavedCard visual

- Background `var(--dt-panel-bg)`, 1px border `var(--dt-panel-border)`, border-radius matching other desktop cards, padding ~12px.
- Top: kind badge using the `--kind-*` token for that kind (same mapping as FocusCard).
- Middle: post `title`, 11–13px, `color: #c8c8c0`, 2-3 line clamp.
- Bottom: topic label, monospace, `var(--dt-text-tertiary)`, format `// topic: {topicSlug}`.
- Top-right: remove button (`lucide:bookmark-minus` or similar), hidden by default (`opacity: 0`), revealed on card hover (`opacity: 1`) via CSS transition. `title="видалити зі збережених"`.

### Empty state

Shown when `saved.value.length === 0`:

```
// тут порожньо
// збережи свій перший пост у стрічці →
```

- Monospace, `var(--dt-text-quaternary)`.
- `→` is a `<NuxtLink to="/">` with the arrow-brighter-on-hover style used elsewhere.
- Vertically centered in the main area.

### Topbar on `/saved`

- Static title: `saved` in the same style as feed-side topic title.
- Focus/browse pills and topic-specific controls hidden.

### Rail

- `feed` → `path: '/'`, `saved` → `path: '/saved'`, `search` / `courses` stay disabled.
- `active = useRoute().path === item.path`.
- Click on enabled item: `router.push(item.path)`.

## Edge cases

- **Stale bookmark (post no longer in topic).** Handled by the `pendingPostId` consumer: if no matching id found, clear marker and fall through to index 0.
- **Topic slug no longer exists.** `useTopicPosts` will surface an empty / error state the same way it does for any missing topic — no additional handling required here.
- **Click a saved card for the topic already active in focus mode.** Slug doesn't change → `FocusMode` does not remount. The `watchEffect` consumer still fires because `pendingPostId` flips `null → number` and is reactive. Posts are already loaded, so the effect sets `activePostIndex` immediately and clears the marker. This is the main regression vector for the handoff — covered explicitly here because the rest of the handoff path assumes slug change.
- **localStorage migration.** `savedAt` missing on old entries → default to 0 derived at read time. They sort to the end (newest first), but still render. No write-back to localStorage on read.
- **Sort stability.** Two entries with identical `savedAt` (rare, only if rapid toggles) — stable-sort by insertion order (JS `Array.prototype.sort` is stable in modern engines, fine).
- **Rail active state on other routes** (e.g. `/topic/foo`). No Rail entry matches → no item is active. Acceptable for MVP; revisit when more pages land.

## Testing

Manual acceptance (automated tests are not yet a pattern in this workspace):

1. Navigate `/` → click Rail `saved` icon → lands on `/saved`.
2. Empty state: fresh localStorage → see "тут порожньо" + link → click `→` → back on `/`.
3. Non-empty: save a few posts from feed → visit `/saved` → see grid, newest first, correct kind colors, correct titles.
4. Click a saved card → lands on `/` in focus mode on that exact post (same topic, same index).
5. Click a saved card for a topic you're already viewing → still lands on the correct post (pendingPostId must win).
6. Hover a card → remove icon appears → click it → card fades out, `/saved` count decreases, other cards stay.
7. Refresh `/saved` → order preserved, count preserved (localStorage-backed).
8. Arrow-key navigation from the newly-focused post works (verifies handoff didn't break the existing keyboard handler).
9. Rail: `saved` button is highlighted while on `/saved`; `feed` is highlighted on `/`. On `/topic/foo` neither is highlighted — this is the documented MVP fallback, not a bug.

## Out-of-scope follow-ups (for future specs)

- Search page (`/search`) — separate spec.
- Courses page (`/courses`) — separate spec, introduces new domain model.
- Mobile saved view.
- Grouping saved posts by topic (deferred until user feedback shows need).
- Sync saved to backend.
