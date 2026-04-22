# Sonner Styling — Design Spec

**Date:** 2026-04-22
**Scope:** Desktop toast notifications. Mobile inherits the same styles (no separate mobile treatment — toasts are small enough that the desktop design works on narrow viewports).

## Goal

Restyle the existing `vue-sonner` integration so toasts visually belong to the DocTok desktop shell — dark panel, mono typography, thin colored left-border accent for state. Do not introduce any wrapper API; existing `toast(...)`, `toast.success(...)`, `toast.error(...)`, `toast.info(...)`, `toast.warning(...)`, `toast.loading(...)` calls continue to work without code changes at call sites.

All six Sonner variants (`neutral`, `success`, `error`, `info`, `warning`, `loading`) are styled in a single pass. Originally we discussed a Stage 1 (neutral/success/error only) but chose to cover the full set now since the visual system is uniform and adding the remaining three types is a pure color-mapping change.

## Non-goals

- No wrapper composable (`useToast`, `showToast`, etc.) — the `vue-sonner` `toast()` API is fine as-is.
- No custom toast content (rich JSX/slots, actions, promise helpers) — out of scope for this spec; revisit if an actual need appears.
- No new toast types beyond Sonner's built-in six.
- No change to how Sonner is mounted. `app.vue` continues to render `<Sonner />` once without props.
- No mobile-specific tweaks. The design is identical on desktop and mobile.

## Visual system

One consistent toast shape. Only the left-border accent color and the icon color change per type.

- Background: `#0a0a0a` (slightly lighter than `--dt-panel-bg: #060606`, so toasts visually separate from the shell background in bottom-right corner).
- Border: `1px solid #1a1a1a`.
- Left accent: `2px solid` in the type color (table below). The 1px outer border is *replaced* on the left edge by the 2px accent (so the total left inset is 2px, not 3px — keeps padding consistent).
- Border radius: `6px`.
- Padding: `10px 14px 10px 12px` (top/right/bottom/left — slightly tighter left to compensate for the 2px accent).
- Min-width: ~240px. Max-width follows Sonner default.
- Icon: Lucide, `14px × 14px`, positioned left, `flex-shrink: 0`. Color matches the left-accent color. The existing icon slot overrides in `Sonner.vue` already set the correct Lucide icons — reused as-is.
- Text: `DM Mono`, `12px`, color `#c8c8c0`, `letter-spacing: 0.2px`.
- Close button (`×`): `12px`, color `var(--dt-text-quaternary)`. `opacity: 0` by default, `opacity: 1` on toast hover with a `120ms` transition.

### Type → color mapping

| Type      | Accent (border-left + icon) | Token                         |
|-----------|-----------------------------|-------------------------------|
| neutral   | `#2a2a2a`                   | hardcoded (no existing token) |
| success   | `#22c55e`                   | `var(--kind-example)`         |
| error     | `#f87171`                   | `var(--destructive)`          |
| info      | `#3b82f6`                   | `var(--kind-summary)`         |
| warning   | `#f97316`                   | `var(--kind-fact)`            |
| loading   | border stays `#2a2a2a`; icon `#7a7f86` spinning | `var(--dt-text-tertiary)` |

Rationale: neutral and loading deliberately do *not* use a strong accent — loading is a transient "in progress" state, not something that needs peripheral attention. Adding `#2a2a2a` as a hardcoded value (rather than a new token) is intentional: it's a one-off visual that isn't reused elsewhere in the system; introducing a token for a single use-site adds noise.

**Note on token reuse:** `--kind-summary` / `--kind-example` / `--kind-fact` were originally defined as *content-classification* colors (kind badges on posts). We reuse them here as *state* colors for toasts because the hues already match what the semantics call for (blue-info, green-success, orange-warning) and introducing a parallel state palette would duplicate values for no gain. If the design system later needs to diverge (e.g. content kinds get new hues while state colors stay), this spot in `Sonner.vue` is the single point of update.

