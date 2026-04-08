<script setup lang="ts">
import { postsVotesToggleMutation, topicsGetPostsQueryKey } from '#api/@tanstack/vue-query.gen'
import { useMutation, useQueryClient } from '@tanstack/vue-query'

import PostCardActions from './PostCardActions.vue'
import PostCardBody from './PostCardBody.vue'
import PostCardHeader from './PostCardHeader.vue'
import PostKindBadge from './PostKindBadge.vue'
import { usePostKind } from '~/composables/usePostKind'

import type { PostItem, TopicsGetPostsResponse, VoteValue } from '#api/types.gen'

const props = defineProps<{
  post: PostItem
  totalInTopic?: number
  currentIndex?: number
}>()

const kindConfig = usePostKind(() => props.post.kind)

const localVote = ref(props.post.myVote)
const localLikeCount = ref(props.post.likeCount)
const localDislikeCount = ref(props.post.dislikeCount)

watch(
  () => props.post,
  (post) => {
    localVote.value = post.myVote
    localLikeCount.value = post.likeCount
    localDislikeCount.value = post.dislikeCount
  }
)

const resolvedTitle = computed(() => {
  if (typeof props.post.title === 'string' && props.post.title.trim().length > 0) {
    return props.post.title
  }

  return props.post.topicTitle
})

const cardBg = computed(
  () =>
    `radial-gradient(ellipse 80% 60% at 70% 20%, rgba(${kindConfig.value.cssColorRgb},0.18) 0%, rgba(8,12,16,0) 70%)`
)

const { lang } = useLang()

const queryClient = useQueryClient()

const voteMutation = useMutation(postsVotesToggleMutation())

const onVote = (value: VoteValue) =>
  voteMutation.mutate(
    {
      path: { postId: props.post.id },
      body: { value },
    },
    {
      onSuccess(data) {
        localVote.value = data.myVote
        localLikeCount.value = data.likeCount
        localDislikeCount.value = data.dislikeCount

        queryClient.setQueryData(
          topicsGetPostsQueryKey({
            query: {
              slug: props.post.topicSlug,
              lang: lang.value,
            },
          }),
          (oldData: TopicsGetPostsResponse | undefined) => {
            if (!oldData) return oldData
            const targetId = props.post.id

            const changes = {
              likeCount: data.likeCount,
              dislikeCount: data.dislikeCount,
              myVote: data.myVote,
            }

            return {
              ...oldData,
              items: oldData.items.map((post) =>
                post.id === targetId ? { ...post, ...changes } : post
              ),
            }
          }
        )
      },
      onError(error) {
        console.error(`${value} Error:`, error)
      },
    }
  )
</script>

<template>
  <div class="card">
    <div
      class="card-bg"
      :style="{ background: cardBg }"
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
        {{ resolvedTitle }}
      </h1>

      <PostCardBody
        :body="post.body"
        class="mt-3.5"
      />

      <PostCardActions
        @on-vote="onVote"
        :my-vote="localVote"
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
