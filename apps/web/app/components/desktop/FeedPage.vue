<script setup lang="ts">
import { motion } from 'motion-v'
import { storeToRefs } from 'pinia'

import BrowseMode from './BrowseMode.vue'
import FocusMode from './FocusMode.vue'
import Sidebar from './Sidebar.vue'
import VariantSelector from '~/components/desktop/VariantSelector.vue'
import { useFeed } from '~/composables/useFeed'
import { type FeedMode, useFeedRouteState } from '~/composables/useFeedRouteState'
import { useFeedViewStore } from '~/stores/feedView'

const hasMountedCommentsPanel = ref(false)
const hasMountedNotesPanel = ref(false)
const modes: FeedMode[] = ['focus', 'browse']

const feedView = useFeedViewStore()
const { activePanel } = storeToRefs(feedView)

const { topicSlug, postId, mode, setMode, clearPostId, openTopic, openPost } = useFeedRouteState()

const { topics, fetchNextPage, hasNextPage, isFetchingNextPage } = useFeed()

const { posts, isLoading } = useTopicPosts(topicSlug)
const activeTopicIndex = computed(() =>
  topics.value.findIndex((topic) => topic.slug === topicSlug.value)
)
const totalPosts = computed(() => posts.value.length)

const activePostIndex = computed(() => {
  if (!posts.value.length) return 0

  const index = posts.value.findIndex((post) => +post.id === postId.value)
  return index >= 0 ? index : 0
})
const activePost = computed(() => posts.value[activePostIndex.value])

const shouldMountCommentsPanel = computed(
  () => activePanel.value === 'comments' || hasMountedCommentsPanel.value
)
const shouldMountNotesPanel = computed(
  () => activePanel.value === 'notes' || hasMountedNotesPanel.value
)

watch(activePanel, (panel) => {
  if (panel === 'comments') hasMountedCommentsPanel.value = true
  if (panel === 'notes') hasMountedNotesPanel.value = true
})

watch(
  [topicSlug, topics],
  ([currentTopicSlug, currentTopics]) => {
    if (currentTopicSlug) return

    const firstTopic = currentTopics[0]
    if (!firstTopic) return

    openTopic(firstTopic.slug)
  },
  { immediate: true }
)

watch(
  [posts, postId, topicSlug, isLoading],
  ([currentPosts, currentPostId, currentTopicSlug, loading]) => {
    if (loading || !currentTopicSlug) return

    const firstPost = currentPosts[0]
    if (!firstPost) {
      if (currentPostId != null) clearPostId()
      return
    }

    const hasPost = currentPostId != null && currentPosts.some((post) => +post.id === currentPostId)
    if (hasPost) return

    openPost(
      { id: +firstPost.id, topicSlug: firstPost.topicSlug },
      { mode: mode.value, replace: true }
    )
  },
  { immediate: true }
)

const openPostAt = (index: number) => {
  const safeIndex = Math.min(Math.max(0, index), Math.max(0, totalPosts.value - 1))
  const post = posts.value[safeIndex]
  if (!post) return

  openPost({ id: +post.id, topicSlug: post.topicSlug }, { mode: 'focus', replace: true })
}

const openTopicAt = (index: number) => {
  const topic = topics.value[index]
  if (!topic) return false

  openTopic(topic.slug)
  return true
}

const openAdjacentTopic = async (direction: 1 | -1) => {
  const nextIndex = activeTopicIndex.value + direction
  if (openTopicAt(nextIndex)) return
  if (direction === -1 || !hasNextPage.value || isFetchingNextPage.value) return

  const loadedCount = topics.value.length
  await fetchNextPage()

  openTopicAt(loadedCount)
}

const onKeydown = (e: KeyboardEvent) => {
  if (mode.value !== 'focus' || activePanel.value !== null) return

  if (e.key === 'ArrowRight') {
    e.preventDefault()
    openPostAt(activePostIndex.value + 1)
  } else if (e.key === 'ArrowLeft') {
    e.preventDefault()
    openPostAt(activePostIndex.value - 1)
  } else if (e.key === 'ArrowDown') {
    e.preventDefault()
    openAdjacentTopic(1)
  } else if (e.key === 'ArrowUp') {
    e.preventDefault()
    openAdjacentTopic(-1)
  }
}

onMounted(() => window.addEventListener('keydown', onKeydown))
onBeforeUnmount(() => window.removeEventListener('keydown', onKeydown))
</script>

<template>
  <div class="feed-page">
    <Sidebar :topics />
    <div class="stack">
      <div class="feed-toolbar">
        <div class="toggle">
          <button
            v-for="m in modes"
            :key="m"
            class="toggle-btn"
            :class="{ 'is-active': mode === m }"
            @click="setMode(m)"
          >
            <motion.span
              v-if="mode === m"
              layoutId="dt-feed-mode-active"
              class="active-pill"
              :transition="{ type: 'spring', stiffness: 500, damping: 35 }"
            />
            <span class="label">{{ m }}</span>
          </button>
        </div>

        <VariantSelector />
      </div>

      <div class="pane-frame">
        <FocusMode
          v-if="mode === 'focus'"
          class="pane"
          :active-post="activePost"
          :current-index="activePostIndex"
          :is-loading="isLoading"
          :total-posts="totalPosts"
        />
        <BrowseMode
          v-else-if="mode === 'browse'"
          class="pane"
          :active-post-id="postId"
          :posts
        />
      </div>
    </div>
    <LazyDesktopCommentsPanel
      v-if="activePost && shouldMountCommentsPanel"
      :active-post-id="+activePost.id"
      :topic-slug="activePost.topicSlug"
    />
    <LazyDesktopNotesPanel
      v-if="activePost && shouldMountNotesPanel"
      :active-post-id="+activePost.id"
    />
  </div>
</template>

<style scoped>
.feed-page {
  flex: 1;
  display: flex;
  flex-direction: row;
  overflow: hidden;
  min-width: 0;
}
.stack {
  flex: 1;
  display: grid;
  grid-template-rows: auto minmax(0, 1fr);
  overflow: hidden;
  min-width: 0;
}
.feed-toolbar {
  height: 42px;
  border-bottom: 1px solid #111;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 0 14px;
  flex-shrink: 0;
}
.toggle {
  display: flex;
  border: 1px solid #161616;
  border-radius: 4px;
  overflow: hidden;
}
.toggle-btn {
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
.toggle-btn:hover:not(.is-active) {
  color: #555;
}
.toggle-btn.is-active {
  color: var(--kind-example);
}
.active-pill {
  position: absolute;
  inset: 0;
  background: var(--dt-rail-active-bg);
  z-index: 0;
}
.label {
  position: relative;
  z-index: 1;
}
.pane-frame {
  min-width: 0;
  min-height: 0;
  overflow: hidden;
  position: relative;
}
.pane {
  position: absolute;
  inset: 0;
  display: flex;
}
</style>
