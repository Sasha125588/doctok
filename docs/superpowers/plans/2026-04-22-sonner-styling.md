# Sonner Styling Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restyle the existing `vue-sonner` toaster to match the DocTok desktop aesthetic (dark panel, mono typography, colored left-border accent per state) across all six variants — neutral / success / error / info / warning / loading.

**Architecture:** Single-file change in `apps/web/app/components/ui/sonner/Sonner.vue`. No wrapper composable, no call-site changes. Styling is driven by (a) extended CSS custom properties on the `<Sonner>` element that vue-sonner consumes internally, and (b) a non-scoped `<style>` block with selectors on vue-sonner's data attributes to handle the per-type accent bar and hover-revealed close button. A temporary dev-only test harness is added first (test-first approach) so the visual change has something concrete to verify against, then removed after acceptance.

**Tech Stack:** Vue 3.5, Nuxt 4, `vue-sonner` 2.0.9, `lucide-vue-next`, `DM Mono` (already loaded via Tailwind).

**Spec:** `docs/superpowers/specs/2026-04-22-sonner-styling-design.md`

**Package manager:** `bun` (not npm/pnpm). Every command below uses `bun run <script>`.

**Branch / worktree:** Work directly on the currently checked-out branch. This is a single-file cosmetic change with no code conflicts — no worktree needed.

---

## File Structure

**Modified files:**

- `apps/web/app/components/ui/sonner/Sonner.vue` — sole structural change. Responsibilities stay the same: receive `ToasterProps`, render `<Toaster>` from vue-sonner, override icon slots with Lucide icons. Adds extended CSS custom properties, adds a non-scoped `<style>` block, adds explicit `position="bottom-right"` prop.

**Temporarily modified (added in Task 1, removed in Task 4):**

- `apps/web/app/components/desktop/FeedPage.vue` — gets a 6-button dev-only harness at the top behind `import.meta.dev`. Reverted at the end.

**Untouched files to be aware of:**

- `apps/web/app/app.vue` — mounts `<Sonner />` once; no changes.
- `apps/web/app/components/desktop/FocusCard.vue` — line 48 calls `toast('скопійовано')`; no changes, just visually different after this work.
- `apps/web/app/assets/css/tailwind.css` — source of the theme tokens we reference (`--font-mono`, `--kind-example`, `--kind-summary`, `--kind-fact`, `--destructive`, `--dt-text-tertiary`, `--dt-text-quaternary`). No changes.

---

## Verified vue-sonner API (version 2.0.9 as installed)

Confirmed in `node_modules/.bun/vue-sonner@2.0.9*/node_modules/vue-sonner/lib/index.css`:

**CSS custom properties exposed per toast type:**
`--normal-bg`, `--normal-border`, `--normal-text`, `--success-bg`, `--success-border`, `--success-text`, `--error-bg`, `--error-border`, `--error-text`, `--info-bg`, `--info-border`, `--info-text`, `--warning-bg`, `--warning-border`, `--warning-text`. Also `--border-radius`. No dedicated `--loading-*` vars — loading reuses normal.

**Data attributes on rendered DOM:**
`data-sonner-toaster` (on the container), `data-sonner-toast` (on each toast), `data-type` (values: `normal` | `success` | `error` | `info` | `warning` | `loading`), `data-close-button` (on the `×` button), `data-icon` (on the icon wrapper), `data-title`, `data-description`.

Plan relies on these exact names. If a future vue-sonner upgrade changes them, this plan's selectors break silently (styles stop applying; Task 3's manual checklist catches it).

---

## Chunk 1: Implementation and verification

### Task 1: Add dev-only test harness to FeedPage

Goal: Have a reliable way to fire all 6 toast variants in dev before we change any styling. This is the "failing test" — it exercises Sonner in its current default-shadcn state, so we can compare before/after.

**Files:**
- Modify: `apps/web/app/components/desktop/FeedPage.vue`

- [ ] **Step 1: Open the file and locate the `<script setup>` and `<template>` blocks**

