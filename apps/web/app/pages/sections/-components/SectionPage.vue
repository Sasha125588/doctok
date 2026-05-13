<script setup lang="ts">
import SectionClusterCard from './SectionClusterCard.vue'
import SectionReferenceList from './SectionReferenceList.vue'
import SectionTrackCard from './SectionTrackCard.vue'

import type { SectionWorkspace } from '../-types'

defineProps<{
  section: SectionWorkspace
}>()
</script>

<template>
  <section
    class="section-page"
    :class="`is-${section.tone}`"
  >
    <div class="section-inner">
      <header class="section-hero">
        <div class="hero-icon">
          <Icon
            :name="section.icon"
            class="hero-icon-svg"
          />
        </div>
        <div class="hero-copy">
          <div class="kicker">// {{ section.eyebrow }}</div>
          <h1 class="hero-title">{{ section.title }}</h1>
          <p class="hero-summary">{{ section.summary }}</p>
        </div>
        <div class="hero-actions">
          <NuxtLink
            class="hero-action is-primary"
            :to="{ name: 'search', query: { q: section.searchQuery } }"
          >
            <Icon
              name="lucide:search"
              class="hero-action-icon"
            />
            Search section
          </NuxtLink>
          <NuxtLink
            class="hero-action"
            :to="{ name: 'feed' }"
          >
            <Icon
              name="lucide:shuffle"
              class="hero-action-icon"
            />
            Feed
          </NuxtLink>
        </div>
      </header>

      <div class="stats-row">
        <div
          v-for="stat in section.stats"
          :key="stat.label"
          class="stat-card"
        >
          <span class="stat-label">{{ stat.label }}</span>
          <span class="stat-value">{{ stat.value }}</span>
        </div>
      </div>

      <div class="workspace-grid">
        <main class="workspace-main">
          <section class="content-band">
            <div class="band-heading">
              <div>
                <div class="kicker">// learn</div>
                <h2 class="band-title">Suggested tracks</h2>
              </div>
              <span class="band-note">ordered for recall</span>
            </div>
            <div class="track-grid">
              <SectionTrackCard
                v-for="(track, index) in section.tracks"
                :key="track.title"
                :track
                :index
              />
            </div>
          </section>

          <section class="content-band">
            <div class="band-heading">
              <div>
                <div class="kicker">// explore</div>
                <h2 class="band-title">Topic clusters</h2>
              </div>
            </div>
            <div class="cluster-grid">
              <SectionClusterCard
                v-for="cluster in section.clusters"
                :key="cluster.title"
                :cluster
              />
            </div>
          </section>
        </main>

        <aside class="workspace-side">
          <section class="content-band">
            <div class="band-heading">
              <div>
                <div class="kicker">// reference</div>
                <h2 class="band-title">Quick lookup</h2>
              </div>
            </div>
            <SectionReferenceList :items="section.reference" />
          </section>

          <section class="content-band">
            <div class="band-heading">
              <div>
                <div class="kicker">// featured</div>
                <h2 class="band-title">From MDN</h2>
              </div>
            </div>
            <a
              v-for="article in section.featured"
              :key="article.href"
              class="featured-card"
              :href="article.href"
              rel="noreferrer"
              target="_blank"
            >
              <span class="featured-category">{{ article.category }}</span>
              <span class="featured-title">{{ article.title }}</span>
              <span class="featured-description">{{ article.description }}</span>
            </a>
          </section>
        </aside>
      </div>
    </div>
  </section>
</template>

