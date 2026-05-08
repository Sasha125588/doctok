<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'
import { storeToRefs } from 'pinia'

import { useFeedRouteState } from '~/composables/useFeedRouteState'
import { useTopicHistory } from '~/composables/useTopicHistory'
import { useFeedViewStore } from '~/stores/feedView'

import type { TopicFeedPageView } from '#api/types.gen'

const props = defineProps<{ topics: TopicFeedPageView[] }>()

const { pinned, recent } = useTopicHistory()

const feedView = useFeedViewStore()
const { sidebarHidden } = storeToRefs(feedView)
const { toggleSidebar } = feedView

const { topicSlug, openTopic } = useFeedRouteState()

const titleOf = (slug: string) => props.topics.find((t) => t.slug === slug)?.title ?? slug

const sections = computed(() => [
  { label: 'pinned', slugs: pinned.value },
  {
    label: 'recent',
    slugs: Array.from(new Set([...recent.value, ...props.topics.map((topic) => topic.slug)])),
  },
])
</script>

<template>
  <motion.aside
    class="sidebar"
    :class="{ 'is-collapsed': sidebarHidden }"
    :animate="{ width: sidebarHidden ? 8 : 168 }"
    :transition="{ duration: 0.22, ease: 'easeInOut' }"
  >
    <div
      class="content"
      :inert="sidebarHidden"
      :aria-hidden="sidebarHidden"
    >
      <div class="header">
        <div class="title">Recent &amp; Pinned</div>
      </div>
      <div class="list">
        <template
          v-for="section in sections"
          :key="section.label"
        >
          <div class="section">{{ section.label }}</div>
          <div
            v-if="!section.slugs.length"
            class="empty"
          >
            // порожньо
          </div>
          <div
            v-for="slug in section.slugs"
            :key="section.label + ':' + slug"
            class="item"
            :class="{ 'is-active': slug === topicSlug }"
            @click="openTopic(slug)"
          >
            <AnimatePresence>
              <motion.span
                v-if="slug === topicSlug"
                layoutId="dt-active-topic-bar"
                class="active-bar"
                :transition="{ type: 'spring', stiffness: 500, damping: 35 }"
              />
            </AnimatePresence>
            <span class="name">{{ titleOf(slug) }}</span>
          </div>
        </template>
      </div>
    </div>
    <button
      class="handle"
      type="button"
      :aria-label="sidebarHidden ? 'Show feed sidebar' : 'Hide feed sidebar'"
      :aria-expanded="!sidebarHidden"
      :title="sidebarHidden ? 'Show feed sidebar' : 'Hide feed sidebar'"
      @click="toggleSidebar"
    />
  </motion.aside>
</template>

<style scoped>
.sidebar {
  position: relative;
  overflow: hidden;
  flex-shrink: 0;
}
.content {
  width: 168px;
  height: 100%;
  display: flex;
  flex-direction: column;
  opacity: 1;
  transition:
    opacity 0.16s,
    visibility 0.16s;
}
.sidebar.is-collapsed .content {
  opacity: 0;
  visibility: hidden;
  pointer-events: none;
}
.handle {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  width: 8px;
  padding: 0;
  border: none;
  border-left: 1px solid var(--dt-sidebar-border);
  background: #080808;
  cursor: col-resize;
  transition:
    background 0.15s,
    border-color 0.15s;
}
.handle::after {
  content: '';
  position: absolute;
  top: 50%;
  right: 3px;
  width: 2px;
  height: 34px;
  border-radius: 999px;
  background: #151515;
  transform: translateY(-50%);
  transition: background 0.15s;
}
.handle:hover,
.handle:focus-visible {
  border-color: color-mix(in oklab, var(--kind-example) 35%, transparent);
  background: #0b0b0b;
  outline: none;
}
.handle:hover::after,
.handle:focus-visible::after {
  background: color-mix(in oklab, var(--kind-example) 45%, #151515);
}
.header {
  padding: 12px 14px 8px;
  border-bottom: 1px solid #0e0e0e;
  flex-shrink: 0;
}
.title {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-tertiary);
  letter-spacing: 0.14em;
  text-transform: uppercase;
  white-space: nowrap;
}
.list {
  overflow-y: auto;
  flex: 1;
  padding: 6px 0;
}
.section {
  font-family: var(--font-mono);
  font-size: 7px;
  color: var(--dt-text-quaternary);
  letter-spacing: 0.12em;
  text-transform: uppercase;
  padding: 10px 12px 4px;
  white-space: nowrap;
}
.empty {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-quaternary);
  padding: 2px 12px 6px;
}
.item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 7px 12px;
  cursor: pointer;
  transition: background 0.12s;
  position: relative;
  white-space: nowrap;
}
.item:hover {
  background: #0c0c0c;
}
.item.is-active {
  background: #0f1a0f;
}
.active-bar {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 2px;
  background: var(--kind-example);
  border-radius: 0 2px 2px 0;
}
.name {
  font-family: var(--font-mono);
  font-size: 11px;
  color: #444;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
}
.item.is-active .name {
  color: #c8e8c0;
}
</style>
