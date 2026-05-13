<script setup lang="ts">
import SectionIndexGroup from './-components/SectionIndexGroup.vue'
import { sectionIndexGroups } from './-constants'

const sectionCount = computed(() =>
  sectionIndexGroups.reduce((count, group) => count + group.items.length, 0)
)

definePageMeta({
  name: 'sections',
})
</script>

<template>
  <section class="sections-index">
    <div class="sections-inner">
      <header class="sections-hero">
        <div class="hero-copy">
          <div class="kicker">// web platform index</div>
          <h1 class="hero-title">All sections</h1>
          <p class="hero-description">
            Browse the full MDN knowledge surface. Workspace sections open dedicated learning pages;
            everything else drops into scoped search until its workspace is ready.
          </p>
        </div>
        <div class="hero-meta">
          <span class="meta-value">{{ sectionCount }}</span>
          <span class="meta-label">sections</span>
        </div>
      </header>

      <SectionIndexGroup
        v-for="group in sectionIndexGroups"
        :key="group.title"
        :group
      />
    </div>
  </section>
</template>

<style scoped>
.sections-index {
  flex: 1;
  min-width: 0;
  overflow-y: auto;
  scrollbar-gutter: stable;
  padding: 22px 26px;
}
.sections-inner {
  display: grid;
  max-width: 1320px;
  gap: 26px;
  padding-bottom: 20px;
}
.sections-hero {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 24px;
  min-height: 170px;
  padding: 24px 0 10px;
  border-bottom: 1px solid #121212;
}
.hero-copy {
  display: grid;
  max-width: 680px;
  gap: 11px;
}
.kicker {
  color: #6f756d;
  font-family: var(--font-mono);
  font-size: 10px;
  line-height: 1.2;
  letter-spacing: 0.08em;
}
.hero-title {
  color: #e1e1d6;
  font-family: var(--font-display);
  font-size: 40px;
  line-height: 1.02;
  font-weight: 400;
}
.hero-description {
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 12px;
  line-height: 1.7;
}
.hero-meta {
  display: grid;
  min-width: 118px;
  justify-items: end;
  gap: 4px;
  padding: 13px;
  border: 1px solid #161616;
  border-radius: 7px;
  background: #090909;
}
.meta-value {
  color: #d4d4ca;
  font-family: var(--font-display);
  font-size: 26px;
  line-height: 1;
}
.meta-label {
  color: var(--dt-text-quaternary);
  font-family: var(--font-mono);
  font-size: 10px;
}
@media (max-width: 720px) {
  .sections-index {
    padding: 18px;
  }
  .sections-hero {
    align-items: flex-start;
    flex-direction: column;
  }
  .hero-title {
    font-size: 34px;
  }
  .hero-meta {
    justify-items: start;
  }
}
</style>
