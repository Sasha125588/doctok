<script setup lang="ts">
import { motion } from 'motion-v'
import { toast } from 'vue-sonner'

import ActionsColumn from './ActionsColumn.vue'
import CardMeta from './CardMeta.vue'
import RelatedTags from './RelatedTags.vue'
import PostCardBody from '~/components/post/PostCardBody.vue'
import { useNotes } from '~/composables/useNotes'
import { useVote } from '~/composables/useVote'

import type { ReactionValue, TopicPostView } from '#api/types.gen'

const props = defineProps<{
  post: TopicPostView
  totalPosts: number
  currentIndex: number
}>()

const emit = defineEmits<{
  openNotes: []
  openComments: []
}>()

const { copy } = useClipboard()
const { share, isSupported: isShareSupported } = useShare()

const { isSaved, toggle } = useSavedPosts()
const { has: hasNote } = useNotes()

const { functions } = useVote({
  postId: +props.post.id,
  topicSlug: props.post.topicSlug,
})

const onToggleSave = () => toggle(props.post)

async function onShare() {
  const url = `${window.location.origin}/topic/${props.post.topicSlug}`

  if (isShareSupported.value) {
    try {
      await share({
        title: props.post.title,
        text: props.post.topicTitle,
        url,
      })
      return
    } catch {
      return
    }
  }
  try {
    await copy(url)
    toast('скопійовано')
  } catch {}
}
</script>

<template>
  <motion.article
    class="card"
    :initial="{ opacity: 0, x: 14 }"
    :animate="{ opacity: 1, x: 0 }"
    :exit="{ opacity: 0, x: -14 }"
    :transition="{ duration: 0.18, ease: 'easeOut' }"
  >
    <CardMeta
      :topic-title="post.topicTitle"
      :kind="post.kind"
      :total-posts="totalPosts"
      :current-index="currentIndex"
    />

    <h1 class="title">{{ post.title }}</h1>

    <PostCardBody
      :body-html="post.bodyHtml"
      class="body"
    />

    <div class="spacer" />

    <div class="bottom">
      <RelatedTags />
      <ActionsColumn
        :my-vote="post.myVote"
        :like-count="+post.likeCount"
        :comment-count="+post.commentCount"
        :is-saved="isSaved(post)"
        :has-note="hasNote(+post.id)"
        @on-vote="functions.onVote"
        @on-toggle-save="onToggleSave"
        @on-open-note="emit('openNotes')"
        @on-open-comments="emit('openComments')"
        @on-share="onShare"
      />
    </div>
  </motion.article>
</template>

<style scoped>
.card {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 9px;
  min-height: 0;
}
.title {
  font-family: var(--font-display);
  font-size: 27px;
  font-weight: 700;
  color: #eeece4;
  line-height: 1.2;
  letter-spacing: -0.02em;
  flex-shrink: 0;
}
.body {
  flex: 1;
  min-height: 0;
}
.spacer {
  flex: 0 0 4px;
}
.bottom {
  display: flex;
  align-items: flex-end;
  gap: 12px;
  flex-shrink: 0;
}
</style>
