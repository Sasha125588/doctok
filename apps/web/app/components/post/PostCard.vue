<script setup lang="ts">
import PostCardActions from './PostCardActions.vue'
import PostCardBody from './PostCardBody.vue'
import PostCardHeader from './PostCardHeader.vue'
import PostKindBadge from './PostKindBadge.vue'
import { usePostKind } from '~/composables/usePostKind'
import { useVote } from '~/composables/useVote'

import type { TopicPostView } from '#api/types.gen'

const props = defineProps<{
  post: TopicPostView
  totalInTopic?: number
  currentIndex?: number
}>()

const kindConfig = usePostKind(() => props.post.kind)

const localMyVote = ref(props.post.myVote)
const localLikeCount = ref(props.post.likeCount)
const localDislikeCount = ref(props.post.dislikeCount)

watch(
  () => props.post,
  (post) => {
    localMyVote.value = post.myVote
    localLikeCount.value = post.likeCount
    localDislikeCount.value = post.dislikeCount
  }
)

const { functions } = useVote({
  postId: +props.post.id,
  topicSlug: props.post.topicSlug,
  localMyVote,
  localLikeCount: +localLikeCount,
  localDislikeCount: +localDislikeCount,
})
</script>

<template>
  <div class="card">
    <div
      class="card-bg"
      :style="{
        background: `radial-gradient(ellipse 80% 60% at 70% 20%, rgba(${kindConfig.cssColorRgb},0.18) 0%, rgba(8,12,16,0) 70%)`,
      }"
    />
    <div class="card-content">
      <PostCardHeader
        :kind="post.kind"
        :topic-slug="post.topicSlug"
        :total-in-topic="totalInTopic"
        :current-index="currentIndex"
      />

      <PostKindBadge
        :kind="post.kind"
        class="mt-4"
      />

      <h1
        class="font-display text-foreground mt-4 text-[clamp(1.5rem,5vw,2.1rem)] leading-[1.15] font-extrabold tracking-tight"
      >
        {{ props.post.title }}
      </h1>

      <PostCardBody
        :body-html="post.bodyHtml"
        class="mt-3.5"
      />

      <PostCardActions
        @on-vote="functions.onVote"
        :my-vote="localMyVote"
        :like-count="localLikeCount"
        :dislike-count="localDislikeCount"
        :comment-count="post.commentCount"
        class="mt-5"
      />
    </div>
  </div>
</template>

<style scoped>
.card {
  position: relative;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  height: 100%;
  width: 100%;
  padding: 0 0 80px;
  user-select: none;
}

.card-bg {
  position: absolute;
  inset: 0;
  z-index: 0;
}
.card-bg::before {
  content: '';
  position: absolute;
  inset: 0;
  opacity: 0.03;
  background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noise'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noise)' opacity='1'/%3E%3C/svg%3E");
  background-size: 256px 256px;
  z-index: 1;
}

.card-content {
  position: relative;
  z-index: 2;
  padding: 0 24px;
}
</style>
