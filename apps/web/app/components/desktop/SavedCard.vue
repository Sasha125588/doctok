<script setup lang="ts">
import PostKindBadge from '~/components/post/PostKindBadge.vue'
import { useFeedRouteState } from '~/composables/useFeedRouteState'

import type { SavedPostView } from '~~/generated/api/types.gen'

const props = defineProps<{ post: SavedPostView; searchQuery: string }>()

const escapeHtml = (value: string) =>
  value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;')

const buildHighlightedHtml = (value: string) => {
  if (!props.searchQuery) return escapeHtml(value)

  const escapedQuery = props.searchQuery.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&')
  const splitRegex = new RegExp(`(${escapedQuery})`, 'gi')
  const exactRegex = new RegExp(`^${escapedQuery}$`, 'i')

  return value
    .split(splitRegex)
    .map((part) =>
      exactRegex.test(part)
        ? `<span style="color: #00e87a; background: #001f0d;">${escapeHtml(part)}</span>`
        : escapeHtml(part)
    )
    .join('')
}

const highlightedTitle = computed(() => {
  return buildHighlightedHtml(props.post.title)
})

const highlightedTopicSlug = computed(() => {
  return buildHighlightedHtml(props.post.topicSlug)
})

const { remove } = useSavedPosts()
const { openPost } = useFeedRouteState()

const open = () =>
  openPost(
    {
      id: +props.post.postId,
      topicSlug: props.post.topicSlug,
    },
    { replace: false }
  )
</script>

<template>
  <article
    class="card"
    tabindex="0"
    role="button"
    :aria-label="`Відкрити: ${props.post.title}`"
    @click="open"
    @keydown.enter="open"
    @keydown.space.prevent="open"
  >
    <PostKindBadge :kind="props.post.kind" />
    <div
      class="title"
      v-html="highlightedTitle"
    />
    <!-- <span>{{ post.topicTitle }}</span> -->
    <div class="topic">// topic: <span v-html="highlightedTopicSlug" /></div>
    <button
      class="remove"
      type="button"
      title="видалити зі збережених"
      @click.stop="remove(props.post)"
    >
      <Icon
        name="lucide:bookmark-minus"
        class="remove-icon"
      />
    </button>
  </article>
</template>

<style scoped>
.card {
  position: relative;
  padding: 12px;
  border: 1px solid #0e0e0e;
  border-radius: 6px;
  background: var(--dt-panel-bg);
  cursor: pointer;
  transition: all 0.15s;
  display: flex;
  flex-direction: column;
  gap: 7px;
  min-height: 140px;
  min-width: 0;
  text-align: left;
  font-family: inherit;
  word-break: break-word;
}
.card:hover {
  border-color: #161616;
  background: #0c0c0c;
}
.card:hover .remove {
  opacity: 1;
}
.title {
  font-family: var(--font-display);
  font-size: 13px;
  color: #dadacf;
  line-height: 1.3;
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 3;
  line-clamp: 3;
  overflow: hidden;
}
.topic {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-tertiary);
  margin-top: auto;
}
.highlight {
  color: #00e87a;
  background: #001f0d;
}
.remove {
  position: absolute;
  top: 8px;
  right: 8px;
  width: 22px;
  height: 22px;
  border-radius: 4px;
  border: none;
  background: rgba(0, 0, 0, 0.4);
  color: var(--dt-text-tertiary);
  cursor: pointer;
  opacity: 0;
  transition:
    opacity 0.15s,
    color 0.15s;
  display: flex;
  align-items: center;
  justify-content: center;
}
.remove:hover {
  color: #d88;
}
.remove-icon {
  width: 12px;
  height: 12px;
}
</style>
