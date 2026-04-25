<script setup lang="ts">
import { VueFlow } from '@vue-flow/core'

import type { Edge, Node } from '@vue-flow/core'
import '@vue-flow/core/dist/style.css'

type Lang = 'html' | 'css' | 'js' | 'api'
type State = 'done' | 'current' | 'unlocked' | 'locked'

interface TopicData {
  label: string
  state: State
  lang: Lang
  description: string
  cards: number
  time: string
}

type TopicNode = Node<TopicData>

const LANG_COLORS: Record<Lang, string> = {
  html: '#ff9a3c',
  css: '#6ab4ff',
  js: '#ffd93c',
  api: '#a78bfa',
}

const LANG_LABELS: Record<Lang, string> = {
  html: 'HTML',
  css: 'CSS',
  js: 'JavaScript',
  api: 'Web APIs',
}

const nodes = ref<TopicNode[]>([
  // HTML — top-left
  {
    id: 'html5_basics',
    type: 'topic',
    position: { x: 120, y: 100 },
    data: {
      label: 'html5_basics',
      state: 'done',
      lang: 'html',
      description: 'Базова структура документа, теги, атрибути.',
      cards: 6,
      time: '~ 15 хв',
    },
  },
  {
    id: 'semantic_html',
    type: 'topic',
    position: { x: 40, y: 200 },
    data: {
      label: 'semantic_html',
      state: 'locked',
      lang: 'html',
      description: 'Семантичні теги: header, main, article, section.',
      cards: 5,
      time: '~ 12 хв',
    },
  },
  {
    id: 'forms',
    type: 'topic',
    position: { x: 200, y: 210 },
    data: {
      label: 'forms',
      state: 'done',
      lang: 'html',
      description: 'Форми, input, валідація на рівні HTML.',
      cards: 7,
      time: '~ 18 хв',
    },
  },
  {
    id: 'accessibility',
    type: 'topic',
    position: { x: 290, y: 120 },
    data: {
      label: 'accessibility',
      state: 'unlocked',
      lang: 'html',
      description: 'ARIA, keyboard nav, screen readers.',
      cards: 8,
      time: '~ 20 хв',
    },
  },

  // CSS — top-right
  {
    id: 'selectors',
    type: 'topic',
    position: { x: 520, y: 130 },
    data: {
      label: 'selectors',
      state: 'done',
      lang: 'css',
      description: 'Селектори, специфічність, каскад.',
      cards: 6,
      time: '~ 15 хв',
    },
  },
  {
    id: 'box_model',
    type: 'topic',
    position: { x: 600, y: 210 },
    data: {
      label: 'box_model',
      state: 'done',
      lang: 'css',
      description: 'Padding, border, margin, box-sizing.',
      cards: 5,
      time: '~ 12 хв',
    },
  },
  {
    id: 'flexbox',
    type: 'topic',
    position: { x: 700, y: 200 },
    data: {
      label: 'flexbox',
      state: 'done',
      lang: 'css',
      description: 'Flex-container і flex-items, осі, вирівнювання.',
      cards: 8,
      time: '~ 20 хв',
    },
  },
  {
    id: 'css_grid',
    type: 'topic',
    position: { x: 780, y: 120 },
    data: {
      label: 'css_grid',
      state: 'done',
      lang: 'css',
      description: 'Grid template, tracks, areas.',
      cards: 9,
      time: '~ 24 хв',
    },
  },
  {
    id: 'animations',
    type: 'topic',
    position: { x: 700, y: 60 },
    data: {
      label: 'animations',
      state: 'locked',
      lang: 'css',
      description: 'Transition, keyframes, timing functions.',
      cards: 6,
      time: '~ 15 хв',
    },
  },
  {
    id: 'css_subgrid',
    type: 'topic',
    position: { x: 870, y: 170 },
    data: {
      label: 'css_subgrid',
      state: 'locked',
      lang: 'css',
      description: 'Subgrid для вкладених сіток.',
      cards: 4,
      time: '~ 10 хв',
    },
  },

  // JS — bottom-left
  {
    id: 'variables',
    type: 'topic',
    position: { x: 80, y: 380 },
    data: {
      label: 'variables',
      state: 'done',
      lang: 'js',
      description: 'let, const, var, типи і scope.',
      cards: 5,
      time: '~ 12 хв',
    },
  },
  {
    id: 'functions',
    type: 'topic',
    position: { x: 180, y: 440 },
    data: {
      label: 'functions',
      state: 'done',
      lang: 'js',
      description: 'Function declarations, arrows, closures.',
      cards: 7,
      time: '~ 18 хв',
    },
  },
  {
    id: 'array_methods',
    type: 'topic',
    position: { x: 280, y: 380 },
    data: {
      label: 'array_methods',
      state: 'done',
      lang: 'js',
      description: 'map, filter, reduce, forEach.',
      cards: 8,
      time: '~ 20 хв',
    },
  },
  {
    id: 'promises',
    type: 'topic',
    position: { x: 300, y: 500 },
    data: {
      label: 'promises',
      state: 'done',
      lang: 'js',
      description: 'Promise, then/catch, ланцюги.',
      cards: 8,
      time: '~ 22 хв',
    },
  },
  {
    id: 'async_await',
    type: 'topic',
    position: { x: 400, y: 450 },
    data: {
      label: 'async_await',
      state: 'unlocked',
      lang: 'js',
      description: 'Синтаксичний цукор над Promise.',
      cards: 6,
      time: '~ 16 хв',
    },
  },
  {
    id: 'event_loop',
    type: 'topic',
    position: { x: 420, y: 560 },
    data: {
      label: 'event_loop',
      state: 'locked',
      lang: 'js',
      description: 'Call stack, microtasks, macrotasks.',
      cards: 7,
      time: '~ 18 хв',
    },
  },

  // APIs — bottom-right
  {
    id: 'fetch_api',
    type: 'topic',
    position: { x: 560, y: 410 },
    data: {
      label: 'fetch_api',
      state: 'unlocked',
      lang: 'api',
      description: 'Fetch, Request, Response, AbortController.',
      cards: 7,
      time: '~ 18 хв',
    },
  },
  {
    id: 'intersection_observer',
    type: 'topic',
    position: { x: 680, y: 470 },
    data: {
      label: 'intersection_observer',
      state: 'current',
      lang: 'api',
      description: 'IO API для lazy-loading і infinite scroll.',
      cards: 8,
      time: '~ 20 хв',
    },
  },
  {
    id: 'localstorage',
    type: 'topic',
    position: { x: 600, y: 340 },
    data: {
      label: 'localstorage',
      state: 'unlocked',
      lang: 'api',
      description: 'localStorage, sessionStorage, serialization.',
      cards: 5,
      time: '~ 12 хв',
    },
  },
  {
    id: 'web_components',
    type: 'topic',
    position: { x: 790, y: 400 },
    data: {
      label: 'web_components',
      state: 'locked',
      lang: 'api',
      description: 'Custom elements, shadow DOM, slots.',
      cards: 9,
      time: '~ 25 хв',
    },
  },
])