Read `apps/web/app/components/desktop/FeedPage.vue`. Current script imports `BrowseMode`, `CommentsPanel`, `FocusMode`, `NotesPanel`, `useFeedView`. Current template root is `<div class="feed-page">` containing `<div class="stack">` with FocusMode/BrowseMode.

- [ ] **Step 2: Add `toast` import to `<script setup>`**

In the `<script setup lang="ts">` block, add a new import line after the existing component imports:

```ts
import { toast } from 'vue-sonner'
```

- [ ] **Step 3: Add the dev-only harness to the template**

Immediately inside the `<div class="feed-page">` opening tag (before `<div class="stack">`), insert:

```vue
<div
  v-if="isDev"
  class="toast-harness"
>
  <button @click="toast('скопійовано')">neutral</button>
  <button @click="toast.success('пост збережено')">success</button>
  <button @click="toast.error('не вдалось зберегти пост')">error</button>
  <button @click="toast.info('доступна нова версія')">info</button>
  <button @click="toast.warning('повторити запит?')">warning</button>
  <button @click="toast.loading('збереження нотатки…')">loading</button>
</div>
```

Then in the `<script setup>` block add a line that exposes `import.meta.dev` as a template-accessible name:

```ts
const isDev = import.meta.dev
```

(Nuxt templates can't read `import.meta.dev` directly; aliasing it to a const makes it accessible as `isDev` in `v-if`.)

- [ ] **Step 4: Add minimal styles for the harness**

At the bottom of the existing `<style scoped>` block (or in a new `<style scoped>` block if none exists — there is one currently with `.feed-page`, `.stack`, `.pane` rules), add:

```css
.toast-harness {
  position: fixed;
  top: 56px;
  right: 20px;
  z-index: 100;
  display: flex;
  gap: 6px;
  font-family: var(--font-mono);
  font-size: 10px;
  pointer-events: auto;
}
.toast-harness button {
  padding: 4px 8px;
  background: #0a0a0a;
  border: 1px solid #1a1a1a;
  color: #c8c8c0;
  border-radius: 4px;
  cursor: pointer;
}
.toast-harness button:hover {
  border-color: #2a2a2a;
}
```

`position: fixed` anchors the harness to the viewport, which avoids a trap: `.feed-page` is `display: flex; overflow: hidden` and is not `position: relative`, so `position: absolute` on the harness would have anchored to an unpredictable ancestor (and potentially been clipped). `top: 56px` clears the Topbar.

- [ ] **Step 5: Run the dev server and verify the harness appears**

Run:

```bash
bun run dev
```

Expected: dev server starts. Open `http://localhost:3000/` in the browser. Expected: six buttons (`neutral`, `success`, `error`, `info`, `warning`, `loading`) appear fixed in the top-right of the viewport, below the Topbar.

- [ ] **Step 6: Click each button and confirm Sonner fires (current default look)**

Click each button in turn. Expected: Sonner shows each toast in its current default style (light/popover background from shadcn defaults). This is the "before" state — styles will change in Task 2.

Stop the dev server (`Ctrl+C`).

- [ ] **Step 7: Commit**

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok"
git add apps/web/app/components/desktop/FeedPage.vue
git commit -m "test: add dev-only toast harness to FeedPage

Six buttons behind import.meta.dev exercise all vue-sonner variants.
To be removed after Sonner styling verification passes."
```

---

### Task 2: Restyle Sonner.vue

Goal: Replace the default shadcn styling with DocTok's dark/mono/left-accent design for all six variants at once.

**Files:**
- Modify: `apps/web/app/components/ui/sonner/Sonner.vue`

Read the current file first — it has a `<script setup>` block importing Lucide icons, a `<template>` with a single `<Sonner>` (aliased `Toaster`) element that sets CSS variables via `:style` and overrides icon slots. There is currently no `<style>` block in this file.

- [ ] **Step 1: Replace the `:style` object with the full palette**

In the `<template>` block, the current `:style` binding reads:

```vue
:style="{
  '--normal-bg': 'var(--popover)',
  '--normal-text': 'var(--popover-foreground)',
  '--normal-border': 'var(--border)',
  '--border-radius': 'var(--radius)',
}"
```

Replace it entirely with:

```vue
:style="{
  '--normal-bg': '#0a0a0a',
  '--normal-text': '#c8c8c0',
  '--normal-border': '#1a1a1a',
  '--success-bg': '#0a0a0a',
  '--success-text': '#c8c8c0',
  '--success-border': '#1a1a1a',
  '--error-bg': '#0a0a0a',
  '--error-text': '#c8c8c0',
  '--error-border': '#1a1a1a',
  '--info-bg': '#0a0a0a',
  '--info-text': '#c8c8c0',
  '--info-border': '#1a1a1a',
  '--warning-bg': '#0a0a0a',
  '--warning-text': '#c8c8c0',
  '--warning-border': '#1a1a1a',
}"
```

All six types share the same background / text / outer-border because the visual differentiation happens via the left-border accent (applied in Step 3 via the `<style>` block). The `--border-radius` var is removed because the `<style>` block in Step 3 sets `border-radius: 6px` directly on `[data-sonner-toast]`.

- [ ] **Step 2: Add the explicit `position` prop to `<Sonner>`**

On the `<Sonner>` element, add a `position` prop. The full opening tag becomes:

```vue
<Sonner
  :class="cn('toaster group', props.class)"
  position="bottom-right"
  :style="{ /* ...extended palette from Step 1... */ }"
  v-bind="props"
