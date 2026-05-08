<script setup lang="ts">
import LangSwitcher from '~/components/lang/LangSwitcher.vue'
import { useLang } from '~/composables/useLang'
import { useTopicPosts } from '~/composables/useTopicPosts'

const route = useRoute()
const slug = computed(() => route.params.slug!.join('/'))

const { lang } = useLang()

const queryOptions = computed(() => ({
  query: {
    slug: slug.value ?? '',
    lang: lang.value,
  },
}))

const { posts } = useTopicPosts(queryOptions)
const topicTitle = computed(() => posts.value[0]?.topicTitle ?? slug.value)
</script>

<template>
  <div class="bg-background relative h-dvh w-full">
    <div class="topbar">
      <NuxtLink
        to="/"
        class="font-display text-foreground text-[1.1rem] font-extrabold tracking-tight"
      >
        doc<span class="text-[var(--kind-summary)]">tok</span>
      </NuxtLink>
      <div
        class="max-w-[50%] truncate rounded-[20px] border border-[rgba(255,255,255,0.15)] px-2.5 py-1 font-mono text-[0.68rem] text-[var(--text-secondary)]"
      >
        {{ topicTitle }}
      </div>
      <LangSwitcher />
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
