<script setup lang="ts">
import { Swiper, SwiperSlide } from 'swiper/vue'

import PostCard from '~/components/post/PostCard.vue'

import type { PostItem } from '../../../client/types.gen'

const props = defineProps<{
  posts: PostItem[]
  initialIndex?: number
}>()

const currentIndex = ref(props.initialIndex ?? 0)

function onSlideChange(swiper: { activeIndex: number }) {
  currentIndex.value = swiper.activeIndex
}
</script>

<template>
  <Swiper
    direction="horizontal"
    :slides-per-view="1"
    :initial-slide="initialIndex ?? 0"
    :touch-release-on-edges="true"
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