const edges = ref<Edge[]>([
  // HTML internal
  { id: 'e-html-sem', source: 'html5_basics', target: 'semantic_html' },
  { id: 'e-html-forms', source: 'html5_basics', target: 'forms' },
  { id: 'e-html-a11y', source: 'html5_basics', target: 'accessibility' },
  { id: 'e-sem-a11y', source: 'semantic_html', target: 'accessibility' },

  // CSS internal
  { id: 'e-sel-box', source: 'selectors', target: 'box_model' },
  { id: 'e-box-flex', source: 'box_model', target: 'flexbox' },
  { id: 'e-box-grid', source: 'box_model', target: 'css_grid' },
  { id: 'e-flex-anim', source: 'flexbox', target: 'animations' },
  { id: 'e-grid-sub', source: 'css_grid', target: 'css_subgrid' },

  // JS internal
  { id: 'e-var-fn', source: 'variables', target: 'functions' },
  { id: 'e-fn-arr', source: 'functions', target: 'array_methods' },
  { id: 'e-arr-prom', source: 'array_methods', target: 'promises' },
  { id: 'e-prom-async', source: 'promises', target: 'async_await' },
  { id: 'e-prom-loop', source: 'promises', target: 'event_loop' },

  // APIs internal
  { id: 'e-fetch-io', source: 'fetch_api', target: 'intersection_observer' },
  { id: 'e-fetch-ls', source: 'fetch_api', target: 'localstorage' },

  // Cross-language bridges
  { id: 'e-html-sel', source: 'html5_basics', target: 'selectors' },
  { id: 'e-arr-fetch', source: 'array_methods', target: 'fetch_api' },
  { id: 'e-grid-wc', source: 'css_grid', target: 'web_components' },
  { id: 'e-async-io', source: 'async_await', target: 'intersection_observer' },
])

