<script setup lang="ts">
interface Stat {
  value: number
  label: string
  color: string
}

interface Badge {
  icon: string
  name: string
  state: 'earned' | 'progress' | 'locked'
  date?: string
  current?: number
  target?: number
}

interface TimelineItem {
  date: string
  textHtml: string
}

interface NextItem {
  icon: string
  name: string
  reason: string
}

interface CalCell {
  date: Date
  count: number
}

const user = {
  initial: 'Y',
  name: 'you',
  handle: '// joined 42 days ago',
  bio: 'Вчу веб-розробку одну картку за раз.',
  streak: 7,
  bestStreak: 12,
}

const progressStats: Stat[] = [
  { value: 34, label: 'постів прочитано', color: '#00e87a' },
  { value: 12, label: 'тем вивчено', color: '#6ab4ff' },
  { value: 3, label: 'курсів', color: '#ffb830' },
  { value: 8, label: 'нотаток', color: '#a78bfa' },
]

const contributionStats: Stat[] = [
  { value: 7, label: 'людям допоміг', color: '#eeece4' },
  { value: 14, label: 'корисних відповідей', color: '#eeece4' },
  { value: 2, label: 'гайди написав', color: '#eeece4' },
  { value: 5, label: 'тем покращив', color: '#eeece4' },
]

interface TreeLine {
  prefix: string
  body: string
  state: 'root' | 'done' | 'current' | 'here' | 'pending'
  blink?: boolean
}

const currentLearning = {
  lastSession: 'сьогодні, 14:32',
  hereName: 'infinite_scroll',
  hereDescription:
    'Приклад використання IO для нескінченної стрічки. Розберемо rootMargin, threshold і як уникати memory leaks.',
  hereEstimate: '~ 8 хв',
  hereIndex: 6,
  hereTotal: 8,
}

const treeLines: TreeLine[] = [
  { prefix: '', body: 'web_apis/', state: 'root' },
  { prefix: '├── ', body: 'fetch_api              ✓', state: 'done' },
  { prefix: '├── ', body: 'intersection_observer  ● 5/8', state: 'current' },
  { prefix: '│   ├── ', body: 'basics              ✓', state: 'done' },
  { prefix: '│   ├── ', body: 'thresholds          ✓', state: 'done' },
  { prefix: '│   ├── ', body: 'infinite_scroll     ▸', state: 'here', blink: true },
  { prefix: '│   ├── ', body: 'cleanup             ·', state: 'pending' },
  { prefix: '│   └── ', body: 'performance         ·', state: 'pending' },
  { prefix: '├── ', body: 'mutation_observer      ·', state: 'pending' },
  { prefix: '└── ', body: 'performance_api        ·', state: 'pending' },
]

const aiSummary = {
  textHtml:
    '+4 нові теми за тиждень, <strong>12 карток</strong> прочитано. Найсильніший прогрес у <strong>Promises</strong> — 0 помилок у quiz. Час спробувати <strong>async/await</strong>, це логічний крок.',
}

const knowledgeMap = {
  unlocked: 12,
  total: 60,
  recent: 'CSS Subgrid, Event Loop',
}

const badges: Badge[] = [
  { icon: '◆', name: 'Перший пост', state: 'earned', date: '42 дні тому' },
  { icon: '●', name: '7 днів поспіль', state: 'earned', date: 'сьогодні' },
  { icon: '▲', name: 'Перший quiz', state: 'earned', date: '18 днів тому' },
  { icon: '◈', name: '10 нотаток', state: 'earned', date: '5 днів тому' },
  { icon: '★', name: 'Перший курс', state: 'earned', date: '12 днів тому' },
  { icon: '◉', name: '30 днів поспіль', state: 'progress', current: 7, target: 30 },
  { icon: '◊', name: '50 нотаток', state: 'progress', current: 8, target: 50 },
  { icon: '⬡', name: 'Перший коментар', state: 'locked' },
  { icon: '☰', name: '100 карток', state: 'locked' },
]