**Current callers (as of spec date):** One call site — `apps/web/app/components/desktop/FocusCard.vue:48` uses `toast('скопійовано')` (neutral). No callers of `.success` / `.error` / `.info` / `.warning` / `.loading` yet. Blast radius for this change is minimal.

## Behavior

- Position: `bottom-right`. Explicitly set on `<Sonner>` even though this is the Sonner default — makes the intent visible in the component.
- Duration: Sonner default (4000ms).
- Animation: Sonner default (slide from edge).
- Stacking: Sonner default (newest on top).
- Offset from edges: Sonner default (~16px).
- Close on hover: the `×` appears on hover as described above; clicking it dismisses the toast via Sonner's built-in close handler.
- Known interaction: when `CommentsPanel` or `NotesPanel` is open on the right side of the feed, a toast shown in bottom-right will appear in front of the panel (Sonner's teleport mounts outside the panel). This is acceptable — toasts are short-lived and rarely overlap long enough to matter. Not worth engineering around.

## Architecture

### Files touched

Only one file is modified:

- `apps/web/app/components/ui/sonner/Sonner.vue`

No new files. No changes to `app.vue`, no changes to any caller (e.g. `FocusCard.vue:48` `toast('скопійовано')` — works unchanged, just looks different).

### Changes inside `Sonner.vue`

1. **CSS variables on `<Sonner>`** — extend the existing `:style` object to cover all six variants. `vue-sonner` reads CSS custom properties with the naming convention `--<type>-bg`, `--<type>-text`, `--<type>-border` for each of `normal | success | error | info | warning | loading`. We set these to drive the shared background (`#0a0a0a`), text color (`#c8c8c0`), and outer border color (`#1a1a1a`) across all types.

   **Verification step during implementation:** Before relying on these variable names, open `node_modules/vue-sonner/dist/*.css` (or the package's source) and confirm the exact custom-property names the current installed version uses. If they differ (e.g. `--sonner-success-bg` instead of `--success-bg`), adjust accordingly. The design does not depend on one naming scheme over another — only on the fact that vue-sonner exposes *some* per-type CSS variables for bg/text/border.

2. **`<style>` block (not scoped)** — Sonner uses `<Teleport>` to mount toasts on `document.body`, so scoped styles wouldn't reach them. Selectors target Sonner's stable data attributes:

   ```css
   [data-sonner-toast] {
     font-family: var(--font-mono);
     font-size: 12px;
     letter-spacing: 0.2px;
     padding: 10px 14px 10px 12px;
     border-left-width: 2px;
     border-left-style: solid;
     border-left-color: #2a2a2a; /* neutral default */
     border-radius: 6px;
   }
   [data-sonner-toast][data-type='success'] { border-left-color: #22c55e; }
   [data-sonner-toast][data-type='success'] svg { color: #22c55e; }
   [data-sonner-toast][data-type='error']   { border-left-color: #f87171; }
   [data-sonner-toast][data-type='error']   svg { color: #f87171; }
   [data-sonner-toast][data-type='info']    { border-left-color: #3b82f6; }
   [data-sonner-toast][data-type='info']    svg { color: #3b82f6; }
   [data-sonner-toast][data-type='warning'] { border-left-color: #f97316; }
   [data-sonner-toast][data-type='warning'] svg { color: #f97316; }
   [data-sonner-toast][data-type='loading'] svg { color: #7a7f86; }

   [data-sonner-toast] [data-close-button] {
     opacity: 0;
     transition: opacity 120ms ease-out;
     color: var(--dt-text-quaternary);
   }
   [data-sonner-toast]:hover [data-close-button] { opacity: 1; }
   ```

   **Verification step during implementation:** Inspect a live Sonner toast in the browser devtools before writing the selectors to confirm `[data-sonner-toast]`, `[data-type]`, and `[data-close-button]` are the actual attributes used by the installed version. The design tolerates different attribute names — the implementer just maps selectors to whatever the current version exposes.

3. **`position="bottom-right"`** — add this prop explicitly on `<Sonner>`.

4. **Preserved as-is:**
   - The `:class="cn('toaster group', props.class)"` binding on `<Sonner>` stays — external callers can still pass a `class` prop if needed (none do today, but no reason to remove the extension point).
   - The `--border-radius` CSS variable set in the current `:style` object (`'--border-radius': 'var(--radius)'`) is removed, since the new `<style>` block applies `border-radius: 6px` directly on `[data-sonner-toast]`. Keeping both would be redundant and confusing.

5. **Icon slots** — no change. The existing `#success-icon`, `#info-icon`, `#warning-icon`, `#error-icon`, `#loading-icon`, `#close-icon` overrides already point at the correct Lucide icons. Neutral toasts have no icon by default in Sonner — we leave that as-is (a neutral `toast('скопійовано')` shows text only, no icon). If later we want an icon on neutral toasts, that's a separate change.

### Why no wrapper composable

The current call site is `toast('скопійовано')`. Wrapping the imported `toast` in a custom helper would add indirection without benefit — every abstraction we'd add (preset durations, preset titles, icon overrides) is either already solved by Sonner's native API or not actually needed yet. YAGNI. When a real pattern emerges (e.g. every save operation needs to pair loading → success/error), we can introduce a helper then.

## Testing

Manual only (no automated test pattern in this workspace for UI components).

Implementation includes a temporary dev-only test harness: add a row of 6 buttons somewhere reachable (e.g. the top of `FeedPage` behind an `import.meta.dev` guard) that fire each variant:

```vue
<div v-if="import.meta.dev" style="display:flex;gap:8px;padding:8px">
  <button @click="toast('скопійовано')">neutral</button>
  <button @click="toast.success('пост збережено')">success</button>
  <button @click="toast.error('не вдалось зберегти')">error</button>
  <button @click="toast.info('доступна нова версія')">info</button>
  <button @click="toast.warning('повторити запит?')">warning</button>
  <button @click="toast.loading('збереження нотатки…')">loading</button>
</div>
```

Acceptance checklist:

1. All six variants appear in bottom-right with the correct left-border accent color and icon color.
2. Hovering a toast fades in the `×` button; clicking `×` dismisses that toast.
3. Stacking multiple toasts: newest on top, older ones push down; all share the same width and vertical spacing.
4. Existing `toast('скопійовано')` in `FocusCard` on copy action looks correct (neutral variant, mono font, dark panel) without any code change at the call site.
5. Loading toast icon spins (the existing `animate-spin` on `Loader2Icon` in the slot override already does this).
6. On a narrow viewport (e.g. mobile-width browser window), toasts stay within the viewport and remain readable. (No mobile-specific styles — we're verifying the desktop design doesn't break.)

After acceptance passes, **remove the dev-only harness** before merge.

## Edge cases

- **`vue-sonner` version upgrade changes data-attribute names.** The `<style>` block breaks silently (styles stop applying, toasts fall back to raw Sonner defaults). Detectable by the acceptance checklist. Mitigation: localized to one file, easy to re-inspect and update selectors.
- **`vue-sonner` CSS variable names differ from the assumption.** Same class of problem — caught by the same checklist. Implementer verifies names during the implementation step.
- **Toast overlaps a right-side panel (`CommentsPanel` / `NotesPanel`).** Documented above under Behavior. Not mitigated; not a bug.
- **Very long toast text.** Sonner's default max-width applies. If a specific message needs truncation, that's a per-call concern (callers pass shorter strings), not a style concern.

## Out-of-scope follow-ups (future specs, if needed)

- Toast with action button (e.g. "Undo save") — Sonner supports this natively via `toast('msg', { action: {...} })`; styling the action button would be a small addition here, but no current call site needs it.
- Promise-style toasts (`toast.promise(...)`) — Sonner built-in; will need verification that loading → success/error transitions animate smoothly with our styles.
- Swipe-to-dismiss on touch — Sonner default works but hasn't been verified with our styles on mobile.
- Icon for neutral toasts — currently no icon; may revisit if neutral toasts look too bare.