const selectedId = ref<string | null>('intersection_observer')
const selectedNode = computed(() => nodes.value.find((n) => n.id === selectedId.value) ?? null)

const onNodeClick = (event: { node: TopicNode }) => {
  selectedId.value = event.node.id
}

const closePanel = () => {
  selectedId.value = null
}

const onKeydown = (e: KeyboardEvent) => {
  if (e.key === 'Escape' && selectedId.value) {
    e.preventDefault()
    closePanel()
  }
}

onMounted(() => window.addEventListener('keydown', onKeydown))
onBeforeUnmount(() => window.removeEventListener('keydown', onKeydown))

const counts = computed(() => {
  const acc = { done: 0, current: 0, unlocked: 0, locked: 0 }
  for (const n of nodes.value) acc[n.data!.state]++
  return acc
})

const total = computed(() => nodes.value.length)
const discovered = computed(() => counts.value.done + counts.value.current + counts.value.unlocked)
</script>

<template>
  <section id="map-page">
    <div id="map-header">
      <div id="map-header-left">
        <span id="map-title">// карта знань</span>
        <span id="map-subtitle">{{ discovered }} / {{ total }} тем відкрито</span>
      </div>
      <div id="map-legend">
        <div
          v-for="lang in ['html', 'css', 'js', 'api'] as Lang[]"
          :key="lang"
          class="map-legend-item"
        >
          <div
            class="map-legend-dot"
            :style="{ background: LANG_COLORS[lang] }"
          />
          <span class="map-legend-label">{{ LANG_LABELS[lang] }}</span>
        </div>
      </div>
    </div>

    <div id="map-canvas-wrap">
      <VueFlow
        :nodes="nodes"
        :edges="edges"
        :nodes-draggable="false"
        :nodes-connectable="false"
        :elements-selectable="true"
        :zoom-on-double-click="false"
        :default-viewport="{ x: 0, y: 0, zoom: 1 }"
        :min-zoom="0.4"
        :max-zoom="2"
        :fit-view-on-init="true"
        :pan-on-drag="true"
        class="map-flow"
        @node-click="onNodeClick"
      >
        <template #node-topic="{ data, id: nodeId }">
          <div
            class="map-node"
            :class="[`state-${data.state}`, { 'is-selected': nodeId === selectedId }]"
            :style="{ '--lang-color': LANG_COLORS[data.lang as Lang] }"
          >
            <div class="map-node-dot" />
            <div class="map-node-label">
              {{ data.label }}
            </div>
          </div>
        </template>
      </VueFlow>

      <aside
        v-if="selectedNode"
        id="map-panel"
      >
        <div class="map-panel-head">
          <span
            class="map-panel-lang"
            :style="{ color: LANG_COLORS[selectedNode.data!.lang] }"
          >
            {{ LANG_LABELS[selectedNode.data!.lang] }}
          </span>
          <button
            class="map-panel-close"
            @click="closePanel"
          >
            [esc]
          </button>
        </div>

        <div class="map-panel-name">
          {{ selectedNode.data!.label }}
        </div>

        <div
          class="map-panel-state"
          :class="`state-${selectedNode.data!.state}`"
        >
          <span class="map-panel-state-dot" />
          <span>{{
            selectedNode.data!.state === 'done'
              ? 'пройдено'
              : selectedNode.data!.state === 'current'
                ? 'вивчаєш зараз'
                : selectedNode.data!.state === 'unlocked'
                  ? 'розблоковано'
                  : 'заблоковано'
          }}</span>
        </div>

        <p class="map-panel-desc">
          {{ selectedNode.data!.description }}
        </p>

        <div class="map-panel-meta">
          <span>{{ selectedNode.data!.cards }} карток</span>
          <span class="map-panel-sep">·</span>
          <span>{{ selectedNode.data!.time }}</span>
        </div>

        <button
          v-if="selectedNode.data!.state !== 'locked'"
          class="map-panel-cta"
        >
          <span class="map-cta-key">↵</span>
          {{
            selectedNode.data!.state === 'current'
              ? 'продовжити'
              : selectedNode.data!.state === 'done'
                ? 'переглянути'
                : 'відкрити'
          }}
        </button>
        <div
          v-else
          class="map-panel-locked"
        >
          // розблокуй попередні теми
        </div>
      </aside>
    </div>
  </section>
