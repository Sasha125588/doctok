<script setup lang="ts">
import { onClickOutside } from '@vueuse/core'

import {
  type PostsContentLang,
  type UiLang,
  contentLanguageSourceLabels,
  useLang,
} from '~/composables/useLang'

const {
  postsContentLang,
  uiLang,
  contentLanguages,
  uiLanguages,
  currentPostsContentLanguage,
  setPostsContentLang,
  setUiLang,
} = useLang()

const isOpen = shallowRef(false)
const searchQuery = shallowRef('')
const root = useTemplateRef<HTMLElement>('languageSwitcher')

const normalizedQuery = computed(() => searchQuery.value.trim().toLowerCase())
const filteredContentLanguages = computed(() => {
  if (!normalizedQuery.value) return contentLanguages

  return contentLanguages.filter((language) =>
    `${language.label} ${language.nativeName} ${language.code}`
      .toLowerCase()
      .includes(normalizedQuery.value)
  )
})

const toggle = () => (isOpen.value = !isOpen.value)
const close = () => {
  isOpen.value = false
  searchQuery.value = ''
}

const selectContentLang = (value: PostsContentLang) => {
  setPostsContentLang(value)
  close()
}

const selectUiLang = (value: UiLang) => {
  setUiLang(value)
}

onClickOutside(root, close)
</script>

<template>
  <div
    ref="languageSwitcher"
    class="lang-switcher"
  >
    <button
      class="lang-trigger"
      type="button"
      aria-haspopup="dialog"
      :aria-expanded="isOpen"
      @click="toggle"
      @keydown.esc="close"
    >
      <Icon
        name="lucide:languages"
        class="trigger-icon"
        aria-hidden="true"
      />
      <span class="trigger-label">{{ currentPostsContentLanguage.shortLabel }}</span>
      <Icon
        name="lucide:chevron-down"
        class="chevron"
        aria-hidden="true"
      />
    </button>

    <div
      v-if="isOpen"
      class="lang-popover"
      role="dialog"
      aria-label="Language preferences"
      @keydown.esc="close"
    >
      <div class="popover-header">
        <div>
          <p class="eyebrow">Language</p>
          <h2 class="popover-title">Content language</h2>
        </div>
        <span class="fallback-note">English fallback</span>
      </div>

      <label class="search">
        <Icon
          name="lucide:search"
          class="search-icon"
          aria-hidden="true"
        />
        <input
          v-model="searchQuery"
          type="search"
          placeholder="Search language"
        />
      </label>

      <div class="language-list">
        <button
          v-for="language in filteredContentLanguages"
          :key="language.code"
          class="language-row"
          :class="{ 'is-active': postsContentLang === language.code }"
          type="button"
          @click="selectContentLang(language.code)"
        >
          <span class="check">
            <Icon
              v-if="postsContentLang === language.code"
              name="lucide:check"
              class="check-icon"
              aria-hidden="true"
            />
          </span>
          <span class="language-copy">
            <span class="native-name">{{ language.nativeName }}</span>
            <span class="english-name">{{ language.label }}</span>
          </span>
          <span
            class="source-badge"
            :class="`is-${language.source}`"
          >
            {{ contentLanguageSourceLabels[language.source] }}
          </span>
        </button>
      </div>

      <div class="ui-section">
        <div class="ui-section-heading">
          <span>UI language</span>
          <span class="ui-hint">Interface labels</span>
        </div>
        <div class="ui-options">
          <button
            v-for="language in uiLanguages"
            :key="language.code"
            class="ui-option"
            :class="{ 'is-active': uiLang === language.code }"
            type="button"
            @click="selectUiLang(language.code)"
          >
            {{ language.shortLabel }}
          </button>
        </div>
      </div>

      <NuxtLink
        class="settings-link"
        :to="{ name: 'settings' }"
        @click="close"
      >
        <span>Manage in Settings</span>
        <Icon
          name="lucide:arrow-right"
          class="settings-icon"
          aria-hidden="true"
        />
      </NuxtLink>
    </div>
  </div>
</template>

