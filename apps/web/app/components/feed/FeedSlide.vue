<script setup lang="ts">
import PostCard from '~/components/post/PostCard.vue'
import TopicSwiper from '~/components/topic/TopicSwiper.vue'
import { useLang } from '~/composables/useLang'
import { useTopicPosts } from '~/composables/useTopicPosts'
import { toPreviewPost } from '~/lib/topic-feed'

import type { PostItem, TopicFeedItem } from '#api/types.gen'

const props = defineProps<{
  topic: TopicFeedItem
  active: boolean
}>()

const emit = defineEmits<{
  activePostChange: [post: PostItem]
}>()

const { lang } = useLang()
const topicSlug = computed(() => props.topic.slug)
// const topicEnabled = computed(() => props.active)

const { posts: topicPosts, isLoading } = useTopicPosts({
  query: { slug: topicSlug.value, lang: lang.value },
})

const previewPost = computed(() => toPreviewPost(props.topic))
const swiperKey = computed(() => `${props.topic.slug}:${lang.value}`)
const showSwiper = computed(() => props.active && topicPosts.value.length > 0)

watch(
  [() => props.active, previewPost, topicPosts],
  ([active, preview, posts]) => {
    if (!active) {
      return
    }

    emit('activePostChange', posts[0] ?? preview)
  },
  { immediate: true }
)

function onActivePostChange(post: PostItem) {
  emit('activePostChange', post)
}
</script>

<template>
  <div class="relative h-full w-full">
    <TopicSwiper
      v-if="showSwiper"
      :key="swiperKey"
      :posts="topicPosts"
      :initial-index="0"
      @active-post-change="onActivePostChange"
    />
    <PostCard
      v-else
      :post="previewPost"
    />

    <div
      v-if="active && isLoading && !topicPosts.length"
      class="pointer-events-none absolute right-6 bottom-24 rounded-full border border-white/10 bg-black/35 px-3 py-1 font-mono text-[0.65rem] text-[var(--text-secondary)] backdrop-blur-sm"
    >
      loading topic…
    </div>
  </div>
</template>