</template>

<style scoped>
#map-page {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: #040404;
  font-family: 'SF Mono', 'Fira Code', 'Consolas', 'Menlo', monospace;
  overflow: hidden;
  min-width: 0;
}
#map-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 28px;
  border-bottom: 1px solid #111;
  flex-shrink: 0;
}
#map-header-left {
  display: flex;
  align-items: baseline;
  gap: 14px;
}
#map-title {
  font-size: 11px;
  color: #555;
  letter-spacing: 0.14em;
}
#map-subtitle {
  font-size: 10px;
  color: #2a2a2a;
  letter-spacing: 0.06em;
}
#map-legend {
  display: flex;
  gap: 16px;
}
.map-legend-item {
  display: flex;
  align-items: center;
  gap: 6px;
}
.map-legend-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  box-shadow: 0 0 6px currentColor;
  opacity: 0.8;
}
.map-legend-label {
  font-size: 10px;
  color: #555;
  letter-spacing: 0.06em;
}
#map-canvas-wrap {
  flex: 1;
  position: relative;
  overflow: hidden;
  min-height: 0;
  background:
    radial-gradient(ellipse at 30% 20%, #0a0f1a 0%, transparent 55%),
    radial-gradient(ellipse at 70% 80%, #0a0a14 0%, transparent 55%), #040404;
}
.map-flow {
  width: 100%;
  height: 100%;
}

/* Node styles */
:deep(.vue-flow__node) {
  cursor: pointer;
}
:deep(.vue-flow__node-topic) {
  background: transparent;
  border: none;
  padding: 0;
  width: auto;
  box-shadow: none;
}
:deep(.vue-flow__handle) {
  opacity: 0;
  pointer-events: none;
}
.map-node {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding: 4px;
  transition: transform 0.15s;
  user-select: none;
}
.map-node:hover {
  transform: scale(1.15);
}
.map-node-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: var(--lang-color);
  opacity: 0.2;
  transition: all 0.2s;
}
.map-node-label {
  font-size: 9px;
  color: #333;
  letter-spacing: 0.04em;
  white-space: nowrap;
  transition: color 0.2s;
}

