<script setup lang="ts">
import { ecosystemLinks, featuredArticles, homeSections, recentContributions } from '../-constants'
import HomeArticleCard from './HomeArticleCard.vue'
import HomeLinkList from './HomeLinkList.vue'
import HomeSectionCard from './HomeSectionCard.vue'

const primarySections = computed(() => homeSections.slice(0, 4))
const secondarySections = computed(() => homeSections.slice(4))
</script>

<template>
  <section class="home">
    <div class="home-inner">
      <header class="home-hero">
        <div class="hero-copy">
          <div class="kicker">// web platform workspace</div>
          <h1 class="hero-title">Explore the Web Platform</h1>
          <p class="hero-description">
            A calmer front door for MDN knowledge: choose a domain, continue learning, or jump into
            discovery mode when you want the web to surprise you.
          </p>
        </div>

        <div class="hero-actions">
          <NuxtLink
            class="hero-action is-primary"
            :to="{ name: 'search' }"
          >
            <Icon
              name="lucide:search"
              class="hero-action-icon"
            />
            Search docs
          </NuxtLink>
          <NuxtLink
            class="hero-action"
            :to="{ name: 'feed' }"
          >
            <Icon
              name="lucide:shuffle"
              class="hero-action-icon"
            />
            Open feed
          </NuxtLink>
        </div>
      </header>

      <section class="home-band">
        <div class="section-heading">
          <div>
            <div class="section-kicker">// sections</div>
            <h2 class="section-title">Choose a domain</h2>
          </div>
          <NuxtLink
            class="section-index-link"
            :to="{ name: 'sections' }"
          >
            All sections
            <Icon
              name="lucide:arrow-right"
              class="section-index-link-icon"
            />
          </NuxtLink>
        </div>

        <div class="section-grid primary">
          <HomeSectionCard
            v-for="section in primarySections"
            :key="section.title"
            :section
          />
        </div>
        <div class="section-grid secondary">
          <HomeSectionCard
            v-for="section in secondarySections"
            :key="section.title"
            :section
          />
        </div>
      </section>

      <div class="home-columns">
        <section class="home-band">
          <div class="section-heading">
            <div>
              <div class="section-kicker">// featured</div>
              <h2 class="section-title">Featured articles</h2>
            </div>
          </div>
          <div class="article-list">
            <HomeArticleCard
              v-for="article in featuredArticles"
              :key="article.title"
              :article
            />
          </div>
        </section>

        <section class="home-band">
          <div class="section-heading">
            <div>
              <div class="section-kicker">// updates</div>
              <h2 class="section-title">Recent contributions</h2>
            </div>
          </div>
          <div class="article-list">
            <HomeArticleCard
              v-for="article in recentContributions"
              :key="article.title"
              :article
            />
          </div>
        </section>
      </div>

      <div class="home-footer-grid">
        <section class="continue-panel">
          <div class="section-kicker">// continue</div>
          <h2 class="continue-title">Random learning is still one click away.</h2>
          <p class="continue-copy">
            Feed stays as the place for unexpected topics, keyboard browsing, and fast exploration
            outside a single domain.
          </p>
          <NuxtLink
            class="continue-link"
            :to="{ name: 'feed' }"
          >
            Continue in feed
            <Icon
              name="lucide:arrow-right"
              class="continue-link-icon"
            />
          </NuxtLink>
        </section>

        <div class="ecosystem-panel">
          <HomeLinkList
            v-for="group in ecosystemLinks"
            :key="group.title"
            :group
          />
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.home {
  flex: 1;
  overflow-y: auto;
  scrollbar-gutter: stable;
  min-width: 0;
  padding: 22px 26px;
}
.home-inner {
  display: grid;
  max-width: 1320px;
  gap: 24px;
}
.home-hero {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 24px;
  min-height: 190px;
  padding: 26px 0 8px;
  border-bottom: 1px solid #121212;
}
.hero-copy {
  display: grid;
  max-width: 720px;
  gap: 12px;
}
.kicker,
.section-kicker {
  color: #6f756d;
  font-family: var(--font-mono);
  font-size: 10px;
  line-height: 1.2;
  letter-spacing: 0.08em;
}
.hero-title {
  color: #e1e1d6;
  font-family: var(--font-display);
  font-size: 42px;
  line-height: 1.02;
  font-weight: 400;
}
.hero-description {
  max-width: 620px;
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 12px;
  line-height: 1.7;
}
.hero-actions {
  display: flex;
  align-items: center;
  gap: 9px;
  flex-shrink: 0;
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
  border-color: rgba(126, 183, 124, 0.28);
  background: rgba(126, 183, 124, 0.075);
  color: #a8cba3;
}
.hero-action-icon {
  width: 13px;
  height: 13px;
}
.home-band {
  display: grid;
  gap: 13px;
}
.section-heading {
  display: flex;
  align-items: end;
  justify-content: space-between;
  gap: 14px;
}
.section-title {
  margin-top: 5px;
  color: #d0d0c7;
  font-family: var(--font-display);
  font-size: 20px;
  line-height: 1.2;
  font-weight: 400;
}
.section-index-link {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  min-height: 30px;
  width: max-content;
  padding: 0 10px;
  border: 1px solid rgba(126, 183, 124, 0.24);
  border-radius: 999px;
  background: rgba(126, 183, 124, 0.055);
  color: #a8cba3;
  font-family: var(--font-mono);
  font-size: 10px;
  text-decoration: none;
  transition:
    border-color 0.15s,
    background 0.15s,
    color 0.15s;
}
.section-index-link:hover {
  border-color: rgba(126, 183, 124, 0.38);
  background: rgba(126, 183, 124, 0.095);
  color: #c7e2c2;
}
.section-index-link-icon {
  width: 11px;
  height: 11px;
}
.section-grid {
  display: grid;
  gap: 12px;
}
.section-grid.primary {
  grid-template-columns: repeat(4, minmax(190px, 1fr));
}
.section-grid.secondary {
  grid-template-columns: repeat(3, minmax(190px, 1fr));
}
.home-columns {
  display: grid;
  grid-template-columns: 1.3fr 1fr;
  gap: 18px;
}
.article-list {
  display: grid;
  gap: 9px;
}
.home-footer-grid {
  display: grid;
  grid-template-columns: minmax(280px, 0.9fr) minmax(320px, 1.1fr);
  gap: 18px;
  padding-bottom: 18px;
}
.continue-panel,
.ecosystem-panel {
  border: 1px solid #151515;
  border-radius: 7px;
  background: #090909;
}
.continue-panel {
  display: grid;
  gap: 10px;
  align-content: start;
  padding: 16px;
}
.continue-title {
  color: #d3d3c8;
  font-family: var(--font-display);
  font-size: 19px;
  line-height: 1.25;
  font-weight: 400;
}
.continue-copy {
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 11px;
  line-height: 1.6;
}
.continue-link {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  width: max-content;
  margin-top: 3px;
  color: var(--kind-example);
  font-family: var(--font-mono);
  font-size: 11px;
  text-decoration: none;
}
.continue-link-icon {
  width: 12px;
  height: 12px;
}
.ecosystem-panel {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 18px;
  padding: 16px;
}
@media (max-width: 1180px) {
  .home-hero {
    align-items: flex-start;
    flex-direction: column;
  }
  .section-grid.primary,
  .section-grid.secondary {
    grid-template-columns: repeat(2, minmax(220px, 1fr));
  }
}
@media (max-width: 860px) {
  .home {
    padding: 18px;
  }
  .hero-title {
    font-size: 34px;
  }
  .home-columns,
  .home-footer-grid,
  .ecosystem-panel {
    grid-template-columns: 1fr;
  }
}
@media (max-width: 560px) {
  .section-grid.primary,
  .section-grid.secondary {
    grid-template-columns: 1fr;
  }
  .hero-actions {
    align-items: stretch;
    flex-direction: column;
    width: 100%;
  }
}
</style>
