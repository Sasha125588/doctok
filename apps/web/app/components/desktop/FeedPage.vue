<script setup lang="ts">
import BrowseMode from './BrowseMode.vue'
import CommentsPanel from './CommentsPanel.vue'
import FocusMode from './FocusMode.vue'
import NotesPanel from './NotesPanel.vue'
import { useFeedView } from '~/composables/useFeedView'

const { mode, activeTopicSlug } = useFeedView()

// Keyed so child composables re-instantiate on slug change (see FocusMode note).
const slugKey = computed(() => `${mode.value}:slug:${activeTopicSlug.value}`)
</script>

<template>
  <div class="feed-page">
    <div class="stack">
      <FocusMode
        v-if="activeTopicSlug && mode === 'focus'"
        :key="slugKey"
        class="pane"
      />
      <BrowseMode
        v-else-if="activeTopicSlug && mode === 'browse'"
        :key="slugKey"
        class="pane"
      />
    </div>
    <CommentsPanel
      v-if="activeTopicSlug"
      :key="`comments-${slugKey}`"
    />
    <NotesPanel
      v-if="activeTopicSlug"
      :key="`notes-${slugKey}`"
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