const timeline: TimelineItem[] = [
  {
    date: 'Сьогодні',
    textHtml: 'Вивчаєш <strong>Web APIs</strong> — Intersection Observer і lazy-loading',
  },
  {
    date: '3 дні тому',
    textHtml: 'Завершив тему <strong>Promises</strong> — пройшов quiz без помилок',
  },
  {
    date: '1 тиждень тому',
    textHtml: 'Розпочав курс <strong>"Promises без болю"</strong> — 5 з 8 карток',
  },
  { date: '2 тижні тому', textHtml: 'Вивчив тему <strong>Flexbox</strong> — зберіг 3 нотатки' },
  { date: '3 тижні тому', textHtml: 'Почав з <strong>CSS Grid</strong> — перша тема на DocTok' },
]

const nextItems: NextItem[] = [
  { icon: '⇢', name: 'async/await', reason: 'Логічний наступний крок після Promises' },
  { icon: '↻', name: 'Event Loop', reason: 'Пояснить чому Promise виконується саме так' },
  { icon: '▦', name: 'CSS Subgrid', reason: 'Розширення Grid яке ти вже знаєш' },
]

const WEEKS = 26
const DAYS = WEEKS * 7
const ukMonths = [
  'Січ',
  'Лют',
  'Бер',
  'Кві',
  'Тра',
  'Чер',
  'Лип',
  'Сер',
  'Вер',
  'Жов',
  'Лис',
  'Гру',
]

// Deterministic pseudo-random so SSR & client match.
const seededRand = (seed: number) => {
  let s = seed
  return () => {
    s = (s * 9301 + 49297) % 233280
    return s / 233280
  }
}

const buildActivity = (): CalCell[] => {
  const rand = seededRand(42)
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const cells: CalCell[] = []
  for (let i = DAYS - 1; i >= 0; i--) {
    const d = new Date(today)
    d.setDate(today.getDate() - i)
    let count = 0
    if (i < 7) count = Math.floor(rand() * 4) + 1
    else if (i < 14) count = rand() > 0.3 ? Math.floor(rand() * 3) + 1 : 0
    else if (i < 30) count = rand() > 0.4 ? Math.floor(rand() * 3) : 0
    else count = rand() > 0.6 ? Math.floor(rand() * 2) + 1 : 0
    cells.push({ date: d, count })
  }
  return cells
}

const activity = buildActivity()

const cellStyle = (count: number) => {
  if (count === 0) return { background: '#0e0e0e', border: '1px solid #161616' }
  if (count === 1) return { background: '#012b12', border: 'none' }
  if (count === 2) return { background: '#015c24', border: 'none' }
  if (count === 3) return { background: '#01a03a', border: 'none' }
  return { background: '#00e87a', border: 'none' }
}

interface Week {
  cells: (CalCell | null)[]
  monthLabel: string
}

const weeks = computed<Week[]>(() => {
  const startOffset = (activity[0]!.date.getDay() + 6) % 7
  const result: Week[] = []
  let current: (CalCell | null)[] = []
  for (let i = 0; i < startOffset; i++) current.push(null)

  for (const cell of activity) {
    current.push(cell)
    if (current.length === 7) {
      result.push({ cells: current, monthLabel: '' })
      current = []
    }
  }
  if (current.length) {
    while (current.length < 7) current.push(null)
    result.push({ cells: current, monthLabel: '' })
  }

  for (const week of result) {
    const firstOfMonth = week.cells.find((c) => c && c.date.getDate() === 1)
    if (firstOfMonth) week.monthLabel = ukMonths[firstOfMonth.date.getMonth()]!
  }

  return result
})

const tooltip = ref<{ show: boolean; text: string; x: number; y: number }>({
  show: false,
  text: '',
  x: 0,
  y: 0,
})

const showTooltip = (e: MouseEvent, cell: CalCell) => {
  const count = cell.count
  const label =
    count === 0
      ? 'немає активності'
      : `${count} ${count === 1 ? 'тема' : count < 5 ? 'теми' : 'тем'}`
  const ds = cell.date.toLocaleDateString('uk-UA', { day: 'numeric', month: 'long' })
  tooltip.value = { show: true, text: `${ds} — ${label}`, x: e.clientX + 12, y: e.clientY - 32 }
}

const moveTooltip = (e: MouseEvent) => {
  if (!tooltip.value.show) return
  tooltip.value.x = e.clientX + 12
  tooltip.value.y = e.clientY - 32
}

const hideTooltip = () => {
  tooltip.value.show = false
}
</script>

