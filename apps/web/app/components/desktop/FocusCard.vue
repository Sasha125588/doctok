<script setup lang="ts">
import { motion } from 'motion-v'
import { toast } from 'vue-sonner'

import ActionsColumn from './ActionsColumn.vue'
import CardMeta from './CardMeta.vue'
import RelatedTags from './RelatedTags.vue'
import PostCardBody from '~/components/post/PostCardBody.vue'
import { useNotes } from '~/composables/useNotes'
import { usePostReaction } from '~/composables/usePostReaction'

import type { ReactionValue, TopicPostView } from '#api/types.gen'

const props = defineProps<{
  post: TopicPostView
  totalPosts: number
  currentIndex: number
}>()

const { copy } = useClipboard()
const { share, isSupported } = useShare()

const feedView = useFeedViewStore()
const { togglePanel } = feedView

const { isSaved, toggle } = useSavedPosts()
const { has: hasNote } = useNotes()

const { mutatePostReaction } = usePostReaction(props.post.topicSlug)

const onPostReaction = (value: ReactionValue) => mutatePostReaction(+props.post.id, value)

const onToggleSave = () => toggle(props.post)

const onShare = async () => {
  const url = `${window.location.origin}/topic/${props.post.topicSlug}`

  if (isSupported.value) {
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
        @on-post-reaction="onPostReaction"
        @on-toggle-save="onToggleSave"
        @on-open-note="togglePanel('notes')"
        @on-open-comments="togglePanel('comments')"
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
