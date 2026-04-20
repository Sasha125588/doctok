<script setup lang="ts">
import PostKindBadge from '~/components/post/PostKindBadge.vue'
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useTopicPosts } from '~/composables/useTopicPosts'

const { lang } = useLang()
const { activeTopicSlug, activePostIndex, mode } = useFeedView()

const { state } = useTopicPosts({
  query: {
    slug: activeTopicSlug.value ?? '',
    lang: lang.value,
  },
})

function open(index: number) {
  activePostIndex.value = index
  mode.value = 'focus'
}
</script>

<template>
  <section class="browse">
    <div class="area">
      <div class="grid">
        <button
          v-for="(post, i) in state.posts.value"
          :key="post.id"
          class="card"
          :class="{ 'is-current': i === activePostIndex }"
          @click="open(i)"
        >
          <PostKindBadge :kind="post.kind" />
          <div class="title">{{ post.title }}</div>
          <div
            class="body"
            v-html="post.bodyHtml"
          />
        </button>
      </div>
      <div
        v-if="!state.posts.value.length"
        class="empty"
      >
        // нема постів
      </div>
    </div>
  </section>
</template>

<style scoped>
.browse {
  display: flex;
  flex-direction: column;
  flex: 1;
  overflow: hidden;
}
.area {
  flex: 1;
  padding: 20px 24px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px;
}
.card {
  padding: 12px;
  border: 1px solid #0e0e0e;
  border-radius: 6px;
  background: var(--dt-panel-bg);
  cursor: pointer;
  transition: all 0.15s;
  display: flex;
  flex-direction: column;
  gap: 7px;
  height: 160px;

  min-width: 0;
  text-align: left;
  font-family: inherit;
  word-break: break-word;
}
.card:hover {
  border-color: #161616;
  background: #0c0c0c;
}
.card.is-current {
  border-color: color-mix(in oklab, var(--kind-example) 35%, transparent);
  background: var(--dt-rail-active-bg);
}
.title {
  font-family: var(--font-display);
  font-size: 13px;
  color: #dadacf;
  line-height: 1.3;
}
.body {
  font-family: var(--font-mono);
  font-size: 9px;
  color: #727272;
  line-height: 1.6;
  flex: 1;
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 5;
  line-clamp: 5;
  overflow: hidden;
  margin-bottom: auto;
}
.empty {
  font-size: 11px;
  color: var(--dt-text-quaternary);
  padding: 80px 0;
  text-align: center;
}
</style>
