<script setup lang="ts">
import { Keyboard, Mousewheel } from 'swiper/modules'
import { Swiper, SwiperSlide } from 'swiper/vue'

import FeedSlide from './TopicFeedItem.vue'
import LangSwitcher from '~/components/lang/LangSwitcher.vue'
import { useFeed } from '~/composables/useFeed'
import { useLang } from '~/composables/useLang'

import type { Swiper as SwiperType } from 'swiper/types'
import 'swiper/css'

const { lang } = useLang()
const { state, functions } = useFeed(lang)

const activeTopicIndex = ref(0)

const activeTopic = computed(() => state.topics.value[activeTopicIndex.value])

function onSlideChange(swiper: SwiperType) {
  activeTopicIndex.value = swiper.activeIndex
}

function onReachEnd() {
  if (state.hasNextPage.value && !state.isFetchingNextPage.value) {
    functions.fetchNextPage()
  }
}
</script>

<template>
  <div class="bg-background relative h-dvh w-full">
    <div class="topbar">
      <div class="font-display text-foreground text-[1.1rem] font-extrabold tracking-tight">
        doc<span>tok</span>
      </div>
      <div
        v-if="activeTopic"
        class="flex max-w-[58%] gap-1 truncate rounded-[20px] border px-2.5 py-1 font-mono text-[0.75rem] transition-colors duration-300"
      >
        {{ activeTopic.title }}
        <!-- <span>({{ activeTopic.postCount }} posts)</span> -->
      </div>
      <LangSwitcher />
    </div>

    <div
      v-if="state.isLoading.value"
      class="flex h-full items-center justify-center"
    >
      <div class="font-mono text-sm text-[var(--text-secondary)]">loading topics...</div>
    </div>

    <div
      v-else-if="!state.topics.value.length"
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
        v-for="(topic, index) in state.topics.value"
        :key="topic.slug"
      >
        <FeedSlide
          :topic="topic"
          :active="index === activeTopicIndex"
        />
      </SwiperSlide>
    </Swiper>

    <div
      v-if="state.isFetchingNextPage.value"
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
