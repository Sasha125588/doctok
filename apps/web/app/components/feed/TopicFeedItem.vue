<script setup lang="ts">
import PostCarousel from '~/components/topic/PostCarousel.vue'
import { useLang } from '~/composables/useLang'
import { useTopicPosts } from '~/composables/useTopicPosts'

import type { TopicFeedPageView } from '#api/types.gen'

const props = defineProps<{
  topic: TopicFeedPageView
  active: boolean
}>()

const { lang } = useLang()

const { state } = useTopicPosts({
  query: { slug: props.topic.slug, lang: lang.value },
})

const swiperKey = computed(() => `${props.topic.slug}:${lang.value}`)
const showSwiper = computed(() => state.posts.value.length > 0)
</script>

<template>
  <div class="relative h-full w-full">
    <PostCarousel
      v-if="showSwiper"
      :key="swiperKey"
      :posts="state.posts.value"
    />

    <div
      v-if="active && state.isLoading && !state.posts.value.length"
      class="pointer-events-none absolute right-6 bottom-24 rounded-full border border-white/10 bg-black/35 px-3 py-1 font-mono text-[0.65rem] text-[var(--text-secondary)] backdrop-blur-sm"
    >
      loading topic…
    </div>
  </div>
</template>