>
```

Keep `:class` and `v-bind="props"` as they are today — they remain extension points for consumers.

- [ ] **Step 3: Add a non-scoped `<style>` block at the end of the file**

Append this block after the closing `</template>`:

```vue
<style>
[data-sonner-toast] {
  font-family: var(--font-mono);
  font-size: 12px;
  letter-spacing: 0.2px;
  padding: 10px 14px 10px 12px;
  min-width: 240px;
  border-left-width: 2px;
  border-left-style: solid;
  border-left-color: #2a2a2a;
  border-radius: 6px;
}
[data-sonner-toast][data-type='success'] {
  border-left-color: var(--kind-example);
}
[data-sonner-toast][data-type='success'] [data-icon] svg {
  color: var(--kind-example);
}
[data-sonner-toast][data-type='error'] {
  border-left-color: var(--destructive);
}
[data-sonner-toast][data-type='error'] [data-icon] svg {
  color: var(--destructive);
}
[data-sonner-toast][data-type='info'] {
  border-left-color: var(--kind-summary);
}
[data-sonner-toast][data-type='info'] [data-icon] svg {
  color: var(--kind-summary);
}
[data-sonner-toast][data-type='warning'] {
  border-left-color: var(--kind-fact);
}
[data-sonner-toast][data-type='warning'] [data-icon] svg {
  color: var(--kind-fact);
}
[data-sonner-toast][data-type='loading'] [data-icon] svg {
  color: var(--dt-text-tertiary);
}

[data-sonner-toast] [data-close-button] {
  opacity: 0;
  transition: opacity 120ms ease-out;
  color: var(--dt-text-quaternary);
}
[data-sonner-toast]:hover [data-close-button] {
  opacity: 1;
}
</style>
```

Colors are referenced via existing theme tokens where they exist (`var(--kind-example)`, `var(--destructive)`, `var(--kind-summary)`, `var(--kind-fact)`, `var(--dt-text-tertiary)`, `var(--dt-text-quaternary)` — all defined in `apps/web/app/assets/css/tailwind.css`). Only the neutral accent `#2a2a2a` is hardcoded, per the spec's explicit call-out that no token exists for it and one shouldn't be created for a single use-site.

Important: no `scoped` modifier. vue-sonner renders toasts via `<Teleport>` to `document.body`, so a scoped `<style>` would not reach them.

