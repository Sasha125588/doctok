<script setup lang="ts">
import { Keyboard, Mousewheel } from 'swiper/modules'
import { Swiper, SwiperSlide } from 'swiper/vue'

import FeedSlide from './FeedSlide.vue'
import LangSwitcher from '~/components/lang/LangSwitcher.vue'
import { useFeed } from '~/composables/useFeed'
import { useLang } from '~/composables/useLang'
import { usePostKind } from '~/composables/usePostKind'
import { toPreviewPost } from '~/lib/topic-feed'
import 'swiper/css'
import type { PostItem } from '#api/types.gen'

const { lang } = useLang()
const { topics, fetchNextPage, hasNextPage, isLoading, isFetchingNextPage } = useFeed(lang)

const activeIndex = ref(0)
const activeTopic = computed(() => topics.value[activeIndex.value])
const activePost = ref<PostItem | null>(null)
const activeKind = usePostKind(
  () => activePost.value?.kind ?? activeTopic.value?.preview.kind ?? 'summary'
)

watch(lang, () => {
  activeIndex.value = 0
})

watch(
  activeTopic,
  (topic) => {
    activePost.value = topic ? toPreviewPost(topic) : null
  },
  { immediate: true }
)

function onSlideChange(swiper: { activeIndex: number }) {
  activeIndex.value = swiper.activeIndex
}

function onReachEnd() {
  if (hasNextPage.value && !isFetchingNextPage.value) {
    fetchNextPage()
  }
}

function onActivePostChange(post: PostItem) {
  activePost.value = post
}
</script>

<template>
  <div class="bg-background relative h-dvh w-full">
    <div class="topbar">
      <div class="font-display text-foreground text-[1.1rem] font-extrabold tracking-tight">
        doc<span :style="{ color: activeKind.cssColor }">tok</span>
      </div>
      <div
        v-if="activeTopic"
        class="max-w-[58%] truncate rounded-[20px] border px-2.5 py-1 font-mono text-[0.68rem] transition-colors duration-300"
        :style="{
          color: activeKind.cssColor,
          borderColor: activeKind.cssColor + '44',
        }"
      >
        {{ activeTopic.title }}
      </div>
      <LangSwitcher />
    </div>

    <div
      v-if="isLoading && !topics.length"
      class="flex h-full items-center justify-center"
    >
      <div class="font-mono text-sm text-[var(--text-secondary)]">loading topics...</div>
    </div>

    <div
      v-else-if="!topics.length"
      class="flex h-full items-center justify-center"
    >
      <div class="font-mono text-sm text-[var(--text-secondary)]">no topics yet</div>
    </div>

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
        v-for="(topic, index) in topics"
        :key="topic.slug"
      >
        <FeedSlide
          :topic="topic"
          :active="index === activeIndex"
          @active-post-change="onActivePostChange"
        />
      </SwiperSlide>
    </Swiper>

    <div
      v-if="isFetchingNextPage"
      class="pointer-events-none absolute right-5 bottom-6 rounded-full border border-white/10 bg-black/30 px-3 py-1 font-mono text-[0.65rem] text-[var(--text-secondary)] backdrop-blur-sm"
    >
      loading more…
    </div>
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
