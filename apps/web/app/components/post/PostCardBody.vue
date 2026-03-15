<script setup lang="ts">
const props = defineProps<{ body: string }>()

function normalizeTopicHref(href: string) {
  if (href.startsWith('/topic/')) {
    return href
  }

  if (href.startsWith('mdn/')) {
    return `/topic/${href}`
  }

  return href
}

function normalizeTopicLinks(body: string) {
  return body
    .replace(/\]\((mdn:[^)]+)\)/g, (_, href: string) => `](${normalizeTopicHref(href)})`)
    .replace(/\]\((mdn\/[^)]+)\)/g, (_, href: string) => `](${normalizeTopicHref(href)})`)
    .replace(/href=(['"])(mdn:[^'"]+)\1/g, (_, quote: string, href: string) => {
      return `href=${quote}${normalizeTopicHref(href)}${quote}`
    })
    .replace(/href=(['"])(mdn\/[^'"]+)\1/g, (_, quote: string, href: string) => {
      return `href=${quote}${normalizeTopicHref(href)}${quote}`
    })
}

const renderedBody = computed(() => {
  const trimmed = props.body.trim()
  const isCodeBlock = trimmed.startsWith('<') && /<\/?[a-z][\s\S]*>/i.test(trimmed)

  if (isCodeBlock) {
    return `\`\`\`html\n${trimmed}\n\`\`\``
  }

  return normalizeTopicLinks(trimmed)
})

function handleClick(e: MouseEvent) {
  const target = e.target as HTMLElement
  const anchor = target.closest('a')
  if (!anchor) return

  const href = anchor.getAttribute('href')
  if (!href) return

  const targetHref = normalizeTopicHref(href)
  if (targetHref === href) {
    return
  }

  e.preventDefault()
  navigateTo(targetHref)
}
</script>

<template>
  <div
    class="post-body max-h-[38vh] overflow-y-auto font-mono text-[clamp(0.82rem,2.5vw,0.92rem)] leading-[1.72] text-[rgba(240,244,248,0.78)]"
    @click="handleClick"
  >
    <MDC :value="renderedBody" />
  </div>
</template>

<style scoped>
.post-body {
  scrollbar-width: none;
  -webkit-overflow-scrolling: touch;
}
.post-body::-webkit-scrollbar {
  display: none;
}
.post-body :deep(h1),
.post-body :deep(h2),
.post-body :deep(h3) {
  font-family: var(--font-display);
  font-weight: 700;
  letter-spacing: -0.02em;
  margin-top: 1.2em;
  margin-bottom: 0.5em;
  color: var(--foreground);
}
.post-body :deep(h1) {
  font-size: 1.15rem;
}
.post-body :deep(h2) {
  font-size: 1.05rem;
}
.post-body :deep(h3) {
  font-size: 0.95rem;
}
.post-body :deep(p) {
  margin-bottom: 0.75em;
}
.post-body :deep(ul),
.post-body :deep(ol) {
  padding-left: 1.5em;
  margin-bottom: 0.75em;
}
.post-body :deep(li) {
  margin-bottom: 0.25em;
}
.post-body :deep(code) {
  background: rgba(255, 255, 255, 0.07);
  padding: 1px 5px;
  border-radius: 3px;
  font-size: 0.88em;
  color: var(--foreground);
}
.post-body :deep(pre) {
  background: rgba(0, 0, 0, 0.4);
  padding: 1em;
  border-radius: 6px;
  overflow-x: auto;
  margin-bottom: 0.75em;
  border: 1px solid rgba(255, 255, 255, 0.05);
}
.post-body :deep(pre code) {
  background: none;
  padding: 0;
}
.post-body :deep(a) {
  color: var(--kind-summary);
  text-decoration: underline;
  text-underline-offset: 2px;
}
.post-body :deep(a:hover) {
  opacity: 0.8;
}
</style>
