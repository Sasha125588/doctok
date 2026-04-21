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

- `DesktopShell.vue` — accepts a `default` slot for main content. `index.vue` keeps its current behavior (falls back to `<FeedPage />` when no slot provided, or passes `<FeedPage />` explicitly — whichever keeps the diff smallest and the pattern cleanest; decide during implementation).
- `Topbar.vue` — becomes route-aware: on `/saved` it shows a static title (`saved`) and hides the focus/browse pills. On `/` it behaves as today.
- `Rail.vue` — becomes route-aware and actually navigates: each enabled nav item has a target path, active state is derived from `useRoute().path`, clicks call `router.push(path)`. `saved` moves from disabled to enabled with `path: '/saved'`. `search` and `courses` remain disabled.
- `useFeedView.ts` — adds `pendingPostId: Ref<number | null>`, a one-shot handoff marker consumed by FocusMode.
- `useSavedPosts.ts` — adds `savedAt: number` to `SavedPost`. New entries get `Date.now()`. Existing entries without `savedAt` default to `0` on read (stable under migration — no data loss, just groups pre-migration entries at the end).

### Data flow: click a saved card

1. `SavedCard` click handler (not the remove button):
   - `activeTopicSlug.value = post.topicSlug`
   - `pendingPostId.value = post.postId`
   - `mode.value = 'focus'`
   - `router.push('/')`
2. On `/`, `FeedPage` renders `FocusMode` for the new slug. `FocusMode` (keyed on slug via `FeedPage`) mounts fresh and `useTopicPosts` starts loading.
3. A new watch inside `FocusMode` observes `[pendingPostId, state.posts.value]`. When both are set and the post list contains a matching id:
   - `activePostIndex.value = posts.findIndex((p) => +p.id === pendingPostId.value)`
   - `pendingPostId.value = null` (consume the marker so it doesn't re-fire)
4. If the post isn't in the loaded list (e.g. stale bookmark, post deleted), `pendingPostId` is still cleared and `activePostIndex` defaults to `0` — user sees the topic's first post instead of a broken state.

### Data flow: remove from saved page

1. `SavedCard` remove button click (with `@click.stop` to prevent card navigation).
2. Calls `useSavedPosts().toggle(post)` — the existing toggle already handles both add and remove.
3. The grid re-renders. `AnimatePresence` fades the card out.

## UI spec

### SavedPage layout

- Header row: `// saved` label (monospace, tertiary color) + count badge (`{n} posts`).
- Grid: `grid-template-columns: repeat(auto-fill, minmax(220px, 1fr))`, gap 12px, padding 20px, scroll inside main.
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
- **localStorage migration.** `savedAt` missing on old entries → default to 0. They sort to the end, but still render. No destructive rewrite on read.
- **Sort stability.** Two entries with identical `savedAt` (should be rare, only if rapid toggles) — stable-sort by insertion order (JS `Array.prototype.sort` is stable in modern engines, fine).
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
9. Rail: `saved` button is highlighted while on `/saved`; `feed` is highlighted on `/`.

## Out-of-scope follow-ups (for future specs)

- Search page (`/search`) — separate spec.
- Courses page (`/courses`) — separate spec, introduces new domain model.
- Mobile saved view.
- Grouping saved posts by topic (deferred until user feedback shows need).
- Sync saved to backend.
