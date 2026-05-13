<script setup lang="ts">
import SectionPage from './-components/SectionPage.vue'
import { findSectionWorkspace } from './-constants'

const route = useRoute()
const section = computed(() => findSectionWorkspace(route.params.slug))

definePageMeta({
  name: 'section',
})
</script>

<template>
  <SectionPage
    v-if="section"
    :section
  />
  <section
    v-else
    class="section-missing"
  >
    <div class="missing-copy">
      <div class="missing-kicker">// section</div>
      <h1 class="missing-title">Section not mapped yet.</h1>
      <p class="missing-text">
        This area is not available as a workspace yet. Search still works as the fallback.
      </p>
      <NuxtLink
        class="missing-link"
        :to="{ name: 'search' }"
      >
        Open search
      </NuxtLink>
    </div>
  </section>
</template>

<style scoped>
.section-missing {
  flex: 1;
  display: grid;
  place-items: center;
  padding: 24px;
}
.missing-copy {
  display: grid;
  max-width: 420px;
  gap: 10px;
  text-align: center;
}
.missing-kicker {
  color: #6f756d;
  font-family: var(--font-mono);
  font-size: 10px;
  letter-spacing: 0.08em;
}
.missing-title {
  color: #d0d0c7;
  font-family: var(--font-display);
  font-size: 24px;
  font-weight: 400;
}
.missing-text {
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 11px;
  line-height: 1.6;
}
.missing-link {
  justify-self: center;
  color: var(--kind-example);
  font-family: var(--font-mono);
  font-size: 11px;
  text-decoration: none;
}
</style>
