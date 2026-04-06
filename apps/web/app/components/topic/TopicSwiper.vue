<script setup lang="ts">
import { Swiper, SwiperSlide } from 'swiper/vue'

import PostCard from '~/components/post/PostCard.vue'

import type { PostItem } from '#api/types.gen'

const props = defineProps<{
  posts: PostItem[]
  initialIndex?: number
}>()

const emit = defineEmits<{
  activePostChange: [post: PostItem]
}>()

const currentIndex = ref(props.initialIndex ?? 0)

watch(
  () => props.initialIndex,
  (value) => {
    currentIndex.value = value ?? 0
  },
  { immediate: true }
)

watch(
  () => props.posts,
  (posts) => {
    if (!posts.length) {
      return
    }

    const nextIndex = Math.max(0, Math.min(currentIndex.value, posts.length - 1))
    const activePost = posts[nextIndex]

    currentIndex.value = nextIndex

    if (activePost) {
      emit('activePostChange', activePost)
    }
  },
  { immediate: true }
)

function onSlideChange(swiper: { activeIndex: number }) {
  currentIndex.value = swiper.activeIndex

  const post = props.posts[swiper.activeIndex]
  if (post) {
    emit('activePostChange', post)
  }
}
</script>

<template>
  <Swiper
    direction="horizontal"
    :slides-per-view="1"
    :initial-slide="initialIndex ?? 0"
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