<template>
  <section id="profile">
    <div id="prof-scroll">
      <div id="prof-layout">
        <aside id="prof-sidebar">
          <div id="prof-avatar">
            {{ user.initial }}
          </div>
          <div id="prof-name">
            {{ user.name }}
          </div>
          <div id="prof-handle">
            {{ user.handle }}
          </div>
          <div id="prof-bio">
            {{ user.bio }}
          </div>

          <div id="prof-streak-pill">
            <div id="prof-streak-num">
              {{ user.streak }}
            </div>
            <div>
              <div id="prof-streak-label">днів поспіль</div>
              <div id="prof-streak-sub">найкращий: {{ user.bestStreak }} днів</div>
            </div>
          </div>

          <div class="prof-sidebar-section">
            <div class="prof-section-label-sm">// твій прогрес</div>
            <div class="prof-stats-grid">
              <div
                v-for="s in progressStats"
                :key="s.label"
                class="prof-stat"
              >
                <div
                  class="prof-stat-val"
                  :style="{ color: s.color }"
                >
                  {{ s.value }}
                </div>
                <div class="prof-stat-label">
                  {{ s.label }}
                </div>
              </div>
            </div>
          </div>

          <div class="prof-sidebar-section">
            <div class="prof-section-label-sm">// внесок</div>
            <div class="prof-stats-grid">
              <div
                v-for="s in contributionStats"
                :key="s.label"
                class="prof-stat"
              >
                <div
                  class="prof-stat-val"
                  :style="{ color: s.color }"
                >
                  {{ s.value }}
                </div>
                <div class="prof-stat-label">
                  {{ s.label }}
                </div>
              </div>
            </div>
          </div>
        </aside>

        <div id="prof-main">
          <section id="prof-current">
            <div class="prof-current-head">
              <span class="prof-current-label">// зараз</span>
              <span class="prof-current-session">last: {{ currentLearning.lastSession }}</span>
            </div>
            <div class="prof-current-body">
              <div class="prof-current-tree">
                <div
                  v-for="(line, i) in treeLines"
                  :key="i"
                  class="prof-tree-line"
                  :class="`state-${line.state}`"
                >
                  <span class="prof-tree-prefix">{{ line.prefix }}</span>
                  <span class="prof-tree-body">{{ line.body }}</span>
                  <span
                    v-if="line.blink"
                    class="prof-tree-caret"
                    >_</span
                  >
                </div>
              </div>
              <aside class="prof-current-preview">
                <div class="prof-preview-header">
                  <span class="prof-preview-name">{{ currentLearning.hereName }}</span>
                  <span class="prof-preview-meta">
                    {{ currentLearning.hereIndex }}/{{ currentLearning.hereTotal }}
                  </span>
                </div>
                <p class="prof-preview-desc">
                  {{ currentLearning.hereDescription }}
                </p>
                <div class="prof-preview-estimate">
                  {{ currentLearning.hereEstimate }} · картка {{ currentLearning.hereIndex }} з
                  {{ currentLearning.hereTotal }}
                </div>
                <button class="prof-current-cta">
                  [<span class="prof-cta-key">↵</span> продовжити]
                </button>
              </aside>
            </div>
          </section>

          <section id="prof-ai-summary">
            <div class="prof-ai-head">
              <span class="prof-ai-icon">✦</span>
              <span class="prof-section-label">// за останній тиждень</span>
            </div>
            <!-- eslint-disable-next-line vue/no-v-html -->
            <div
              class="prof-ai-text"
              v-html="aiSummary.textHtml"
            />
          </section>

          <section>
            <div class="prof-section-label">активність за пів року</div>
            <div id="prof-cal-months">
              <div
                v-for="(w, i) in weeks"
                :key="`m-${i}`"
                class="prof-cal-month"
              >
                {{ w.monthLabel }}
              </div>
            </div>
            <div id="prof-cal-body">
              <div id="prof-cal-day-labels">
                <div class="prof-cal-day-label" />
                <div class="prof-cal-day-label">пн</div>
                <div class="prof-cal-day-label" />
                <div class="prof-cal-day-label">ср</div>
                <div class="prof-cal-day-label" />
                <div class="prof-cal-day-label">пт</div>
                <div class="prof-cal-day-label" />
              </div>
              <div id="prof-cal-grid">
                <div
                  v-for="(w, wi) in weeks"
                  :key="`w-${wi}`"
                  class="prof-cal-week"
                >
                  <div
                    v-for="(c, ci) in w.cells"
                    :key="`c-${wi}-${ci}`"
                    class="prof-cal-cell"
                    :style="c ? cellStyle(c.count) : { background: 'transparent', border: 'none' }"
                    @mouseenter="c && showTooltip($event, c)"
                    @mousemove="moveTooltip"
                    @mouseleave="hideTooltip"
                  />
                </div>
              </div>
            </div>
            <div id="prof-cal-legend">
              <span class="prof-cal-legend-label">менше</span>
              <div
                class="prof-cal-legend-cell"
                style="background: #0e0e0e; border: 1px solid #161616"
              />
              <div
                class="prof-cal-legend-cell"
                style="background: #012b12"
              />
              <div
                class="prof-cal-legend-cell"
                style="background: #015c24"
              />
              <div
                class="prof-cal-legend-cell"
                style="background: #01a03a"
              />
              <div
                class="prof-cal-legend-cell"
                style="background: #00e87a"
              />
              <span class="prof-cal-legend-label">більше</span>
            </div>
          </section>

          <section>
            <div class="prof-section-label">досягнення</div>
            <div id="prof-badges">
              <div
                v-for="b in badges"
                :key="b.name"
                class="prof-badge"
                :class="[`state-${b.state}`]"
              >
                <div class="prof-badge-icon">
                  {{ b.icon }}
                </div>
                <div class="prof-badge-body">
                  <div class="prof-badge-name">
                    {{ b.name }}
                  </div>
                  <div
                    v-if="b.state === 'earned'"
                    class="prof-badge-date"
                  >
                    {{ b.date }}
                  </div>
                  <div
                    v-else-if="b.state === 'progress'"
                    class="prof-badge-progress"
                  >
                    <div class="prof-badge-bar">
                      <div
                        class="prof-badge-fill"
                        :style="{
                          width: (b.current! / b.target!) * 100 + '%',
                        }"
                      />
                    </div>
                    <div class="prof-badge-count">{{ b.current }} / {{ b.target }}</div>
                  </div>
                  <div
                    v-else
                    class="prof-badge-date"
                  >
                    // не розблоковано
                  </div>
                </div>
              </div>
            </div>
          </section>

          <NuxtLink
            to="/map"
            class="prof-map-teaser"
          >
            <div class="prof-map-icon">◉</div>
            <div class="prof-map-body">
              <div class="prof-map-title">карта знань</div>
              <div class="prof-map-sub">
                {{ knowledgeMap.unlocked }} / {{ knowledgeMap.total }} тем розблоковано · нещодавно:
                {{ knowledgeMap.recent }}
              </div>
            </div>
            <div class="prof-map-arrow">→</div>
          </NuxtLink>

          <div id="prof-two-col">
            <section>
              <div class="prof-section-label">мій шлях</div>
              <div id="prof-timeline">
                <div
                  v-for="(t, i) in timeline"
                  :key="i"
                  class="prof-tl-item"
                >
                  <div class="prof-tl-dot" />
                  <div class="prof-tl-content">
                    <div class="prof-tl-date">
                      {{ t.date }}
                    </div>
                    <!-- eslint-disable-next-line vue/no-v-html -->
                    <div
                      class="prof-tl-text"
                      v-html="t.textHtml"
                    />
                  </div>
                </div>
              </div>
            </section>

            <section>
              <div class="prof-section-label">що далі</div>
              <div id="prof-next">
                <div
                  v-for="n in nextItems"
                  :key="n.name"
                  class="prof-next-card"
                >
                  <div class="prof-next-icon">
                    {{ n.icon }}
                  </div>
                  <div>
                    <div class="prof-next-name">
                      {{ n.name }}
                    </div>
                    <div class="prof-next-reason">
                      {{ n.reason }}
                    </div>
                  </div>
                  <div class="prof-next-arrow">→</div>
                </div>
              </div>
            </section>
          </div>
        </div>
      </div>
    </div>

    <Teleport to="body">
      <div
        v-show="tooltip.show"
        id="cal-tooltip"
        :style="{ left: tooltip.x + 'px', top: tooltip.y + 'px' }"
      >
        {{ tooltip.text }}
      </div>
    </Teleport>
  </section>
