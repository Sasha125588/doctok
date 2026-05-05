<script setup lang="ts">
import { AnimatePresence, LayoutGroup, motion } from 'motion-v'
import {
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogOverlay,
  AlertDialogPortal,
  AlertDialogRoot,
  AlertDialogTitle,
  AlertDialogTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuPortal,
  DropdownMenuRoot,
  DropdownMenuTrigger,
} from 'reka-ui'
import { toast } from 'vue-sonner'

import SavedCard from './SavedCard.vue'

type SavedKindFilter = 'all' | 'summary' | 'example' | 'concept' | 'tip'
type SavedSortMode = 'newest' | 'oldest' | 'topic'

const kindFilters: Array<{ value: SavedKindFilter; label: string }> = [
  { value: 'all', label: 'all' },
  { value: 'summary', label: 'summary' },
  { value: 'example', label: 'example' },
  { value: 'concept', label: 'concept' },
  { value: 'tip', label: 'tip' },
]

const sortOptions: Array<{ value: SavedSortMode; label: string }> = [
  { value: 'newest', label: '↑ новіші спочатку' },
  { value: 'oldest', label: '↓ старіші спочатку' },
  { value: 'topic', label: 'A-Z за темою' },
]

const { savedPosts, clear, isClearing } = useSavedPosts()

const isClearDialogOpen = ref(false)
const searchQuery = ref('')
const selectedKind = ref<SavedKindFilter>('all')
const selectedSort = ref<SavedSortMode>('newest')

const normalizedSearchQuery = computed(() => searchQuery.value.trim().toLowerCase())

const resetSearchQuery = () => {
  searchQuery.value = ''
  selectedKind.value = 'all'
}

const filteredSavedPosts = computed(() => {
  const query = normalizedSearchQuery.value

  return savedPosts.value.filter((post) => {
    const matchesKind = selectedKind.value === 'all' || post.kind === selectedKind.value
    const searchable = `${post.title} ${post.topicSlug} ${post.topicTitle}`.toLowerCase()
    const matchesSearch = !query || searchable.includes(query)

    return matchesKind && matchesSearch
  })
})

const sortedSavedPosts = computed(() => {
  const posts = [...filteredSavedPosts.value]

  if (selectedSort.value === 'oldest') {
    return posts.sort((a, b) => Date.parse(String(a.savedAt)) - Date.parse(String(b.savedAt)))
  }

  if (selectedSort.value === 'topic') {
    return posts.sort((a, b) =>
      `${a.topicTitle} ${a.title}`.localeCompare(`${b.topicTitle} ${b.title}`)
    )
  }

  return posts.sort((a, b) => Date.parse(String(b.savedAt)) - Date.parse(String(a.savedAt)))
})

const selectedSortLabel = computed(
  () => sortOptions.find((option) => option.value === selectedSort.value)?.label ?? 'сортування'
)

const hasActiveFilters = computed(
  () => !!normalizedSearchQuery.value || selectedKind.value !== 'all'
)

const savedPostsCount = computed(() =>
  hasActiveFilters.value
    ? `${filteredSavedPosts.value.length} / ${savedPosts.value.length} posts`
    : `${savedPosts.value.length} posts`
)

const onClearSavedPosts = async () => {
  try {
    await clear()
    closeClearDialog()
    toast.success('збережене очищено')
  } catch {
    toast.error('не вдалося очистити збережене')
  }
}

const closeClearDialog = () => {
  if (isClearing.value) return

  isClearDialogOpen.value = false
}
</script>

