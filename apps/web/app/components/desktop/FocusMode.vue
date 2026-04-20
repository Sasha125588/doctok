<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'

import FocusCard from './FocusCard.vue'
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useTopicPosts } from '~/composables/useTopicPosts'

const { lang } = useLang()
const { activeTopicSlug, activePostIndex, activePanel, activeTopicPostCount } = useFeedView()

// Snapshot slug: FocusMode is re-keyed by FeedPage on slug change (see Task 5.2),
// so the composable remounts naturally. Mirrors the `topic/[...slug].vue` approach.
const { state } = useTopicPosts({
  query: {
    slug: activeTopicSlug.value ?? '',
    lang: lang.value,
  },
})

const activePost = computed(() => state.posts.value[activePostIndex.value])

// Publish post count to shared state so keyboard handler (DesktopShell, Task 6.6)
// can bound ← / → navigation without re-calling useTopicPosts.
watch(
  () => state.posts.value.length,
  (len) => (activeTopicPostCount.value = len),
  { immediate: true }
)

const toastMessage = ref<string | null>(null)
let toastTimer: ReturnType<typeof setTimeout> | null = null

function showToast(msg: string) {
  toastMessage.value = msg
  if (toastTimer) clearTimeout(toastTimer)
  toastTimer = setTimeout(() => {
    toastMessage.value = null
  }, 1600)
}

function openNotes() {
  activePanel.value = activePanel.value === 'notes' ? null : 'notes'
}

function openComments() {
  activePanel.value = activePanel.value === 'comments' ? null : 'comments'
}

onUnmounted(() => {
  if (toastTimer) clearTimeout(toastTimer)
})
</script>

<template>
  <section class="focus">
    <div class="card-area">
      <AnimatePresence
        :initial="false"
        mode="wait"
      >
        <motion.div
          :key="activePost ? `card-${activePost.id}` : state.isLoading.value ? 'loading' : 'empty'"
          class="card-slot"
          :initial="{ opacity: 0, x: 10 }"
          :animate="{ opacity: 1, x: 0 }"
          :exit="{ opacity: 0, x: -10 }"
          :transition="{ duration: 0.16, ease: 'easeOut' }"
        >
          <FocusCard
            v-if="activePost"
            :post="activePost"
            :total-posts="state.posts.value.length"
            :current-index="activePostIndex"
            @open-notes="openNotes"
            @open-comments="openComments"
            @toast="showToast"
          />
          <div
            v-else-if="state.isLoading.value"
            class="loading"
          >
            // завантаження...
          </div>
          <div
            v-else
            class="loading"
          >
            // оберіть тему
          </div>
        </motion.div>
      </AnimatePresence>

      <div
        v-if="toastMessage"
        class="toast"
      >
        {{ toastMessage }}
      </div>
    </div>
  </section>
</template>

<style scoped>
.focus {
  display: flex;
  flex-direction: column;
  flex: 1;
  overflow: hidden;
}
.card-area {
  flex: 1;
  padding: 18px 28px 18px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  position: relative;
}
.loading {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  color: var(--dt-text-quaternary);
}
.card-slot {
  flex: 1;
  min-height: 0;
  display: flex;
}
.toast {
  position: absolute;
  bottom: 20px;
  left: 50%;
  transform: translateX(-50%);
  background: #141414;
  border: 1px solid #222;
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 9px;
  padding: 6px 12px;
  border-radius: 4px;
  letter-spacing: 0.08em;
  pointer-events: none;
}
</style>
