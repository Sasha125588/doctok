<script setup lang="ts">
import { toast } from 'vue-sonner'

import BrowseMode from './BrowseMode.vue'
import CommentsPanel from './CommentsPanel.vue'
import FocusMode from './FocusMode.vue'
import NotesPanel from './NotesPanel.vue'
import { useFeedView } from '~/composables/useFeedView'

const isDev = import.meta.dev

const { mode, activeTopicSlug, activePostIndex, pendingPostId } = useFeedView()

const { lang } = useLang()

const queryOptions = computed(() => ({
  query: {
    slug: activeTopicSlug.value ?? '',
    lang: lang.value,
  },
}))

const { state } = useTopicPosts(queryOptions)

// Consume a pendingPostId handoff (e.g. from SavedPage). Runs on both fresh
// state-posts load (slug changed, posts load async) and same-slug re-entry
// (posts already loaded, runs on next tick after pendingPostId flips null→number).
// Clearing pendingPostId to null at the end ensures the effect is a one-shot.
watchEffect(() => {
  const target = pendingPostId.value
  if (target == null) return
  const posts = state.posts.value
  if (!posts.length) return
  const idx = posts.findIndex((p) => +p.id === target)
  activePostIndex.value = idx >= 0 ? idx : 0
  pendingPostId.value = null
})

const activePost = computed(() => state.posts.value[activePostIndex.value])
const totalPosts = computed(() => state.posts.value.length)

const slugKey = computed(() => `$slug:${activeTopicSlug.value}`)
</script>

<template>
  <div class="feed-page">
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
    <div class="stack">
      <FocusMode
        v-if="mode === 'focus'"
        :key="`focus-${slugKey}`"
        class="pane"
        :active-post="activePost"
        :is-loading="state.isLoading.value"
        :total-posts="totalPosts"
      />
      <BrowseMode
        v-else-if="mode === 'browse'"
        :key="`browse-${slugKey}`"
        class="pane"
        :posts="state.posts.value"
      />
    </div>
    <CommentsPanel
      v-if="activePost"
      :key="`comments-${slugKey}`"
      :active-post-id="+activePost.id"
    />
    <NotesPanel
      v-if="activePost"
      :key="`notes-${slugKey}`"
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
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-width: 0;
  position: relative;
}
.pane {
  position: absolute;
  inset: 0;
  display: flex;
}
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
</style>
