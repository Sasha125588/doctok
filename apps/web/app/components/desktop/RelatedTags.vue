<script setup lang="ts">
import { useFeedView } from '~/composables/useFeedView'
import { useLang } from '~/composables/useLang'
import { useTopicLinks } from '~/composables/useTopicLinks'

const { lang } = useLang()
const { activeTopicSlug, activePostIndex } = useFeedView()

const enabled = computed(() => !!activeTopicSlug.value)
const queryOptions = computed(() => ({
  query: {
    slug: activeTopicSlug.value ?? '',
    lang: lang.value,
  },
}))

const { state } = useTopicLinks(queryOptions, enabled)

function go(slug: string) {
  activeTopicSlug.value = slug
  activePostIndex.value = 0
}
</script>

<template>
  <div class="wrap">
    <div class="label">→ related</div>
    <div
      v-if="state.links.value.length"
      class="tags"
    >
      <button
        v-for="link in state.links.value"
        :key="link.slug"
        class="tag"
        @click="go(link.slug)"
      >
        {{ link.title }}
      </button>
    </div>
    <div
      v-else-if="state.isLoading.value"
      class="loading"
    >
      …
    </div>
  </div>
</template>

<style scoped>
.wrap {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 5px;
  min-width: 0;
}
.label {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-quaternary);
  letter-spacing: 0.1em;
}
.tags {
  display: flex;
  gap: 4px;
  flex-wrap: wrap;
}
.tag {
  font-family: var(--font-mono);
  font-size: 9px;
  padding: 3px 8px;
  border-radius: 2px;
  border: 1px solid var(--dt-border-subtle);
  color: var(--dt-text-tertiary);
  cursor: pointer;
  background: none;
  transition: all 0.15s;
}
.tag:hover {
  border-color: color-mix(in oklab, var(--kind-example) 30%, transparent);
  color: var(--kind-example);
  background: var(--dt-rail-active-bg);
}
.loading {
  font-family: var(--font-mono);
  font-size: 8px;
  color: var(--dt-text-quaternary);
}
</style>
