<script setup lang="ts">
import { Keyboard, Mousewheel } from 'swiper/modules'
import { Swiper, SwiperSlide } from 'swiper/vue'

import FeedSlide from './FeedSlide.vue'
import LangSwitcher from '~/components/lang/LangSwitcher.vue'
import { useFeed } from '~/composables/useFeed'
import { useLang } from '~/composables/useLang'
import { usePostKind } from '~/composables/usePostKind'
import 'swiper/css'

const { lang } = useLang()
const { posts, fetchNextPage, hasNextPage, isLoading } = useFeed(lang)

const activeIndex = ref(0)
const activePost = computed(() => posts.value[activeIndex.value])
const activeKind = usePostKind(() => activePost.value?.kind ?? 'summary')

function onSlideChange(swiper: { activeIndex: number }) {
  activeIndex.value = swiper.activeIndex
}

function onReachEnd() {
  if (hasNextPage.value) {
    fetchNextPage()
  }
}
</script>

<template>
  <div class="bg-background relative h-dvh w-full">
    <!-- Top bar -->
    <div class="topbar">
      <div class="font-display text-foreground text-[1.1rem] font-extrabold tracking-tight">
        doc<span :style="{ color: activeKind.cssColor }">tok</span>
      </div>
      <div
        v-if="activePost"
        class="max-w-[50%] truncate rounded-[20px] border px-2.5 py-1 font-mono text-[0.68rem] transition-colors duration-300"
        :style="{
          color: activeKind.cssColor,
          borderColor: activeKind.cssColor + '44',
        }"
      >
        {{ activePost.topicTitle }}
      </div>
      <LangSwitcher />
    </div>

    <!-- Loading state -->
    <div
      v-if="isLoading && !posts.length"
      class="flex h-full items-center justify-center"
    >
      <div class="font-mono text-sm text-[var(--text-secondary)]">loading...</div>
    </div>

    <!-- Feed -->
    <Swiper
      v-else
      direction="vertical"
      :modules="[Mousewheel, Keyboard]"
      :mousewheel="{ forceToAxis: true }"
      :keyboard="{ enabled: true }"
      :slides-per-view="1"
      class="h-full w-full"
      @slide-change="onSlideChange"
      @reach-end="onReachEnd"
    >
      <SwiperSlide
        v-for="(post, index) in posts"
        :key="post.id"
      >
        <FeedSlide
          :post="post"
          :active="index === activeIndex"
        />
      </SwiperSlide>
    </Swiper>
  </div>
</template>

<style scoped>
.topbar {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px 12px;
  background: linear-gradient(to bottom, rgba(8, 12, 16, 0.95) 60%, transparent);
}
</style>