/* Node states */
.map-node.state-done .map-node-dot {
  opacity: 0.9;
  box-shadow: 0 0 8px var(--lang-color);
}
.map-node.state-done .map-node-label {
  color: #888;
}
.map-node.state-current .map-node-dot {
  opacity: 1;
  width: 14px;
  height: 14px;
  box-shadow:
    0 0 14px var(--lang-color),
    0 0 30px var(--lang-color);
  animation: map-pulse 1.6s ease-in-out infinite;
}
.map-node.state-current .map-node-label {
  color: var(--lang-color);
  font-weight: 600;
}
.map-node.state-unlocked .map-node-dot {
  opacity: 0.55;
  background: transparent;
  border: 1.5px solid var(--lang-color);
  box-shadow: 0 0 6px var(--lang-color);
}
.map-node.state-unlocked .map-node-label {
  color: #666;
}
.map-node.state-locked .map-node-dot {
  opacity: 0.25;
  background: #2a2a2a;
}
.map-node.state-locked .map-node-label {
  color: #2a2a2a;
}
.map-node.is-selected .map-node-label {
  color: #eeece4;
}
.map-node.is-selected .map-node-dot {
  transform: scale(1.2);
}

@keyframes map-pulse {
  0%,
  100% {
    box-shadow:
      0 0 14px var(--lang-color),
      0 0 30px var(--lang-color);
  }
  50% {
    box-shadow:
      0 0 18px var(--lang-color),
      0 0 42px var(--lang-color);
  }
}

/* Edge styling */
:deep(.vue-flow__edge-path) {
  stroke: #1a1a1a;
  stroke-width: 1;
  fill: none;
}
:deep(.vue-flow__edge) {
  pointer-events: none;
}

/* Remove vue-flow default attribution */
:deep(.vue-flow__panel.bottom.right) {
  display: none;
}

/* Side panel */
#map-panel {
  position: absolute;
  top: 20px;
  right: 20px;
  width: 280px;
  background: #080808;
  border: 1px solid #1a1a1a;
  border-radius: 8px;
  padding: 18px 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  z-index: 10;
}
.map-panel-head {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
}
.map-panel-lang {
  font-size: 10px;
  letter-spacing: 0.1em;
}
.map-panel-close {
  background: transparent;
  border: none;
  color: #333;
  font-family: inherit;
  font-size: 10px;
  cursor: pointer;
  padding: 0;
  letter-spacing: 0.04em;
}
.map-panel-close:hover {
  color: #888;
}
.map-panel-name {
  font-size: 16px;
  color: #eeece4;
  letter-spacing: 0.01em;
}
.map-panel-state {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 10px;
  letter-spacing: 0.04em;
}
.map-panel-state-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
}
.map-panel-state.state-done {
  color: #3a8a50;
}
.map-panel-state.state-done .map-panel-state-dot {
  background: #00e87a;
}
.map-panel-state.state-current {
  color: #00e87a;
}
.map-panel-state.state-current .map-panel-state-dot {
  background: #00e87a;
  box-shadow: 0 0 6px #00e87a;
}
.map-panel-state.state-unlocked {
  color: #888;
}
.map-panel-state.state-unlocked .map-panel-state-dot {
  border: 1px solid #888;
}
.map-panel-state.state-locked {
  color: #444;
}
.map-panel-state.state-locked .map-panel-state-dot {
  background: #333;
}
.map-panel-desc {
  font-size: 11px;
  color: #666;
  line-height: 1.6;
}
.map-panel-meta {
  display: flex;
  gap: 6px;
  font-size: 9px;
  color: #2a2a2a;
  letter-spacing: 0.06em;
}
.map-panel-sep {
  opacity: 0.5;
}
.map-panel-cta {
  background: transparent;
  border: 1px solid #00e87a44;
  color: #00e87a;
  padding: 7px 10px;
  font-family: inherit;
  font-size: 11px;
  border-radius: 3px;
  cursor: pointer;
  letter-spacing: 0.04em;
  transition: all 0.15s;
  align-self: flex-start;
}
.map-panel-cta:hover {
  border-color: #00e87a;
  background: #00e87a11;
}
.map-cta-key {
  display: inline-block;
  padding: 0 3px;
  background: #00e87a22;
  border-radius: 2px;
  margin-right: 4px;
  font-weight: 600;
}
.map-panel-locked {
  font-size: 10px;
  color: #333;
  letter-spacing: 0.04em;
}
</style>