</template>

<style scoped>
#profile {
  flex: 1;
  background: #080808;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  font-family: 'SF Mono', 'Fira Code', 'Consolas', 'Menlo', monospace;
}
#prof-scroll {
  flex: 1;
  overflow-y: auto;
  padding: 28px 36px 48px;
  width: 100%;
}
#prof-scroll::-webkit-scrollbar {
  width: 2px;
}
#prof-scroll::-webkit-scrollbar-thumb {
  background: #1a1a1a;
}
#prof-layout {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 40px;
  max-width: 1240px;
  margin: 0 auto;
  align-items: start;
}
#prof-sidebar {
  position: sticky;
  top: 0;
  display: flex;
  flex-direction: column;
  gap: 14px;
}
#prof-main {
  display: flex;
  flex-direction: column;
  gap: 32px;
  min-width: 0;
}
#prof-two-col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 32px;
}
@media (max-width: 1100px) {
  #prof-layout {
    grid-template-columns: 1fr;
    gap: 28px;
  }
  #prof-sidebar {
    position: static;
  }
  #prof-two-col {
    grid-template-columns: 1fr;
  }
}
#prof-avatar {
  width: 128px;
  height: 128px;
  border-radius: 50%;
  background: #001f0d;
  border: 1px solid #00e87a22;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 48px;
  color: #00e87a;
  flex-shrink: 0;
  font-family: Georgia, serif;
  margin-bottom: 4px;
}
#prof-name {
  font-family: Georgia, serif;
  font-size: 24px;
  color: #eeece4;
  font-weight: 700;
  line-height: 1.1;
}
#prof-handle {
  font-size: 10px;
  color: #2a2a2a;
  letter-spacing: 0.1em;
}
#prof-bio {
  font-size: 11px;
  color: #444;
  line-height: 1.6;
}
#prof-streak-pill {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #001f0d;
  border: 1px solid #00e87a1a;
  border-radius: 6px;
  padding: 12px 14px;
  margin-top: 4px;
}
#prof-streak-num {
  font-family: Georgia, serif;
  font-size: 28px;
  font-weight: 700;
  color: #00e87a;
  line-height: 1;
}
#prof-streak-label {
  font-size: 10px;
  color: #3a8a50;
  letter-spacing: 0.06em;
}
#prof-streak-sub {
  font-size: 9px;
  color: #1a4a28;
}
.prof-sidebar-section {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 4px;
}
.prof-section-label-sm {
  font-size: 10px;
  color: #3a3a3a;
  letter-spacing: 0.08em;
}
.prof-stats-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}
.prof-stat {
  background: #060606;
  border: 1px solid #0e0e0e;
  border-radius: 6px;
  padding: 12px 14px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.prof-stat-val {
  font-size: 22px;
  font-weight: 600;
  line-height: 1;
}
.prof-stat-label {
  font-size: 9px;
  color: #2a2a2a;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}
.prof-section-label {
  font-size: 10px;
  color: #3a3a3a;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  margin-bottom: 14px;
}
.prof-section-label--accent {
  color: #00e87a;
}

/* Currently Learning — tree view */
#prof-current {
  border: 1px solid #0e0e0e;
  background: #060606;
  border-radius: 6px;
  padding: 14px 18px 16px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.prof-current-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.prof-current-label {
  font-size: 10px;
  color: #3a3a3a;
  letter-spacing: 0.14em;
}
.prof-current-session {
  font-size: 9px;
  color: #2a2a2a;
  letter-spacing: 0.04em;
}
.prof-current-body {
  display: grid;
  grid-template-columns: auto 1px 1fr;
  gap: 20px;
  align-items: start;
}
.prof-current-body::before {
  content: '';
  grid-column: 2;
  align-self: stretch;
  background: #111;
}
.prof-current-tree {
  font-size: 12px;
  line-height: 1.7;
  grid-column: 1;
}
.prof-current-preview {
  grid-column: 3;
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding-top: 2px;
  min-width: 0;
}
.prof-preview-header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 8px;
}
.prof-preview-name {
  font-size: 13px;
  color: #eeece4;
  letter-spacing: 0.01em;
}
.prof-preview-meta {
  font-size: 9px;
  color: #3a8a50;
  letter-spacing: 0.04em;
}
.prof-preview-desc {
  font-size: 11px;
  color: #666;
  line-height: 1.6;
}
.prof-preview-estimate {
  font-size: 9px;
  color: #2a2a2a;
  letter-spacing: 0.06em;
}
@media (max-width: 900px) {
  .prof-current-body {
    grid-template-columns: 1fr;
    gap: 14px;
  }
  .prof-current-body::before {
    display: none;
  }
  .prof-current-tree,
  .prof-current-preview {
    grid-column: 1;
  }
}
.prof-tree-line {
  display: flex;
  align-items: baseline;
  white-space: pre;
}
.prof-tree-prefix {
  color: #1e1e1e;
}
.prof-tree-body {
  color: #3a3a3a;
}
.state-root .prof-tree-body {
  color: #888;
}
.state-done .prof-tree-body {
  color: #3a8a50;
}
.state-current .prof-tree-body {
  color: #00e87a;
}
.state-here .prof-tree-body {
  color: #eeece4;
}
.state-pending .prof-tree-body {
  color: #2a2a2a;
}
.prof-tree-caret {
  color: #00e87a;
  margin-left: 2px;
  animation: prof-blink 1s steps(2, start) infinite;
}
@keyframes prof-blink {
  to {
    visibility: hidden;
  }
}
.prof-current-cta {
  align-self: flex-start;
  background: transparent;
  border: 1px solid #00e87a44;
  padding: 6px 10px;
  margin-top: 4px;
  font-family: inherit;
  font-size: 11px;
  color: #00e87a;
  cursor: pointer;
  letter-spacing: 0.04em;
  border-radius: 3px;
  transition: all 0.15s;
}
.prof-current-cta:hover {
  border-color: #00e87a;
  background: #00e87a11;
}
.prof-cta-key {
  display: inline-block;
  padding: 0 3px;
  background: #00e87a22;
  border-radius: 2px;
  margin-right: 2px;
  font-weight: 600;
}

/* AI weekly summary */
#prof-ai-summary {
  border: 1px solid #0e0e0e;
  background: #060606;
  border-radius: 8px;
  padding: 16px 18px;
}
.prof-ai-head {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 10px;
}
.prof-ai-head .prof-section-label {
  margin-bottom: 0;
}
.prof-ai-icon {
  font-size: 12px;
  color: #a78bfa;
}
.prof-ai-text {
  font-size: 11px;
  color: #666;
  line-height: 1.7;
}
.prof-ai-text :deep(strong) {
  color: #a78bfa;
  font-weight: 500;
}
#prof-cal-body {
  display: flex;
  gap: 6px;
  margin-top: 4px;
}
#prof-cal-day-labels {
  display: flex;
  flex-direction: column;
  gap: 4px;
  margin-top: 0;
  flex-shrink: 0;
}
.prof-cal-day-label {
  font-size: 9px;
  color: #2a2a2a;
  height: 14px;
  display: flex;
  align-items: center;
  width: 22px;
}
#prof-cal-grid {
  display: flex;
  gap: 4px;
}
.prof-cal-week {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.prof-cal-cell {
  width: 14px;
  height: 14px;
  border-radius: 3px;
  cursor: default;
  flex-shrink: 0;
}
#prof-cal-months {
  display: flex;
  gap: 4px;
  margin-left: 28px;
  margin-bottom: 4px;
}
.prof-cal-month {
  font-size: 9px;
  color: #2a2a2a;
  width: 14px;
  flex-shrink: 0;
}
#prof-cal-legend {
  display: flex;
  align-items: center;
  gap: 6px;
  justify-content: flex-end;
  margin-top: 6px;
}
.prof-cal-legend-label {
  font-size: 8px;
  color: #1a1a1a;
}
.prof-cal-legend-cell {
  width: 11px;
  height: 11px;
  border-radius: 2px;
}
#prof-badges {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px;
}
.prof-badge {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 14px;
  border: 1px solid #0e0e0e;
  border-radius: 6px;
  background: #060606;
}
.prof-badge.state-earned {
  border-color: #00e87a22;
}
.prof-badge.state-progress {
  border-color: #1a3a22;
}
.prof-badge.state-locked {
  opacity: 0.3;
}
.prof-badge-icon {
  font-size: 18px;
  flex-shrink: 0;
  width: 22px;
  text-align: center;
}
.state-earned .prof-badge-icon {
  color: #00e87a;
}
.state-progress .prof-badge-icon {
  color: #666;
}
.state-locked .prof-badge-icon {
  color: #555;
}
.prof-badge-body {
  flex: 1;
  min-width: 0;
}
.prof-badge-name {
  font-size: 11px;
  color: #888;
  line-height: 1.2;
}
.prof-badge-date {
  font-size: 9px;
  color: #2a2a2a;
  margin-top: 4px;
}
.prof-badge-progress {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 6px;
}
.prof-badge-bar {
  flex: 1;
  height: 3px;
  background: #0e0e0e;
  border-radius: 2px;
  overflow: hidden;
}
.prof-badge-fill {
  height: 100%;
  background: #3a8a50;
  border-radius: 2px;
}
.prof-badge-count {
  font-size: 8px;
  color: #3a8a50;
  letter-spacing: 0.04em;
  flex-shrink: 0;
}

