<script setup lang="ts">
import { Swiper, SwiperSlide } from 'swiper/vue'

import PostCard from '~/components/post/PostCard.vue'

import type { TopicPostView } from '#api/types.gen'

const props = defineProps<{
  posts: TopicPostView[]
}>()

const currentIndex = ref(0)

function onSlideChange(swiper: { activeIndex: number }) {
  currentIndex.value = swiper.activeIndex
}
</script>

<template>
  <Swiper
    direction="horizontal"
    :slides-per-view="1"
    :initial-slide="0"
    :touch-release-on-edges="true"
    :nested="true"
    class="h-full w-full"
    @slide-change="onSlideChange"
  >
    <SwiperSlide
      v-for="post in posts"
      :key="post.id"
    >
      <PostCard
        :post="post"
        :total-in-topic="posts.length"
        :current-index="currentIndex"
      />
    </SwiperSlide>
  </Swiper>
</template>
