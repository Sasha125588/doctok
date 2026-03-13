<script setup lang="ts">
import PostCard from '~/components/post/PostCard.vue'
import TopicSwiper from '~/components/topic/TopicSwiper.vue'
import { useLang } from '~/composables/useLang'
import { useTopicPosts } from '~/composables/useTopicPosts'

import type { PostItem } from '../../../client/types.gen'

const props = defineProps<{
  post: PostItem
  active: boolean
}>()

const { lang } = useLang()
const topicSlug = computed(() => props.post.topicSlug)
const { posts: topicPosts } = useTopicPosts(topicSlug, lang)

const initialIndex = computed(() => {
  if (!topicPosts.value.length) return 0
  const pos = Number(props.post.position)
  return Math.max(0, Math.min(pos, topicPosts.value.length - 1))
})

const showSwiper = computed(() => props.active && topicPosts.value.length > 0)
</script>

<template>
  <div class="h-full w-full">
    <TopicSwiper
      v-if="showSwiper"
      :posts="topicPosts"
      :initial-index="initialIndex"
    />
    <PostCard
      v-else
      :post="post"
    />
  </div>
</template>