<style scoped>
.section-page {
  --section-color: var(--kind-example);
  flex: 1;
  min-width: 0;
  overflow-y: auto;
  scrollbar-gutter: stable;
  padding: 22px 26px;
}
.section-page.is-api {
  --section-color: var(--kind-summary);
}
.section-page.is-js {
  --section-color: var(--kind-tip);
}
.section-page.is-css {
  --section-color: var(--kind-example);
}
.section-page.is-html {
  --section-color: var(--kind-fact);
}
.section-page.is-http {
  --section-color: #38bdf8;
}
.section-page.is-a11y {
  --section-color: #f472b6;
}
.section-page.is-perf {
  --section-color: #14b8a6;
}
.section-inner {
  display: grid;
  max-width: 1320px;
  gap: 20px;
}
.section-hero {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: end;
  gap: 16px;
  min-height: 168px;
  padding: 24px 0 12px;
  border-bottom: 1px solid #121212;
}
.hero-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  border: 1px solid color-mix(in srgb, var(--section-color) 28%, transparent);
  border-radius: 7px;
  background: color-mix(in srgb, var(--section-color) 8%, transparent);
  color: var(--section-color);
}
.hero-icon-svg {
  width: 20px;
  height: 20px;
}
.hero-copy {
  display: grid;
  min-width: 0;
  gap: 10px;
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
.hero-summary {
  max-width: 680px;
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 12px;
  line-height: 1.7;
}
.hero-actions {
  display: flex;
  align-items: center;
  gap: 9px;
}
.hero-action {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-height: 38px;
  padding: 0 13px;
  border: 1px solid #1a1a1a;
  border-radius: 5px;
  background: #0b0b0b;
  color: #8b9188;
  font-family: var(--font-mono);
  font-size: 11px;
  text-decoration: none;
  transition:
    border-color 0.15s,
    background 0.15s,
    color 0.15s;
}
.hero-action:hover {
  border-color: #252525;
  background: #101010;
  color: #c8c8c0;
}
.hero-action.is-primary {
  border-color: color-mix(in srgb, var(--section-color) 28%, transparent);
  background: color-mix(in srgb, var(--section-color) 8%, transparent);
  color: color-mix(in srgb, var(--section-color) 74%, #d8d8d0);
}
.hero-action-icon {
  width: 13px;
  height: 13px;
}
.stats-row {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 10px;
}
.stat-card {
  display: grid;
  gap: 5px;
  padding: 11px 12px;
  border: 1px solid #151515;
  border-radius: 6px;
  background: #090909;
}
.stat-label {
  color: var(--dt-text-quaternary);
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.2;
}
.stat-value {
  color: #c8c8c0;
  font-family: var(--font-mono);
  font-size: 11px;
  line-height: 1.3;
}
.workspace-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(300px, 0.42fr);
  gap: 18px;
  padding-bottom: 18px;
}
.workspace-main,
.workspace-side,
.content-band {
  display: grid;
  align-content: start;
  gap: 14px;
}
.band-heading {
  display: flex;
  align-items: end;
  justify-content: space-between;
  gap: 12px;
}
.band-title {
  margin-top: 5px;
  color: #d0d0c7;
  font-family: var(--font-display);
  font-size: 20px;
  line-height: 1.2;
  font-weight: 400;
}
.band-note {
  color: var(--dt-text-quaternary);
  font-family: var(--font-mono);
  font-size: 10px;
}
.track-grid,
.cluster-grid {
  display: grid;
  gap: 10px;
}
.cluster-grid {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}
.featured-card {
  display: grid;
  gap: 8px;
  padding: 13px;
  border: 1px solid #161616;
  border-radius: 7px;
  background: #090909;
  color: inherit;
  text-decoration: none;
  transition:
    border-color 0.15s,
    background 0.15s;
}
.featured-card:hover {
  border-color: #222;
  background: #0e0e0e;
}
.featured-category {
  color: #6f756d;
  font-family: var(--font-mono);
  font-size: 9px;
}
.featured-title {
  color: #d0d0c7;
  font-family: var(--font-display);
  font-size: 15px;
  line-height: 1.25;
}
.featured-description {
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 10px;
  line-height: 1.55;
}
@media (max-width: 1100px) {
  .section-hero,
  .workspace-grid {
    grid-template-columns: 1fr;
  }
  .hero-actions {
    justify-self: start;
  }
  .cluster-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
@media (max-width: 720px) {
  .section-page {
    padding: 18px;
  }
  .hero-title {
    font-size: 34px;
  }
  .stats-row,
  .cluster-grid {
    grid-template-columns: 1fr;
  }
}
</style>
