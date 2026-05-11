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

import SavedCard from './_components/SavedCard.vue'
import { useSavedRouteState } from './_composables/useSavedRouteState'
import { kindFilters, sortOptions, viewOptions } from './_constants'

import type { SavedTopicGroup } from './_types'

const {
  savedPosts,
  clear: clearSavedPosts,
  fetchNextPage,
  hasNextPage,
  isClearing,
  isFetchingNextPage,
  isLoading,
} = useSavedPosts()

const isClearDialogOpen = ref(false)
const savedRootRef = useTemplateRef('savedRootRef')
const {
  searchQuery,
  normalizedSearchQuery,
  selectedKind,
  selectedSort,
  selectedView,
  resetFilters,
} = useSavedRouteState()

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
    return posts.sort((a, b) => Date.parse(a.savedAt) - Date.parse(b.savedAt))
  }

  if (selectedSort.value === 'topic') {
    return posts.sort((a, b) =>
      `${a.topicTitle} ${a.title}`.localeCompare(`${b.topicTitle} ${b.title}`)
    )
  }

  return posts.sort((a, b) => Date.parse(b.savedAt) - Date.parse(a.savedAt))
})

const selectedSortLabel = computed(
  () => sortOptions.find((option) => option.value === selectedSort.value)?.label
)

const formatSavedDate = (value: string) => {
  const date = new Date(value)

  if (Number.isNaN(date.getTime())) return null

  return new Intl.DateTimeFormat('en-GB', {
    day: 'numeric',
    month: 'short',
  }).format(date)
}

const groupedSavedPosts = computed(() => {
  const groups = new Map<string, SavedTopicGroup>()

  for (const post of sortedSavedPosts.value) {
    const group = groups.get(post.topicSlug)

    if (group) {
      group.posts.push(post)

      if (Date.parse(post.savedAt) > Date.parse(group.lastSavedAt)) {
        group.lastSavedAt = post.savedAt
      }

      continue
    }

    groups.set(post.topicSlug, {
      topicTitle: post.topicTitle,
      topicSlug: post.topicSlug,
      lastSavedAt: post.savedAt,
      posts: [post],
    })
  }

  return [...groups.values()]
})

const hasActiveFilters = computed(
  () => !!normalizedSearchQuery.value || selectedKind.value !== 'all'
)

const savedPostsCount = computed(() =>
  hasActiveFilters.value
    ? `${filteredSavedPosts.value.length} / ${savedPosts.value.length}${hasNextPage.value ? '+' : ''} posts`
    : `${savedPosts.value.length}${hasNextPage.value ? '+' : ''} posts`
)