Note on selector specificity: `[data-icon] svg` targets the Lucide `<svg>` that the icon slots render into vue-sonner's `data-icon` wrapper. This is how the per-type icon color gets applied without editing the slot templates.

- [ ] **Step 4: Resize the icon slots to 14px to match the spec**

The current slot icon overrides use `class="size-4"` (16px in Tailwind). The spec specifies 14px. Change every `class="size-4"` inside the `#success-icon`, `#info-icon`, `#warning-icon`, `#error-icon`, `#loading-icon`, `#close-icon` slots to `class="size-3.5"` (14px). The `animate-spin` on the loading icon stays.

The expected edits — 6 lines total, one per slot:

```vue
<template #success-icon>
  <CircleCheckIcon class="size-3.5" />
</template>
<template #info-icon>
  <InfoIcon class="size-3.5" />
</template>
<template #warning-icon>
  <TriangleAlertIcon class="size-3.5" />
</template>
<template #error-icon>
  <OctagonXIcon class="size-3.5" />
</template>
<template #loading-icon>
  <div>
    <Loader2Icon class="size-3.5 animate-spin" />
  </div>
</template>
<template #close-icon>
  <XIcon class="size-3.5" />
</template>
```

The `<script setup>` block (imports of `CircleCheckIcon`, `InfoIcon`, `Loader2Icon`, `OctagonXIcon`, `TriangleAlertIcon`, `XIcon`, `Toaster as Sonner`, `cn`, `ToasterProps`) stays exactly as it is.

- [ ] **Step 5: Lint + format check**

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok/apps/web"
bun run lint
bun run fmt:check
```

Expected: both pass with 0 errors.

If `fmt:check` reports the edited file, run:

```bash
bun run fmt
```

then re-run `bun run fmt:check` and verify it passes.

- [ ] **Step 6: Commit**

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok"
git add apps/web/app/components/ui/sonner/Sonner.vue
git commit -m "feat(ui): restyle Sonner toasts with dark panel and accent borders

All six variants (neutral/success/error/info/warning/loading) share
dark background (#0a0a0a), mono typography, and a 2px colored
left-border accent per state. Close button fades in on hover."
```

---

### Task 3: Manual acceptance

Goal: Run through the spec's acceptance checklist with real toasts.

**Files:**
- No file changes in this task.

- [ ] **Step 1: Start the dev server**

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok/apps/web"
bun run dev
```

Open `http://localhost:3000/` in the browser. Six harness buttons appear at the top-right of the feed area.

- [ ] **Step 2: Fire each variant and verify the visual spec**

For each button, click and verify the corresponding toast renders as follows (see `docs/superpowers/specs/2026-04-22-sonner-styling-design.md` for the full spec):

| Button | Expected left-border color | Expected icon color | Icon |
|--------|---------------------------|---------------------|------|
| neutral | `#2a2a2a` (barely visible) | — (no icon) | No icon — vue-sonner renders neutral toasts as text-only; the spec leaves this behavior as-is |
| success | `#22c55e` (green) | `#22c55e` | CircleCheck |
| error | `#f87171` (red) | `#f87171` | OctagonX |
| info | `#3b82f6` (blue) | `#3b82f6` | Info |
| warning | `#f97316` (orange) | `#f97316` | TriangleAlert |
| loading | `#2a2a2a` (neutral border) | `#7a7f86` gray | Loader2 spinning |

All toasts should share: dark background `#0a0a0a`, thin border `#1a1a1a`, mono font `DM Mono`, 12px text, `#c8c8c0` text color, bottom-right position.

- [ ] **Step 3: Verify the hover-reveal close button**

Hover over a toast (any variant). Expected: an `×` appears on the right side of the toast, faded in over ~120ms. Click the `×`. Expected: the toast dismisses.

- [ ] **Step 4: Verify stacking**

Click `neutral`, `success`, `error` in quick succession. Expected: three toasts stack, newest on top, equal width, consistent vertical spacing. All three remain visible until they time out (~4s each).

- [ ] **Step 5: Verify the existing real caller works**

