<script setup lang="ts">
import DesktopSidePanel from './DesktopSidePanel.vue'
import { useComments } from '~/composables/useComments'
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useTopicPosts } from '~/composables/useTopicPosts'

const { lang } = useLang()
const { activeTopicSlug, activePostIndex, activePanel } = useFeedView()

const { state } = useTopicPosts({
  query: {
    slug: activeTopicSlug.value ?? '',
    lang: lang.value,
  },
})

const activePostId = computed(() => {
  const post = state.posts.value[activePostIndex.value]
  return post ? +post.id : null
})

const { comments, isLoading, isSending, send } = useComments(activePostId)

const draft = ref('')
const isOpen = computed(() => activePanel.value === 'comments')

function submit() {
  if (isSending.value) return
  if (!draft.value.trim()) return
  send(draft.value, () => {
    draft.value = ''
  })
}

function close() {
  if (document.activeElement instanceof HTMLElement) document.activeElement.blur()
  activePanel.value = null
}

function initial(str: string | undefined) {
  return (str?.[0] ?? '?').toUpperCase()
}

function formatTime(iso: string | undefined) {
  if (!iso) return ''
  try {
    const d = new Date(iso)
    return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  } catch {
    return ''
  }
}
</script>

<template>
  <DesktopSidePanel
    :open="isOpen"
    title="comments"
    @close="close"
  >
    <div class="list">
      <div
        v-if="isLoading"
        class="empty"
      >
        // завантаження...
      </div>
      <div
        v-else-if="!comments.length"
        class="empty"
      >
        // поки немає коментарів
      </div>
      <div
        v-for="c in comments"
        :key="String(c.id)"
        class="comment"
      >
        <div class="meta">
          <div class="avatar">{{ initial(c.userId) }}</div>
          <span class="author">{{ c.userId ?? 'user' }}</span>
          <span class="time">{{ formatTime(c.createdAt) }}</span>
        </div>
        <div class="text">{{ c.body }}</div>
      </div>
    </div>
    <div class="input-wrap">
      <textarea
        v-model="draft"
        class="textarea"
        rows="1"
        placeholder="написати..."
        @keydown.enter.exact.prevent="submit"
      />
      <button
        class="send"
        :disabled="isSending"
        @click="submit"
      >
        →
      </button>
    </div>
  </DesktopSidePanel>
</template>

<style scoped>
.list {
  flex: 1;
  overflow-y: auto;
  padding: 10px 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.empty {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-quaternary);
}
.comment {
  display: flex;
  flex-direction: column;
  gap: 3px;
}
.meta {
  display: flex;
  align-items: center;
  gap: 6px;
}
.avatar {
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: var(--dt-rail-active-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: var(--font-mono);
  font-size: 7px;
  color: var(--kind-example);
  flex-shrink: 0;
}
.author {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--dt-text-tertiary);
}
.time {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-quaternary);
  margin-left: auto;
}
.text {
  font-family: var(--font-mono);
  font-size: 10px;
  color: #3a3a3a;
  line-height: 1.6;
  padding-left: 24px;
}
.input-wrap {
  padding: 10px 12px;
  border-top: 1px solid #0e0e0e;
  display: flex;
  gap: 6px;
  flex-shrink: 0;
}
.textarea {
  background: var(--dt-panel-bg);
  border: 1px solid var(--dt-panel-border);
  border-radius: 4px;
  outline: none;
  font-family: var(--font-mono);
  font-size: 10px;
  color: #c8c8c0;
  padding: 7px 9px;
  flex: 1;
  caret-color: var(--kind-example);
  resize: none;
}
.textarea::placeholder {
  color: var(--dt-text-quaternary);
}
.send {
  background: var(--dt-rail-active-bg);
  border: 1px solid color-mix(in oklab, var(--kind-example) 20%, transparent);
  border-radius: 3px;
  color: var(--kind-example);
  font-family: var(--font-mono);
  font-size: 9px;
  padding: 0 9px;
  cursor: pointer;
  transition: all 0.15s;
}
.send:hover:not(:disabled) {
  background: #002a12;
}
.send:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
