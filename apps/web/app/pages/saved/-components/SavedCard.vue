<script setup lang="ts">
import { toast } from 'vue-sonner'

import PostKindBadge from '~/components/post/PostKindBadge.vue'
import { useFeedRouteState } from '~/composables/useFeedRouteState'
import { usePostKind } from '~/composables/usePostKind'
import { savedPostRemoveUndoDelayMs } from '~/composables/useSavedPosts'

import type { SavedPostView } from '~~/generated/api/types.gen'

type SavedCardView = 'grid' | 'grouped'

const props = withDefaults(
  defineProps<{ post: SavedPostView; searchQuery: string; view?: SavedCardView }>(),
  { view: 'grid' }
)

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

const isGrouped = computed(() => props.view === 'grouped')
const kindConfig = usePostKind(() => props.post.kind)

const savedAtLabel = computed(() => {
  const date = new Date(props.post.savedAt)

  if (Number.isNaN(date.getTime())) return null

  return new Intl.DateTimeFormat('en-GB', {
    day: 'numeric',
    month: 'short',
  }).format(date)
})

const { remove, restore } = useSavedPosts()

const removeSavedPost = async () => {
  try {
    await remove(props.post)

    toast.success('видалено зі збережених', {
      duration: savedPostRemoveUndoDelayMs,
      action: {
        label: 'скасувати',
        onClick: async () => {
          try {
            await restore(props.post)
          } catch {
            toast.error('не вдалося повернути пост')
          }
        },
      },
    })
  } catch {
    toast.error('не вдалося видалити зі збережених')
  }
}

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
    :class="{ 'is-grouped': isGrouped }"
    :style="{ '--saved-kind-color': kindConfig.cssColor }"
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
    <div class="meta">
      <div
        v-if="!isGrouped"
        class="topic"
      >
        // topic: <span v-html="highlightedTopicSlug" />
      </div>
      <div
        v-if="savedAtLabel"
        class="saved-at"
      >
        saved {{ savedAtLabel }}
      </div>
    </div>
    <button
      class="remove"
      type="button"
      title="видалити зі збережених"
      @click.stop="removeSavedPost()"
      @keydown.enter.stop
      @keydown.space.stop
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
  --saved-kind-color: var(--text-secondary);
  position: relative;
  overflow: hidden;
  padding: 12px;
  border: 1px solid #181818;
  border-radius: 6px;
  background: #0b0b0b;
  box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.012) inset;
  cursor: pointer;
  transition: all 0.15s;
  display: flex;
  flex-direction: column;
  gap: 7px;
  min-height: 140px;
  min-width: 0;
  text-align: left;
  font-family: inherit;
}
.card::before {
  position: absolute;
  top: 10px;
  bottom: 10px;
  left: 0;
  width: 2px;
  border-radius: 0 999px 999px 0;
  background: var(--saved-kind-color);
  content: '';
  opacity: 0.2;
  transition:
    opacity 0.15s,
    transform 0.15s;
}
.card.is-grouped {
  gap: 8px;
}
.card:hover {
  border-color: #242424;
  background: #101010;
}
.card:hover::before {
  opacity: 0.72;
  transform: scaleY(1.06);
}
.card:hover .remove,
.remove:focus-visible {
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
.meta {
  display: grid;
  gap: 4px;
  min-width: 0;
  margin-top: auto;
}
.topic {
  overflow: hidden;
  min-width: 0;
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.35;
}
.saved-at {
  color: #6f756d;
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.35;
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
