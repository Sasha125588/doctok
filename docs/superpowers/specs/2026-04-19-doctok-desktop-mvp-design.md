# DocTok Desktop MVP — Design Spec

**Date:** 2026-04-19
**Reference:** `apps/web/design/doctok_desktop_v6.html` (committed alongside this spec).
**Status:** approved (pending spec review)

## Context

Existing web app (`apps/web`, Nuxt 4) is a mobile-first, TikTok-like feed — vertical swipe between topics, horizontal swipe between posts inside a topic. The reference `doctok_desktop_v6.html` is a much richer desktop multi-page layout (rail + sidebar + focus/browse + quiz + notes + courses + saved + progress + search).

This spec defines an **MVP desktop-only** port of the reference for viewports `≥1024px`. Mobile layout is untouched.

## Goals

- Ship a desktop layout that matches the visual character of `doctok_desktop_v6.html` for the **feed page only**.
- Preserve the existing mobile experience unchanged.
- Do not require any backend changes. All new interactive state that has no API lives in `localStorage`.
- Use `motion-v` for animations.

## Non-goals

- Mobile layout changes or responsive adaptation below 1024px (existing mobile stays).
- **Dedicated pages** for Quiz, Courses, Saved, Search, Progress. (The Saved *toggle* and the per-post Notes *panel* **are** in scope — only their list/index pages are out.)
- Tooltips on inline terms.
- Pinned/Recent sync across devices (localStorage only).
- PWA / native app / offline.
- Real read-mode variants (stub only).
- Category breadcrumb (not in API).
- Deep-link URL to a specific post (only topic-level URL works).
- New backend endpoints.

## Architecture

### Layout decision

`app/pages/index.vue` picks layout based on `useMediaQuery('(min-width: 1024px)')`:

- `false` → existing `<TopicFeed />` (mobile, unchanged)
- `true`  → new `<DesktopShell />`

Mobile and desktop share API composables (`useFeed`, `useTopicPosts`, `useVote`, `useTopicLinks`, `useLang`) but have separate component trees.

### Desktop shell structure

```
DesktopShell
├── Rail (50px, left)                 — nav icons, only "feed" active in MVP
├── Sidebar (0 or 168px, left)        — pinned + recent topics
└── Main (flex)
    ├── Topbar (46px)                 — page title + focus/browse + read-mode
    └── FeedPage
        ├── FocusMode                 — single card + actions column
        │   or
        ├── BrowseMode                — 3-col grid of posts in active topic
        ├── CommentsPanel (slide-in, right, 210px)
        └── NotesPanel    (slide-in, right, 210px)
```

Side panels are mutually exclusive (only one open at a time).

## Components

All new desktop components live under `app/components/desktop/`.

| Component | Responsibility |
|---|---|
| `DesktopShell.vue` | Root layout. Owns `FeedViewState` via `useFeedView()`. Renders Rail, Sidebar, Main. |
| `Rail.vue` | 50px column. Logo "DOC". Feed icon active. Other icons (search/courses/saved/profile) rendered but disabled with `title="скоро"` tooltip. |
| `Sidebar.vue` | Headers "pinned" and "recent". Lists topics from `useTopicHistory()`. Active item shows left accent bar via `motion-v` `layoutId`. Clicking sets `activeTopicSlug`. |
| `Topbar.vue` | Left: page title "feed". Center: focus/browse toggle. Right: read-mode toggle. Mode toggles use `motion-v` `layoutId` for the sliding active pill. |
| `FeedPage.vue` | Container. Switches between `<FocusMode>` and `<BrowseMode>` with `AnimatePresence mode="wait"` fade. Mounts `<CommentsPanel>` and `<NotesPanel>` in a flex row. |
| `FocusMode.vue` | Single-card column: `<CardMeta>`, `<PostTitle>`, `<PostCardBody>` (reused), `<RelatedTags>`, and a right-aligned `<ActionsColumn>`. Wraps card in `motion.div` with fade+slide on `activePostIndex`/`activeTopicSlug` change (AnimatePresence mode="wait"). Owns local refs `localMyVote`/`localLikeCount`/`localDislikeCount` seeded from the active post (mirroring `PostCardActions.vue` pattern) and instantiates `useVote({postId, topicSlug, localMyVote, localLikeCount, localDislikeCount})` once per active post. Passes `functions.onVote` + counts to `<ActionsColumn>`. |
| `BrowseMode.vue` | 3-column grid of `BrowseCard` (inline or small component), each card = title + kind badge + short body. Click → switches to focus mode at that post index. |
| `CardMeta.vue` | Row: `topicTitle` → · kind badge · progress dots · source badge "MDN". Kind badge colors from `usePostKind`. Progress dots width animates via `motion.div` with `animate={{ width }}`. |
| `ActionsColumn.vue` | Dumb/presentational. Props: `myVote`, `likeCount`, `commentCount`, `isSaved`, `hasNote`. Emits: `onVote(ReactionValue)`, `onToggleSave`, `onOpenNote`, `onOpenComments`, `onShare`. Vertical icon buttons with counts. Spring scale pop on toggle. |
| `RelatedTags.vue` | Row of tag buttons. Data from `useTopicLinks({query: {slug: activeTopicSlug, lang}}, enabledRef)` → read `state.links`. Click navigates to that topic via `activeTopicSlug = link.slug`. |
| `CommentsPanel.vue` | Slide-in from right. Scrollable comments list. Textarea + send button. Real API via `useComments`. |
| `NotesPanel.vue` | Slide-in from right. Textarea for per-post notes. Save button writes to `useNotes`. |
| `ReadModeToggle.vue` | 4 buttons: simplified / standard / detailed / original. Stub — sets `readMode` ref, no effect on data. |
| `DesktopSidePanel.vue` | Generic wrapper for Comments/Notes panels — slide-in (`animate={{ width }}`), header with title + close button, slotted body. |