Navigate to `/` in focus mode with at least one post showing. Click the copy button on a post card (this triggers `toast('скопійовано')` in `FocusCard.vue:48`). Expected: a neutral toast appears in bottom-right with the new styling. No change needed at the call site.

- [ ] **Step 6: Verify on narrow viewport**

Resize the browser window to ~400px wide (or toggle devtools responsive mode). Fire any toast. Expected: toast stays within the viewport and remains readable. No horizontal scroll, no cut-off text.

- [ ] **Step 7: Record pass/fail**

If every check above passes, proceed to Task 4. If any check fails, fix the issue in `Sonner.vue` (the vast majority of failures will be selector typos or a mismatched CSS var name — open devtools, inspect the toast DOM to confirm the actual attribute/variable names against the plan), then re-run the checklist from Step 2.

Stop the dev server (`Ctrl+C`).

---

### Task 4: Remove the dev harness

Goal: Leave the tree clean.

**Files:**
- Modify: `apps/web/app/components/desktop/FeedPage.vue` (revert Task 1's additions)

- [ ] **Step 1: Remove the harness import, template block, const, and styles**

In `apps/web/app/components/desktop/FeedPage.vue`:

1. Remove the `import { toast } from 'vue-sonner'` line added in Task 1 Step 2.
2. Remove the `const isDev = import.meta.dev` line added in Task 1 Step 3.
3. Remove the entire `<div v-if="isDev" class="toast-harness">…</div>` block from the template.
4. Remove the `.toast-harness` and `.toast-harness button` CSS rules from the `<style scoped>` block.

After removal, the file should match its state before Task 1 (you can cross-check with `git show HEAD~2:apps/web/app/components/desktop/FeedPage.vue` after Task 2's commit — the harness commit is two commits back).

- [ ] **Step 2: Verify the diff is clean**

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok"
git diff apps/web/app/components/desktop/FeedPage.vue
```

Expected: the diff removes exactly the lines added in Task 1 — nothing else.

- [ ] **Step 3: Lint + format**

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok/apps/web"
bun run lint
bun run fmt:check
```

Expected: both pass.

- [ ] **Step 4: Commit**

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok"
git add apps/web/app/components/desktop/FeedPage.vue
git commit -m "test: remove dev-only toast harness from FeedPage

Sonner styling verified manually against the spec checklist;
harness is no longer needed."
```

---

### Task 5: Final validation

Goal: Catch any regression before handoff.

**Files:**
- No file changes.

- [ ] **Step 1: Build check**

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok/apps/web"
bun run build
```

Expected: build completes without errors. If this project does not have a `build` script, skip this step (check `apps/web/package.json` `scripts` section).

- [ ] **Step 2: Re-run lint + format**

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok/apps/web"
bun run lint
bun run fmt:check
```

Expected: both pass with 0 errors.

- [ ] **Step 3: Quick smoke test**

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok/apps/web"
bun run dev
```

Navigate to `/`, trigger `toast('скопійовано')` by copying a post (via FocusCard copy button). Verify the new styling still works. Stop the dev server.

- [ ] **Step 4: Confirm commit history is clean**

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok"
git log --oneline -5
```

Expected: three new commits on top — `test: add dev-only toast harness…`, `feat(ui): restyle Sonner toasts…`, `test: remove dev-only toast harness…`. No extra noise.

If everything passes, the feature is ready. Use @superpowers:finishing-a-development-branch to decide how to integrate (merge locally / open PR / keep / discard).

---

## Rollback plan

If something goes wrong and you need to back out:

```bash
cd "/Users/sasha/Documents/Visual Studio Code/webProjects/vue/doctok"
# Revert only the Sonner styling commit, keep the harness work
git revert <commit-sha-of-Sonner-restyle>

# Or nuke all three commits from this plan
git reset --hard HEAD~3   # destructive — only if nothing else is depending on these commits
```

The change is isolated to two files and one commit to `Sonner.vue`. Reverting that single commit restores the previous shadcn-default look without touching any call sites.
