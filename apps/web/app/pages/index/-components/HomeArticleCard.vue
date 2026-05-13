<script setup lang="ts">
import type { HomeArticle } from '../-types'

defineProps<{
  article: HomeArticle
}>()
</script>

<template>
  <a
    class="article-card"
    :href="article.href"
    rel="noreferrer"
    target="_blank"
  >
    <span class="article-category">{{ article.category }}</span>
    <span class="article-title">{{ article.title }}</span>
    <span class="article-description">{{ article.description }}</span>
    <span class="article-link">
      open
      <Icon
        name="lucide:external-link"
        class="article-link-icon"
      />
    </span>
  </a>
</template>

<style scoped>
.article-card {
  display: grid;
  gap: 8px;
  min-width: 0;
  padding: 13px;
  border: 1px solid #151515;
  border-radius: 6px;
  background: #090909;
  color: inherit;
  text-decoration: none;
  transition:
    border-color 0.15s,
    background 0.15s;
}
.article-card:hover {
  border-color: #222;
  background: #0d0d0d;
}
.article-category {
  color: #6f756d;
  font-family: var(--font-mono);
  font-size: 9px;
  line-height: 1.2;
}
.article-title {
  color: #d0d0c7;
  font-family: var(--font-display);
  font-size: 14px;
  line-height: 1.25;
}
.article-description {
  color: var(--dt-text-tertiary);
  font-family: var(--font-mono);
  font-size: 10px;
  line-height: 1.55;
}
.article-link {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  width: max-content;
  color: var(--dt-text-quaternary);
  font-family: var(--font-mono);
  font-size: 9px;
}
.article-card:hover .article-link {
  color: var(--kind-example);
}
.article-link-icon {
  width: 10px;
  height: 10px;
}
</style>
