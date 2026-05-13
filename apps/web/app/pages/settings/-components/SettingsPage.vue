<script setup lang="ts">
import { contentLanguageSourceLabels, uiLanguageSourceLabels, useLang } from '~/composables/useLang'

import type { PostsContentLang, UiLang } from '~/composables/useLang'

const {
  postsContentLang,
  uiLang,
  contentLanguages,
  uiLanguages,
  currentPostsContentLanguage,
  currentUiLanguage,
  setPostsContentLang,
  setUiLang,
} = useLang()

const onContentLanguageChange = (event: Event) => {
  setPostsContentLang((event.target as HTMLSelectElement).value as PostsContentLang)
}

const onUiLanguageChange = (event: Event) => {
  setUiLang((event.target as HTMLSelectElement).value as UiLang)
}
</script>

<template>
  <section class="settings-page">
    <div class="settings-inner">
      <header class="settings-hero">
        <div class="kicker">// preferences</div>
        <h1 class="hero-title">Settings</h1>
        <p class="hero-description">
          Control how Doctok reads, displays, and personalizes MDN knowledge.
        </p>
      </header>

      <section class="settings-band">
        <div class="section-heading">
          <div>
            <div class="section-kicker">// language</div>
            <h2 class="section-title">Language</h2>
          </div>
        </div>

        <div class="setting-row">
          <div class="setting-copy">
            <h3>Content language</h3>
            <p>Articles, feed topics, saved posts, and related links.</p>
          </div>

          <div class="setting-control">
            <div class="select-box">
              <select
                :value="postsContentLang"
                @change="onContentLanguageChange"
              >
                <option
                  v-for="language in contentLanguages"
                  :key="language.code"
                  :value="language.code"
                >
                  {{ language.nativeName }} - {{ contentLanguageSourceLabels[language.source] }}
                </option>
              </select>
              <Icon
                name="lucide:chevron-down"
                class="select-icon"
                aria-hidden="true"
              />
            </div>

            <span
              class="source-badge"
              :class="`is-${currentPostsContentLanguage.source}`"
            >
              {{ contentLanguageSourceLabels[currentPostsContentLanguage.source] }}
            </span>
          </div>
        </div>

        <p class="setting-note">Missing content falls back to English.</p>

        <div class="setting-row">
          <div class="setting-copy">
            <h3>UI language</h3>
            <p>Navigation, controls, settings labels, and app copy.</p>
          </div>

          <div class="setting-control">
            <div class="select-box">
              <select
                :value="uiLang"
                @change="onUiLanguageChange"
              >
                <option
                  v-for="language in uiLanguages"
                  :key="language.code"
                  :value="language.code"
                >
                  {{ language.nativeName }} - {{ uiLanguageSourceLabels[language.source] }}
                </option>
              </select>
              <Icon
                name="lucide:chevron-down"
                class="select-icon"
                aria-hidden="true"
              />
            </div>

            <span
              class="source-badge"
              :class="`is-${currentUiLanguage.source}`"
            >
              {{ uiLanguageSourceLabels[currentUiLanguage.source] }}
            </span>
          </div>
        </div>
      </section>
    </div>
  </section>
</template>

<style scoped>
.settings-page {
  flex: 1;
  min-width: 0;
  overflow-y: auto;
  scrollbar-gutter: stable;
  padding: 22px 26px;
  color: var(--foreground);
}
.settings-inner {
  display: grid;
  max-width: 980px;
  gap: 24px;
}
.settings-hero {
  display: grid;
  max-width: 700px;
  gap: 12px;
  min-height: 170px;
  align-content: end;
  padding: 24px 0 10px;
  border-bottom: 1px solid #1b1d1a;
}
.kicker,
.section-kicker {
  color: #8a9486;
  font-family: var(--font-mono);
  font-size: 10px;
  line-height: 1.2;
  letter-spacing: 0.08em;
}
.hero-title {
  color: #f0f0e5;
  font-family: var(--font-display);
  font-size: 42px;
  line-height: 1.02;
  font-weight: 400;
}
.hero-description {
  max-width: 620px;
  color: #9ba39a;
  font-family: var(--font-mono);
  font-size: 12px;
  line-height: 1.7;
}
.settings-band {
  display: grid;
  gap: 12px;
  border: 1px solid #242721;
  border-radius: 8px;
  background: #0f1110;
  padding: 17px;
}
.section-heading {
  display: flex;
  align-items: end;
  justify-content: space-between;
  gap: 14px;
  margin-bottom: 2px;
}
.section-title {
  margin-top: 5px;
  color: #e1e1d6;
  font-family: var(--font-display);
  font-size: 20px;
  line-height: 1.2;
  font-weight: 400;
}
.setting-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(300px, 380px);
  align-items: center;
  gap: 18px;
  min-height: 78px;
  padding: 14px;
  border: 1px solid #20231e;
  border-radius: 7px;
  background: #111312;
}
.setting-copy {
  min-width: 0;
}
.setting-copy h3 {
  color: #ededdf;
  font-family: var(--font-display);
  font-size: 16px;
  font-weight: 400;
}
.setting-copy p,
.setting-note {
  color: #919991;
  font-family: var(--font-mono);
  font-size: 11px;
  line-height: 1.6;
}
.setting-copy p {
  max-width: 440px;
  margin-top: 5px;
}
.setting-note {
  margin: -2px 14px 2px;
  color: #7f877f;
}
.setting-control {
  align-items: center;
  display: flex;
  gap: 10px;
}
.select-box {
  position: relative;
  min-width: 280px;
}
.select-box select {
  width: 100%;
  height: 42px;
  appearance: none;
  border: 1px solid #2a2e28;
  border-radius: 6px;
  background: #090b0a;
  color: #eeeedf;
  font-family: var(--font-mono);
  font-size: 11px;
  outline: none;
  padding: 0 36px 0 11px;
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.025);
  transition:
    border-color 0.15s,
    background 0.15s,
    color 0.15s;
}
.select-box select:hover,
.select-box select:focus {
  border-color: rgba(126, 183, 124, 0.46);
  background: #0b100d;
  color: #f5f5e9;
}
.select-icon {
  position: absolute;
  right: 12px;
  top: 50%;
  width: 14px;
  height: 14px;
  color: #8f988e;
  pointer-events: none;
  transform: translateY(-50%);
}
.source-badge {
  min-height: 28px;
  border: 1px solid transparent;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-family: var(--font-mono);
  font-size: 8px;
  padding: 0 9px;
  white-space: nowrap;
}
.source-badge.is-official {
  border-color: color-mix(in oklab, var(--kind-summary) 36%, transparent);
  background: color-mix(in oklab, var(--kind-summary) 8%, transparent);
  color: #a8caff;
}
.source-badge.is-aiGenerated {
  border-color: color-mix(in oklab, var(--kind-concept) 40%, transparent);
  background: color-mix(in oklab, var(--kind-concept) 9%, transparent);
  color: #d2aaff;
}
.source-badge.is-community {
  border-color: color-mix(in oklab, var(--kind-example) 38%, transparent);
  background: color-mix(in oklab, var(--kind-example) 8%, transparent);
  color: #b7dcb0;
}
@media (max-width: 720px) {
  .settings-page {
    padding: 18px;
  }
  .settings-inner {
    max-width: none;
  }
  .settings-hero {
    min-height: 150px;
  }
  .hero-title {
    font-size: 34px;
  }
  .setting-row,
  .setting-control {
    grid-template-columns: 1fr;
  }
}
</style>