/* Knowledge map teaser */
.prof-map-teaser {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 16px 20px;
  border: 1px solid #0e0e0e;
  border-radius: 8px;
  background: #060606;
  text-decoration: none;
  transition: all 0.15s;
  color: inherit;
}
.prof-map-teaser:hover {
  border-color: #6ab4ff33;
  background: #0a0e14;
}
.prof-map-icon {
  font-size: 24px;
  color: #6ab4ff;
  flex-shrink: 0;
}
.prof-map-body {
  flex: 1;
  min-width: 0;
}
.prof-map-title {
  font-family: Georgia, serif;
  font-size: 15px;
  color: #eeece4;
  margin-bottom: 3px;
}
.prof-map-sub {
  font-size: 10px;
  color: #555;
  line-height: 1.4;
}
.prof-map-arrow {
  font-size: 14px;
  color: #333;
  margin-left: auto;
  flex-shrink: 0;
}
.prof-map-teaser:hover .prof-map-arrow {
  color: #6ab4ff;
}
#prof-timeline {
  display: flex;
  flex-direction: column;
}
.prof-tl-item {
  display: flex;
  gap: 12px;
  position: relative;
}
.prof-tl-item:not(:last-child)::before {
  content: '';
  position: absolute;
  left: 5px;
  top: 18px;
  bottom: -6px;
  width: 1px;
  background: #111;
}
.prof-tl-dot {
  width: 11px;
  height: 11px;
  border-radius: 50%;
  border: 1px solid #00e87a33;
  background: #001f0d;
  flex-shrink: 0;
  margin-top: 3px;
}
.prof-tl-content {
  flex: 1;
  padding-bottom: 14px;
}
.prof-tl-date {
  font-size: 9px;
  color: #2a2a2a;
  margin-bottom: 4px;
  letter-spacing: 0.06em;
}
.prof-tl-text {
  font-size: 11px;
  color: #555;
  line-height: 1.6;
}
.prof-tl-text :deep(strong) {
  color: #888;
  font-weight: 500;
}
#prof-next {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.prof-next-card {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 13px;
  border: 1px solid #0e0e0e;
  border-radius: 6px;
  background: #060606;
  cursor: pointer;
  transition: all 0.15s;
}
.prof-next-card:hover {
  border-color: #161616;
  background: #0a0a0a;
}
.prof-next-icon {
  font-size: 14px;
  color: #2a2a2a;
  flex-shrink: 0;
  width: 20px;
  text-align: center;
}
.prof-next-name {
  font-size: 12px;
  color: #666;
  margin-bottom: 3px;
}
.prof-next-reason {
  font-size: 10px;
  color: #2a2a2a;
  line-height: 1.4;
}
.prof-next-arrow {
  font-size: 10px;
  color: #1a1a1a;
  margin-left: auto;
}
</style>

<style>
#cal-tooltip {
  position: fixed;
  background: #0d1f35;
  border: 1px solid #2a5f9f44;
  border-radius: 5px;
  padding: 6px 10px;
  font-size: 9px;
  color: #8ab8e8;
  pointer-events: none;
  z-index: 999;
  white-space: nowrap;
  font-family: 'SF Mono', 'Fira Code', monospace;
}
</style>
