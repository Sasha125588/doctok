<script setup lang="ts">
import PostKindBadge from '~/components/post/PostKindBadge.vue'
import { useFeedView } from '~/composables/useFeedView'
import { type SavedPost, useSavedPosts } from '~/composables/useSavedPosts'

const props = defineProps<{ post: SavedPost }>()

const router = useRouter()
const { activeTopicSlug, pendingPostId, mode } = useFeedView()
const { remove } = useSavedPosts()

function open() {
  activeTopicSlug.value = props.post.topicSlug
  pendingPostId.value = props.post.postId
  mode.value = 'focus'
  router.push('/')
}

function onRemove() {
  remove(props.post.postId)
}
</script>

<template>
  <article
    class="card"
    tabindex="0"
    role="button"
    @click="open"
    @keydown.enter="open"
    @keydown.space.prevent="open"
  >
    <PostKindBadge :kind="post.kind" />
    <div class="title">{{ post.title }}</div>
    <div class="topic">// topic: {{ post.topicSlug }}</div>
    <button
      class="remove"
      type="button"
      title="видалити зі збережених"
      @click.stop="onRemove"
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