const onClearSavedPosts = async () => {
  try {
    await clearSavedPosts()
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

const canFetchNextPage = computed(() => hasNextPage.value && !isFetchingNextPage.value)

useInfiniteScroll(
  savedRootRef,
  async () => {
    await fetchNextPage()
  },
  {
    distance: 220,
    canLoadMore: () => canFetchNextPage.value,
  }
)
</script>

<template>
  <section
    ref="savedRootRef"
    class="saved"
  >
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
              <div class="clear-dialog-icon-wrap">
                <Icon
                  name="lucide:trash-2"
                  class="clear-dialog-icon"
                />
              </div>
              <div class="clear-dialog-copy">
                <div class="clear-dialog-kicker">// confirmation</div>
                <AlertDialogTitle class="clear-dialog-title">
                  Clear all saved posts?
                </AlertDialogTitle>
                <AlertDialogDescription class="clear-dialog-description">
                  This removes {{ savedPosts.length }} saved posts from your library. This cannot be
                  undone.
                </AlertDialogDescription>
              </div>
            </div>

            <div class="clear-dialog-footer">
              <AlertDialogCancel
                class="clear-cancel"
                :disabled="isClearing"
              >
                Cancel
              </AlertDialogCancel>
              <AlertDialogAction
                class="clear-action"
                :disabled="isClearing"
                @click.prevent="onClearSavedPosts"
              >
                <Icon
                  v-if="isClearing"
                  name="lucide:loader"
                  class="clear-spinner"
                />
                <Icon
                  v-else
                  name="lucide:trash-2"
                  class="clear-action-icon"
                />
                {{ isClearing ? 'Clearing...' : 'Clear saved' }}
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
          <Icon
            v-if="normalizedSearchQuery"
            name="lucide:x"
            class="empty-link"
            @click="resetFilters"
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

        <div
          class="view-switcher"
          aria-label="Режим відображення збережених постів"
        >
          <button
            v-for="option in viewOptions"
            :key="option.value"
            type="button"
            class="view-switcher-button"
            :class="{ 'is-active': selectedView === option.value }"
            :aria-label="option.label"
            :aria-pressed="selectedView === option.value"
            @click="selectedView = option.value"
          >
            <Icon
              :name="option.icon"
              class="view-switcher-icon"
            />
          </button>
        </div>
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
      <div>{{ isLoading ? '// завантаження...' : '// тут порожньо' }}</div>
      <NuxtLink
        v-if="!isLoading"
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
        @click="resetFilters"
      >
        // скинути пошук
      </button>
    </div>
    <div
      v-else-if="selectedView === 'grid'"
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
            <SavedCard
              :post="post"
              :search-query="normalizedSearchQuery"
              view="grid"
            />
          </motion.div>
        </AnimatePresence>
      </LayoutGroup>
    </div>
    <div
      v-else
      class="grouped"
    >
      <section
        v-for="group in groupedSavedPosts"
        :key="group.topicSlug"
        class="topic-group"
      >
        <header class="topic-group-header">
          <div class="topic-group-copy">
            <div class="topic-group-title">{{ group.topicTitle }}</div>
            <div class="topic-group-meta">// {{ group.topicSlug }}</div>
          </div>
          <div class="topic-group-stats">
            <span class="topic-group-count">{{ group.posts.length }} posts</span>
            <span
              v-if="formatSavedDate(group.lastSavedAt)"
              class="topic-group-last"
            >
              last saved {{ formatSavedDate(group.lastSavedAt) }}
            </span>
          </div>
        </header>

        <div class="topic-group-grid">
          <LayoutGroup>
            <AnimatePresence mode="popLayout">
              <motion.div
                v-for="post in group.posts"
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
                <SavedCard
                  :post="post"
                  :search-query="normalizedSearchQuery"
                  view="grouped"
                />
              </motion.div>
            </AnimatePresence>
          </LayoutGroup>
        </div>
      </section>
    </div>

    <div
      v-if="savedPosts.length && hasNextPage"
      class="load-more"
    >
      <Icon
        v-if="isFetchingNextPage"
        name="lucide:loader"
        class="load-more-spinner"
      />
      <span>{{ isFetchingNextPage ? 'завантаження...' : 'прокрути нижче' }}</span>
    </div>
  </section>
</template>

<style scoped>
.saved {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  scrollbar-gutter: stable;
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
  cursor: pointer;
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
  max-width: 460px;
  gap: 20px;
  padding: 20px;
  transform: translate(-50%, -50%);
  border: 1px solid #1c1c1c;
  border-radius: 6px;
  background: #0b0b0b;
  color: #999;
  box-shadow:
    0 18px 48px rgba(0, 0, 0, 0.58),
    0 0 0 1px rgba(255, 255, 255, 0.018) inset;
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
  display: flex;
  gap: 14px;
  text-align: left;
}
.clear-dialog-icon-wrap {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 34px;
  height: 34px;
  flex-shrink: 0;
  border: 1px solid rgba(216, 136, 136, 0.2);
  border-radius: 5px;
  background: rgba(216, 136, 136, 0.065);
  color: #d88;
}
.clear-dialog-icon {
  width: 16px;
  height: 16px;
}
.clear-dialog-copy {
  display: grid;
  min-width: 0;
  gap: 8px;
}
.clear-dialog-kicker {
  font-family: var(--font-mono);
  font-size: 10px;
  line-height: 1.2;
  color: #6f756d;
  letter-spacing: 0.06em;
}
.clear-dialog-title {
  font-family: var(--font-display);
  font-size: 18px;
  line-height: 1.3;
  color: #d2d2c8;
  font-weight: 400;
}
.clear-dialog-description {
  font-family: var(--font-mono);
  font-size: 12px;
  line-height: 1.6;
  color: #6f756d;
}
.clear-dialog-footer {
  display: flex;
  gap: 10px;
  padding-top: 2px;
}
.clear-cancel {
  flex: 1;
  height: 40px;
  border: 1px solid #222;
  border-radius: 4px;
  background: transparent;
  color: #777;
  cursor: pointer;
  font-family: var(--font-mono);
  font-size: 12px;
}
.clear-cancel:hover {
  border-color: #262626;
  background: #111;
  color: #aaa;
}
.clear-action {
  flex: 1;
  height: 40px;
  gap: 7px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 0;
  border: 1px solid rgba(224, 122, 122, 0.28);
  border-radius: 4px;
  background: #2a0b0b;
  color: #e07a7a;
  cursor: pointer;
  font-family: var(--font-mono);
  font-size: 12px;
}
.clear-action:hover {
  border-color: rgba(224, 122, 122, 0.4);
  background: #331010;
  color: #ef9a9a;
}
.clear-spinner,
.clear-action-icon {
  width: 15px;
  height: 15px;
}
.clear-spinner {
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
  flex-wrap: wrap;
}
.search-box {
  display: flex;
  align-items: center;
  flex: 1;
  min-width: 220px;
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
.view-switcher {
  display: inline-flex;
  align-items: center;
  height: 38px;
  padding: 3px;
  border: 1px solid #141414;
  border-radius: 5px;
  background: #080808;
  flex-shrink: 0;
}
.view-switcher-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 30px;
  height: 30px;
  border: 1px solid transparent;
  border-radius: 3px;
  background: transparent;
  color: #3d3d3d;
  cursor: pointer;
  transition:
    border-color 0.15s,
    color 0.15s,
    background 0.15s;
}
.view-switcher-button:hover {
  color: #777;
  background: #101010;
}
.view-switcher-button.is-active {
  border-color: rgba(126, 183, 124, 0.28);
  background: rgba(126, 183, 124, 0.08);
  color: #93bd8f;
}
.view-switcher-icon {
  width: 13px;
  height: 13px;
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
  grid-template-columns: repeat(4, minmax(220px, 1fr));
  gap: 12px;
}
.grouped {
  display: grid;
  gap: 22px;
}
.topic-group {
  display: grid;
  gap: 10px;
  padding-top: 2px;
}
.topic-group + .topic-group {
  padding-top: 18px;
  border-top: 1px solid #191919;
}
.topic-group-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  min-width: 0;
  padding: 0 1px;
}
.topic-group-copy {
  display: grid;
  gap: 5px;
  min-width: 0;
}
.topic-group-title {
  overflow: hidden;
  color: #d2d2c8;
  font-family: var(--font-display);
  font-size: 15px;
  line-height: 1.25;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.topic-group-meta {
  overflow: hidden;
  color: var(--dt-text-quaternary);
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.3;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.topic-group-stats {
  display: flex;
  align-items: center;
  flex-shrink: 0;
  gap: 7px;
  padding-top: 1px;
}
.topic-group-count,
.topic-group-last {
  color: var(--dt-text-quaternary);
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.3;
  white-space: nowrap;
}
.topic-group-count {
  padding: 3px 7px;
  border: 1px solid #1d241f;
  border-radius: 999px;
  background: rgba(126, 183, 124, 0.05);
  color: #7fa17b;
}
.topic-group-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(220px, 1fr));
  gap: 10px;
}
.topic-group-grid :deep(.card) {
  min-height: 112px;
}
.topic-group-grid :deep(.title) {
  -webkit-line-clamp: 2;
  line-clamp: 2;
}
.load-more {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  padding: 18px 0 4px;
  flex-shrink: 0;
  color: var(--dt-text-quaternary);
  font-family: var(--font-mono);
  font-size: 10px;
}
.load-more-spinner {
  width: 13px;
  height: 13px;
  color: var(--kind-example);
  animation: spin 0.8s linear infinite;
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
@media (max-width: 1180px) {
  .grid {
    grid-template-columns: repeat(3, minmax(210px, 1fr));
  }
  .topic-group-grid {
    grid-template-columns: repeat(2, minmax(210px, 1fr));
  }
}
@media (max-width: 760px) {
  .saved {
    padding: 16px;
  }
  .grid,
  .topic-group-grid {
    grid-template-columns: repeat(2, minmax(180px, 1fr));
  }
}
@media (max-width: 520px) {
  .search-box,
  .sort-trigger,
  .view-switcher {
    width: 100%;
    max-width: none;
  }
  .sort-trigger {
    justify-content: space-between;
  }
  .view-switcher-button {
    flex: 1;
  }
  .grid,
  .topic-group-grid {
    grid-template-columns: 1fr;
  }
  .topic-group-header {
    align-items: flex-start;
    flex-direction: column;
    gap: 5px;
  }
}
</style>
