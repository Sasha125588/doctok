<script setup lang="ts">
import { format } from 'date-fns'
import { storeToRefs } from 'pinia'

import DesktopSidePanel from './DesktopSidePanel.vue'
import { useComments } from '~/composables/useComments'
import { useFeedViewStore } from '~/stores/feedView'

const props = defineProps<{ activePostId: number; topicSlug: string }>()

const feedView = useFeedViewStore()
const { activePanel } = storeToRefs(feedView)

const activePostId = computed(() => props.activePostId)
const topicSlug = computed(() => props.topicSlug)
const isOpen = computed(() => activePanel.value === 'comments')

const { comments, isLoading, isSending, send, vote } = useComments(activePostId, topicSlug, isOpen)

const draft = ref('')

const submit = () => {
  if (isSending.value) return
  send(draft.value, () => {
    draft.value = ''
  })
}

const formatTime = (iso: string) => format(new Date(iso), 'HH:mm')
</script>

<template>
  <DesktopSidePanel
    :open="isOpen"
    title="comments"
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
        :key="c.id"
        class="comment"
      >
        <div class="meta">
          <div class="avatar">{{ (c.userId[0] ?? '?').toUpperCase() }}</div>
          <span class="author">{{ c.userId?.slice(10) ?? 'user' }}</span>
          <span class="time">{{ formatTime(c.createdAt) }}</span>
        </div>
        <div class="text">{{ c.body }}</div>
        <div class="reactions">
          <button
            type="button"
            class="reaction"
            :class="{ 'reaction--liked': c.myVote === 'like' }"
            :aria-pressed="c.myVote === 'like'"
            aria-label="Like comment"
            @click="vote(+c.id, 'like')"
          >
            <Icon
              name="lucide:heart"
              class="reaction-icon"
            />
            <span>{{ c.likeCount }}</span>
          </button>
          <button
            type="button"
            class="reaction"
            :class="{ 'reaction--disliked': c.myVote === 'dislike' }"
            :aria-pressed="c.myVote === 'dislike'"
            aria-label="Dislike comment"
            @click="vote(+c.id, 'dislike')"
          >
            <Icon
              name="lucide:thumbs-down"
              class="reaction-icon"
            />
            <span>{{ c.dislikeCount }}</span>
          </button>
        </div>
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
.reactions {
  display: flex;
  align-items: center;
  gap: 5px;
  padding-left: 24px;
}
.reaction {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  min-width: 32px;
  height: 18px;
  border: 1px solid transparent;
  border-radius: 3px;
  background: transparent;
  color: var(--dt-text-quaternary);
  font-family: var(--font-mono);
  font-size: 8px;
  cursor: pointer;
  transition:
    color 0.15s,
    background 0.15s,
    border-color 0.15s;
}
.reaction:hover:not(:disabled) {
  background: color-mix(in oklab, var(--dt-rail-active-bg) 65%, transparent);
  color: var(--dt-text-tertiary);
}
.reaction:disabled {
  cursor: wait;
  opacity: 0.6;
}
.reaction-icon {
  width: 10px;
  height: 10px;
}
.reaction--liked {
  color: #d14d4d;
  background: rgba(209, 77, 77, 0.1);
  border-color: rgba(209, 77, 77, 0.18);
}
.reaction--disliked {
  color: #3d72c6;
  background: rgba(61, 114, 198, 0.1);
  border-color: rgba(61, 114, 198, 0.18);
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