### Component reuse from existing tree

- `PostCardBody.vue` (existing) — reused for body HTML rendering inside `FocusMode`.
- `usePostKind.ts` (existing) — kind colors/labels reused for meta badge.

## Composables (new)

| Composable | Purpose | Storage |
|---|---|---|
| `useDesktop()` | `MediaQuery('(min-width: 1024px)')` → `Ref<boolean>` | - |
| `useFeedView()` | Reactive state: `activeTopicSlug`, `activePostIndex`, `mode: 'focus'\|'browse'`, `readMode`, `activePanel: 'comments'\|'notes'\|null`. Singleton (module-level refs) so Sidebar and Main share state. | - |
| `useTopicHistory()` | `pinned: string[]`, `recent: string[]` (slugs). Methods: `addRecent(slug)`, `togglePin(slug)`. Auto-trim recent to 5. | `localStorage` |
| `useSavedPosts()` | `list: {postId, topicSlug, title, kind}[]`. Methods: `toggle(post)`, `isSaved(postId)`. | `localStorage` |
| `useNotes()` | Map `postId → text`. Methods: `get(postId)`, `set(postId, text)`, `remove(postId)`. | `localStorage` |
| `useReadMode()` | Stub. `readMode: Ref<'simplified'\|'standard'\|'detailed'\|'original'>`. Does not affect data. | - |
| `useComments(postId)` | `useQuery(postsCommentsListOptions)` for list; `useMutation` for create via `postsCommentsCreate`. Invalidates list on create. | API |

All composables use `@vueuse/core` `useLocalStorage` where persistence is needed, so reactivity is automatic.

## Data flow

### Topics list
- `useFeed(lang)` (existing, returns `{state: {topics, hasNextPage, isLoading, isFetchingNextPage}, functions: {fetchNextPage}}`) → infinite list of topics (paginated).
- `Sidebar` shows intersection: pinned (from `useTopicHistory`) + recent (from `useTopicHistory`). Topics not in history are not listed in sidebar (by design — sidebar is "Recent & Pinned" only).
- Topic lookup in Sidebar resolves slug → title by reading `state.topics.value` and matching by `slug`. If not in current feed page (e.g. pinned slug outside loaded range), show slug as-is.

### Active topic
- `activeTopicSlug` in `useFeedView`. Defaulted to `state.topics.value[0]?.slug` once `useFeed` loads.
- When it changes: push via `useTopicHistory.addRecent(slug)`, reset `activePostIndex = 0`.

### Posts for active topic
- `useTopicPosts({query: {slug: activeTopicSlug, lang}})` (existing, returns `{state: {posts, ...query}}`) → read `state.posts.value`.
- `activePostIndex` selects current post.
- `progress dots` width = total posts count.

### Related tags
- `useTopicLinks({query: {slug: activeTopicSlug, lang}}, enabledRef)` (existing) → read `state.links.value` (array of `TopicLink`).
- `enabledRef` = `computed(() => !!activeTopicSlug.value)`.
- Each tag → button; click sets `activeTopicSlug = link.slug`.

### Keyboard
Attached to `window` via `onKeyStroke` (`@vueuse/core`). Only active when:
- `useDesktop.value === true`
- `activePanel === null`
- `document.activeElement.tagName` ∉ `['INPUT', 'TEXTAREA']`

Bindings:
- `ArrowLeft` → `activePostIndex = max(0, i - 1)`
- `ArrowRight` → `activePostIndex = min(len - 1, i + 1)`
- `ArrowUp` → previous topic in the **full `useFeed` topics list** (not sidebar). No-op at index 0.
- `ArrowDown` → next topic in the **full `useFeed` topics list**. If at last loaded topic and `state.hasNextPage.value === true`, call `functions.fetchNextPage()` and set `activeTopicSlug` to the first newly-loaded topic on resolution. Otherwise no-op.

### Like / vote
- `FocusMode` owns local refs `localMyVote` / `localLikeCount` / `localDislikeCount`, seeded from the active `TopicPostView` via `watch(() => activePost.value, ...)` (mirrors `PostCardActions.vue`).
- Instantiates `useVote({postId: +activePost.id, topicSlug: activePost.topicSlug, localMyVote, localLikeCount, localDislikeCount})`.
- `ActionsColumn` receives `functions.onVote` (emitted as `@onVote(value)`) and the local counts/myVote as props.
- `useVote` internally updates the `@tanstack/vue-query` cache for `topicsGetPosts`, so the post will re-render with fresh counts on next computed evaluation.