<style scoped>
.lang-switcher {
  position: relative;
  font-family: var(--font-mono);
  flex-shrink: 0;
}
.lang-trigger {
  height: 30px;
  border: 1px solid #171717;
  border-radius: 5px;
  background: color-mix(in oklab, var(--dt-card-bg) 76%, transparent);
  color: var(--dt-text-tertiary);
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 0 8px;
  cursor: pointer;
  transition:
    border-color 0.15s ease,
    background 0.15s ease,
    color 0.15s ease;
}
.lang-trigger:hover,
.lang-trigger[aria-expanded='true'] {
  border-color: color-mix(in oklab, var(--kind-example) 22%, #171717);
  background: color-mix(in oklab, var(--dt-card-bg) 92%, transparent);
  color: var(--dt-text-secondary);
}
.trigger-icon,
.chevron {
  width: 13px;
  height: 13px;
}
.trigger-label {
  min-width: 18px;
  font-size: 10px;
  line-height: 1;
  text-align: center;
  color: var(--kind-example);
}
.chevron {
  color: var(--dt-text-quaternary);
}
.lang-popover {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  z-index: 80;
  width: min(360px, calc(100vw - 74px));
  border: 1px solid #1b1d1f;
  border-radius: 8px;
  background: #0b0d0e;
  box-shadow: 0 18px 60px rgba(0, 0, 0, 0.45);
  padding: 12px;
}
.popover-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 10px;
}
.eyebrow {
  color: var(--dt-text-quaternary);
  font-size: 9px;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}
.popover-title {
  color: var(--foreground);
  font-size: 13px;
  font-weight: 500;
}
.fallback-note {
  border: 1px solid #1a1f24;
  border-radius: 999px;
  color: var(--dt-text-quaternary);
  font-size: 8px;
  padding: 4px 7px;
  white-space: nowrap;
}
.search {
  height: 34px;
  border: 1px solid #171a1d;
  border-radius: 5px;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 9px;
  margin-bottom: 8px;
  background: #080909;
}
.search-icon {
  width: 13px;
  height: 13px;
  color: var(--dt-text-quaternary);
}
.search input {
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--foreground);
  font-family: var(--font-mono);
  font-size: 11px;
}
.search input::placeholder {
  color: var(--dt-text-quaternary);
}
.language-list {
  max-height: 304px;
  display: grid;
  gap: 3px;
  overflow: auto;
  padding-right: 2px;
}
.language-row {
  min-height: 42px;
  border: 1px solid transparent;
  border-radius: 5px;
  background: transparent;
  display: grid;
  grid-template-columns: 18px minmax(0, 1fr) auto;
  align-items: center;
  gap: 8px;
  padding: 7px 8px;
  color: var(--dt-text-tertiary);
  cursor: pointer;
  text-align: left;
  transition:
    background 0.15s ease,
    border-color 0.15s ease,
    color 0.15s ease;
}
.language-row:hover {
  background: rgba(255, 255, 255, 0.025);
  color: var(--dt-text-secondary);
}
.language-row.is-active {
  border-color: color-mix(in oklab, var(--kind-example) 24%, transparent);
  background: color-mix(in oklab, var(--dt-rail-active-bg) 74%, transparent);
}
.check {
  display: flex;
  justify-content: center;
}
.check-icon {
  width: 13px;
  height: 13px;
  color: var(--kind-example);
}
.language-copy {
  min-width: 0;
  display: grid;
  gap: 2px;
}
.native-name {
  color: var(--foreground);
  font-size: 11px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.english-name {
  color: var(--dt-text-quaternary);
  font-size: 9px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.source-badge {
  border: 1px solid transparent;
  border-radius: 999px;
  font-size: 8px;
  padding: 3px 6px;
  white-space: nowrap;
}
.source-badge.is-official {
  border-color: color-mix(in oklab, var(--kind-summary) 22%, transparent);
  color: #8dbbff;
}
.source-badge.is-aiGenerated {
  border-color: color-mix(in oklab, var(--kind-concept) 24%, transparent);
  color: #c494ff;
}
.ui-section {
  border-top: 1px solid #171a1d;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-top: 10px;
  padding-top: 10px;
}
.ui-section-heading {
  display: grid;
  gap: 2px;
  color: var(--dt-text-tertiary);
  font-size: 10px;
}
.ui-hint {
  color: var(--dt-text-quaternary);
  font-size: 8px;
}
.ui-options {
  border: 1px solid #171717;
  border-radius: 5px;
  display: flex;
  overflow: hidden;
}
.ui-option {
  min-width: 30px;
  height: 24px;
  border: 0;
  border-right: 1px solid #171717;
  background: transparent;
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 9px;
  cursor: pointer;
}
.ui-option:last-child {
  border-right: 0;
}
.ui-option.is-active {
  background: var(--dt-rail-active-bg);
  color: var(--kind-example);
}
.settings-link {
  height: 34px;
  border: 1px solid #171a1d;
  border-radius: 5px;
  color: var(--dt-text-tertiary);
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 10px;
  padding: 0 10px;
  font-size: 10px;
  text-decoration: none;
  transition:
    background 0.15s ease,
    color 0.15s ease;
}
.settings-link:hover {
  background: rgba(255, 255, 255, 0.025);
  color: var(--foreground);
}
.settings-icon {
  width: 13px;
  height: 13px;
}
</style>
