<script setup lang="ts">
import BrowseMode from './BrowseMode.vue'
import CommentsPanel from './CommentsPanel.vue'
import FocusMode from './FocusMode.vue'
import NotesPanel from './NotesPanel.vue'
import { useFeedView } from '~/composables/useFeedView'

const { mode, activeTopicSlug, activePostIndex } = useFeedView()

const { lang } = useLang()

const queryOptions = computed(() => ({
  query: {
    slug: activeTopicSlug.value ?? '',
    lang: lang.value,
  },
}))

const { state } = useTopicPosts(queryOptions)

const activePost = computed(() => state.posts.value[activePostIndex.value])
const totalPosts = computed(() => state.posts.value.length)

const slugKey = computed(() => `$slug:${activeTopicSlug.value}`)
</script>

<template>
  <div class="feed-page">
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
</style>