### Save
- `useSavedPosts.toggle(post)`. Visual state via `isSaved(post.id)`.
- Saved page is out of scope, but the toggle works for future consumption.

### Note
- Click note button → opens `NotesPanel` (sets `activePanel = 'notes'`).
- Panel textarea seeded with `useNotes.get(activePostId)`.
- Save button writes via `useNotes.set(activePostId, text)`.

### Comments
- Click comment icon → opens `CommentsPanel` (sets `activePanel = 'comments'`).
- Panel loads `useComments(activePostId)` — real API.
- Send button posts via mutation, invalidates list, scrolls to bottom.

### Share
- `navigator.share({url})` with fallback to copy-to-clipboard + in-DOM toast ("скопійовано").
- URL = `${location.origin}/topic/${topicSlug}`. The existing `/topic/[...slug]` route does not currently deep-link into a specific post index, so the hash `#post-${id}` is omitted for MVP. (Deep-linking to a post is a future follow-up.)

## Design tokens

Existing CSS variables (in `app/assets/css/tailwind.css` `:root`) are kept. New tokens added:

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

Desktop layout overrides `--font-display` to Georgia within a `.desktop-scope` class wrapping the shell root:

```css
.desktop-scope {
  --font-display: Georgia, 'Palatino Linotype', serif;
}
```

This keeps Syne as the mobile display font and Georgia on desktop only. Mono font (`DM Mono`) stays global.

## Motion (motion-v)

Installed via `bun add motion-v`. Usage below.

| Target | Animation |
|---|---|
| Sidebar collapse | `motion.aside` `animate={{ width: hidden ? 0 : 168 }}`, `transition={{ duration: 0.22, ease: 'easeInOut' }}` |
| Focus card on prev/next/topic change | `AnimatePresence mode="wait"` wrapping keyed `motion.div`, `initial={{ opacity: 0, x: dir * 14 }}`, `animate={{ opacity: 1, x: 0 }}`, `exit={{ opacity: 0, x: -dir * 14 }}`, 180ms |
| Mode switch focus↔browse | Same `AnimatePresence`, fade-only (no x offset) |
| Side panels | `motion.aside` `animate={{ width: open ? 210 : 0 }}`, 220ms |
| Progress dots | Each dot is `motion.div` with `animate={{ width: active ? 18 : 5, backgroundColor: active ? activeColor : '#1a1a1a' }}`, 220ms |
| Action button click (like/save/note) | Spring scale `1 → 1.2 → 1`, 200ms |
| Topic item active bar | Shared `layoutId="dt-active-topic-bar"` between items for smooth slide |
| Read-mode toggle active pill | Shared `layoutId="dt-rm-active"` across the 4 buttons |
| Mode toggle active pill | Shared `layoutId="dt-mode-active"` across focus/browse |

Rail hover and kind-badge pulse remain CSS-only (motion would be overkill).

## Files

### New

```
app/components/desktop/
  DesktopShell.vue
  Rail.vue
  Sidebar.vue
  Topbar.vue
  FeedPage.vue
  FocusMode.vue
  BrowseMode.vue
  CardMeta.vue
  ActionsColumn.vue
  RelatedTags.vue
  CommentsPanel.vue
  NotesPanel.vue
  ReadModeToggle.vue
  DesktopSidePanel.vue

app/composables/
  useDesktop.ts
  useFeedView.ts
  useTopicHistory.ts
  useSavedPosts.ts
  useNotes.ts
  useReadMode.ts
  useComments.ts
```

### Modified

- `app/pages/index.vue` — select mobile vs desktop shell.
- `app/assets/css/tailwind.css` — new tokens + `.desktop-scope` font override.
- `apps/web/package.json` — add `motion-v`.

### Untouched

- `app/components/feed/*`, `app/components/post/*`, `app/components/topic/*`, `app/pages/topic/[...slug].vue`, existing composables.

## Backend

No changes. All new features either reuse existing endpoints (comments, topic links, votes, feed, posts) or live in localStorage (pinned/recent, saved, notes).

## Testing

Manual desktop verification per the feature testing policy (Nuxt UI, `bun dev`):

1. Viewport ≥1024: desktop shell renders; <1024: mobile feed renders.
2. Sidebar shows pinned/recent; clicking a topic switches content.
3. Focus mode: meta row, title, body, related tags, actions column.
4. Browse mode: grid; click card → switches to focus at that index.
5. Keyboard ←→ navigates posts; ↑↓ switches topic.
6. Comments panel opens, loads real API, POST creates a comment.
7. Notes panel opens, textarea persists across reloads via localStorage.
8. Save button toggles persistence (verified via localStorage inspection).
9. Read-mode buttons animate but body doesn't change (stub confirmed).
10. Related tag click switches topic (when link exists in feed).
11. Motion transitions: card slide on prev/next, panel slide-in, dots morph, active bar shared layout.

## Open questions

None. All scope decisions confirmed with user.