<template>
  <section class="saved">
    <header class="header">
      <div class="header-copy">
        <div class="title">// saved</div>
        <div class="count">{{ savedPostsCount }}</div>
      </div>

      <AlertDialogRoot
        v-if="savedPosts.length"
        v-model:open="isClearDialogOpen"
      >
        <AlertDialogTrigger as-child>
          <button
            class="clear-trigger"
            type="button"
            :disabled="isClearing"
            aria-label="Очистити всі збережені пости"
          >
            <Icon
              name="lucide:trash-2"
              class="clear-trigger-icon"
            />
          </button>
        </AlertDialogTrigger>

        <AlertDialogPortal>
          <AlertDialogOverlay class="clear-dialog-overlay" />
          <AlertDialogContent
            class="clear-dialog"
            @interact-outside="closeClearDialog"
          >
            <div class="clear-dialog-header">
              <div class="clear-dialog-kicker">// підтвердження</div>
              <AlertDialogTitle class="clear-dialog-title">
                Видалити всі {{ savedPosts.length }} збережених постів?
              </AlertDialogTitle>
              <AlertDialogDescription class="clear-dialog-description">
                Це не можна скасувати.
              </AlertDialogDescription>
            </div>

            <div class="clear-dialog-footer">
              <AlertDialogCancel
                class="clear-cancel"
                :disabled="isClearing"
              >
                Скасувати
              </AlertDialogCancel>
              <AlertDialogAction
                class="clear-action"
                :disabled="isClearing"
                @click.prevent="onClearSavedPosts"
              >
                <Icon
                  v-if="isClearing"
                  name="lucide:loader-2"
                  class="clear-spinner"
                />
                {{ isClearing ? 'видалення...' : '✕ видалити' }}
              </AlertDialogAction>
            </div>
          </AlertDialogContent>
        </AlertDialogPortal>
      </AlertDialogRoot>
    </header>

    <div
      v-if="savedPosts.length"
      class="controls"
    >
      <div class="search-row">
        <label class="search-box">
          <span class="sr-only">Пошук у збережених</span>
          <Icon
            name="lucide:search"
            class="search-icon"
          />
          <input
            v-model="searchQuery"
            class="search-input"
            type="search"
            placeholder="Пошук у збережених..."
            spellcheck="false"
          />
        </label>

        <DropdownMenuRoot>
          <DropdownMenuTrigger as-child>
            <button
              type="button"
              class="sort-trigger"
              aria-label="Сортування збережених постів"
            >
              <span class="sort-trigger-label">{{ selectedSortLabel }}</span>
              <Icon
                name="lucide:chevron-down"
                class="sort-trigger-icon"
              />
            </button>
          </DropdownMenuTrigger>

          <DropdownMenuPortal>
            <DropdownMenuContent
              class="sort-menu"
              align="end"
              :side-offset="5"
            >
              <DropdownMenuItem
                v-for="option in sortOptions"
                :key="option.value"
                class="sort-item"
                :class="{ 'is-active': selectedSort === option.value }"
                @click="selectedSort = option.value"
              >
                <span>{{ option.label }}</span>
                <Icon
                  v-if="selectedSort === option.value"
                  name="lucide:check"
                  class="sort-check"
                />
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenuPortal>
        </DropdownMenuRoot>
      </div>

      <div
        class="filters"
        aria-label="Фільтр збережених постів"
      >
        <button
          v-for="filter in kindFilters"
          :key="filter.value"
          type="button"
          class="filter-chip"
          :class="{ 'is-active': selectedKind === filter.value }"
          :aria-pressed="selectedKind === filter.value"
          @click="selectedKind = filter.value"
        >
          {{ filter.label }}
        </button>
      </div>
    </div>

    <div
      v-if="!savedPosts.length"
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
      v-else-if="!filteredSavedPosts.length"
      class="empty"
    >
      <div>// нічого не знайдено</div>
      <button
        type="button"
        class="empty-link"
        @click="resetSearchQuery"
      >
        // скинути пошук
      </button>
    </div>
    <div
      v-else
      class="grid"
    >
      <LayoutGroup>
        <AnimatePresence mode="popLayout">
          <motion.div
            v-for="post in sortedSavedPosts"
            :key="post.postId"
            :layout="true"
            :initial="{ opacity: 0, y: 8, scale: 0.985 }"
            :animate="{ opacity: 1, y: 0, scale: 1 }"
            :exit="{ opacity: 0, y: -6, scale: 0.985 }"
            :transition="{
              opacity: { duration: 0.14 },
              y: { duration: 0.18 },
              scale: { duration: 0.18 },
              layout: { duration: 0.22 },
            }"
          >
            <SavedCard :post="post" />
          </motion.div>
        </AnimatePresence>
      </LayoutGroup>
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
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  flex-shrink: 0;
}
.header-copy {
  display: flex;
  align-items: baseline;
  gap: 12px;
  min-width: 0;
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
.clear-trigger {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  margin-left: auto;
  color: #a47272;
  opacity: 0.72;
  border: 1px solid transparent;
  border-radius: 5px;
  background: transparent;
}
.clear-trigger:hover,
.clear-trigger[data-state='open'] {
  color: #d88;
  opacity: 1;
  border-color: rgba(216, 136, 136, 0.16);
  background: rgba(216, 136, 136, 0.06);
}
.clear-trigger-icon {
  width: 13px;
  height: 13px;
}
.clear-dialog-overlay {
  position: fixed;
  inset: 0;
  z-index: 50;
  background: rgba(0, 0, 0, 0.72);
}
.clear-dialog-overlay[data-state='open'] {
  animation: clearOverlayIn 0.18s ease-out;
}
.clear-dialog-overlay[data-state='closed'] {
  animation: clearOverlayOut 0.14s ease-in;
}
.clear-dialog {
  position: fixed;
  top: 50%;
  left: 50%;
  z-index: 51;
  display: grid;
  width: calc(100vw - 32px);
  max-width: 390px;
  gap: 16px;
  padding: 18px;
  transform: translate(-50%, -50%);
  border: 1px solid #1a1a1a;
  border-radius: 5px;
  background: #0d0d0d;
  color: #999;
  box-shadow:
    0 18px 48px rgba(0, 0, 0, 0.58),
    0 0 0 1px rgba(255, 255, 255, 0.012) inset;
  transform-origin: center;
  will-change: opacity, transform;
}
.clear-dialog[data-state='open'] {
  animation: clearDialogIn 0.24s cubic-bezier(0.16, 1, 0.3, 1);
}
.clear-dialog[data-state='closed'] {
  animation: clearDialogOut 0.16s cubic-bezier(0.4, 0, 1, 1);
}
.clear-dialog[data-state='open'] .clear-dialog-header {
  animation: clearDialogContentIn 0.28s cubic-bezier(0.16, 1, 0.3, 1) both;
}
.clear-dialog[data-state='open'] .clear-dialog-footer {
  animation: clearDialogContentIn 0.28s 0.035s cubic-bezier(0.16, 1, 0.3, 1) both;
}
.clear-dialog-header {
  display: grid;
  gap: 7px;
  text-align: left;
}
.clear-dialog-kicker {
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.2;
  color: #553333;
  letter-spacing: 0.06em;
}
.clear-dialog-title {
  font-family: var(--font-mono);
  font-size: 13px;
  line-height: 1.45;
  color: #8d7777;
  font-weight: 400;
}
.clear-dialog-description {
  font-family: var(--font-mono);
  font-size: 11px;
  line-height: 1.4;
  color: #442222;
}
.clear-dialog-footer {
  display: flex;
  gap: 10px;
  padding-top: 3px;
}
.clear-cancel {
  flex: 1;
  height: 34px;
  border: 1px solid #1a1a1a;
  border-color: #1a1a1a;
  border-radius: 3px;
  background: transparent;
  color: #444;
  cursor: pointer;
  font-family: var(--font-mono);
  font-size: 11px;
}
.clear-cancel:hover {
  background: #111;
  color: #777;
}
.clear-action {
  flex: 1;
  height: 34px;
  gap: 7px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 0;
  border: 1px solid #cc333333;
  border-radius: 3px;
  background: #1a0606;
  color: #cc3333;
  cursor: pointer;
  font-family: var(--font-mono);
  font-size: 11px;
}
.clear-action:hover {
  background: #230808;
  color: #e04a4a;
}
.clear-spinner {
  width: 14px;
  height: 14px;
  animation: spin 0.8s linear infinite;
}
.controls {
  display: flex;
  flex-direction: column;
  gap: 9px;
  margin-bottom: 16px;
  flex-shrink: 0;
}
.search-row {
  display: flex;
  align-items: center;
  gap: 8px;
}
.search-box {
  display: flex;
  align-items: center;
  flex: 1;
  gap: 10px;
  height: 38px;
  padding: 0 12px;
  border: 1px solid #141414;
  border-radius: 6px;
  background: #080808;
  transition:
    border-color 0.15s,
    background 0.15s;
}
.search-box:focus-within {
  border-color: #1f3224;
  background: #070707;
}
.search-icon {
  width: 13px;
  height: 13px;
  color: #333;
  flex-shrink: 0;
}
.search-input {
  width: 100%;
  min-width: 0;
  border: none;
  outline: none;
  background: transparent;
  color: #c8c8c0;
  caret-color: var(--kind-example);
  font-family: var(--font-mono);
  font-size: 12px;
}
.search-input::placeholder {
  color: #2c2c2c;
}
.sort-trigger {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  height: 38px;
  max-width: 180px;
  min-width: 168px;
  padding: 0 10px;
  border: 1px solid #141414;
  border-radius: 4px;
  background: #0c0c0c;
  color: #666;
  cursor: pointer;
  font-family: var(--font-mono);
  font-size: 9px;
  letter-spacing: 0.04em;
  transition:
    border-color 0.15s,
    color 0.15s,
    background 0.15s;
}
.sort-trigger:hover,
.sort-trigger[data-state='open'] {
  border-color: #1b1b1b;
  background: #0a0a0a;
  color: #8e8e8e;
}
.sort-trigger-label {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.sort-trigger-icon {
  width: 10px;
  height: 10px;
  flex-shrink: 0;
}
:global(.sort-menu) {
  z-index: 50;
  min-width: 168px;
  padding: 4px 0;
  border: 1px solid #1a1a1a;
  border-radius: 4px;
  background: #0d0d0d;
  box-shadow:
    0 18px 42px rgba(0, 0, 0, 0.55),
    0 0 0 1px rgba(255, 255, 255, 0.025) inset;
}
:global(.sort-item) {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  min-height: 28px;
  padding: 0 10px;
  color: #444;
  cursor: pointer;
  font-family: var(--font-mono);
  font-size: 9px;
  outline: none;
}
:global(.sort-item:hover),
:global(.sort-item:focus) {
  background: #171717;
  color: #888;
}
:global(.sort-item.is-active) {
  color: var(--kind-example);
}
:global(.sort-check) {
  width: 11px;
  height: 11px;
  flex-shrink: 0;
}
.filters {
  display: flex;
  align-items: center;
  gap: 6px;
  overflow-x: auto;
  padding-bottom: 1px;
}
.filter-chip {
  height: 24px;
  padding: 0 9px;
  border: 1px solid #141414;
  border-radius: 999px;
  background: #080808;
  color: var(--dt-text-quaternary);
  cursor: pointer;
  font-family: var(--font-mono);
  font-size: 9px;
  transition:
    border-color 0.15s,
    color 0.15s,
    background 0.15s;
}
.filter-chip:hover {
  border-color: #1d1d1d;
  color: var(--dt-text-tertiary);
}
.filter-chip.is-active {
  border-color: rgba(126, 183, 124, 0.32);
  background: rgba(126, 183, 124, 0.07);
  color: #93bd8f;
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
  border: none;
  background: transparent;
  color: var(--dt-text-quaternary);
  cursor: pointer;
  font: inherit;
  text-decoration: none;
  transition: color 0.15s;
}
.empty-link:hover {
  color: var(--kind-example);
}
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
@keyframes clearOverlayIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
@keyframes clearOverlayOut {
  from {
    opacity: 1;
  }
  to {
    opacity: 0;
  }
}
@keyframes clearDialogIn {
  from {
    opacity: 0;
    transform: translate(-50%, calc(-50% + 8px)) scale(0.965);
  }
  to {
    opacity: 1;
    transform: translate(-50%, -50%) scale(1);
  }
}
@keyframes clearDialogOut {
  from {
    opacity: 1;
    transform: translate(-50%, -50%) scale(1);
  }
  to {
    opacity: 0;
    transform: translate(-50%, calc(-50% + 5px)) scale(0.98);
  }
}
@keyframes clearDialogContentIn {
  from {
    opacity: 0;
    transform: translateY(5px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
@media (prefers-reduced-motion: reduce) {
  .clear-dialog-overlay,
  .clear-dialog,
  .clear-dialog-header,
  .clear-dialog-footer {
    animation: none !important;
  }
}
</style>
