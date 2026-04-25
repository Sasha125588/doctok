export type MockPostKind = 'summary' | 'example' | 'fact'

export interface MockPost {
  title: string
  kind: MockPostKind
  related: string[]
}

export interface MockTopic {
  name: string
  icon: string
  category: string
  source: string
  posts: MockPost[]
}

export interface MockCategory {
  name: string
  icon: string
  topics: MockTopic[]
}

export interface MockCourse {
  name: string
  icon: string
  count: number
  progress: number
  category: string
}

export interface MockSearchHit {
  topic: MockTopic
  post: MockPost
}

export const mockCategories: MockCategory[] = [
  {
    name: 'CSS',
    icon: '◈',
    topics: [
      {
        name: 'CSS Grid',
        icon: '▦',
        category: 'CSS',
        source: 'MDN',
        posts: [
          {
            kind: 'summary',
            title: 'CSS Grid',
            related: ['grid-template', 'Flexbox', '1fr', 'subgrid'],
          },
          {
            kind: 'example',
            title: 'Три рівні колонки',
            related: ['repeat()', 'minmax()', 'auto-fill', 'auto-fit'],
          },
          {
            kind: 'fact',
            title: 'IE вперше реалізував Grid',
            related: ['CSS history', 'IE prefixes', 'W3C'],
          },
        ],
      },
      {
        name: 'Flexbox',
        icon: '⇔',
        category: 'CSS',
        source: 'MDN',
        posts: [
          {
            kind: 'summary',
            title: 'CSS Flexbox',
            related: ['justify-content', 'align-items', 'flex-grow', 'CSS Grid'],
          },
          {
            kind: 'example',
            title: 'Класичне центрування',
            related: ['flex-direction', 'flex-wrap', 'gap'],
          },
          {
            kind: 'fact',
            title: 'Три різних синтаксиси',
            related: ['Can I Use', 'CSS history', '-webkit-'],
          },
        ],
      },
    ],
  },
  {
    name: 'JavaScript',
    icon: '◇',
    topics: [
      {
        name: 'Promises',
        icon: '⇢',
        category: 'JavaScript',
        source: 'MDN',
        posts: [
          {
            kind: 'summary',
            title: 'JavaScript Promises',
            related: ['async/await', 'мікрозадачі', 'Event Loop', 'fetch()'],
          },
          {
            kind: 'example',
            title: 'Ланцюжок .then()',
            related: ['.catch()', '.finally()', 'Promise.all', 'async/await'],
          },
          {
            kind: 'fact',
            title: 'Promises/A+ з 2012',
            related: ['ES2015', 'Bluebird', 'TC39'],
          },
        ],
      },
    ],
  },
  {
    name: 'Web APIs',
    icon: '◉',
    topics: [
      {
        name: 'Web APIs',
        icon: '◈',
        category: 'Web APIs',
        source: 'web.dev',
        posts: [
          {
            kind: 'summary',
            title: 'Web APIs',
            related: ['Navigator', 'Window', 'DOM', 'WHATWG'],
          },
          {
            kind: 'example',
            title: 'Intersection Observer',
            related: ['lazy loading', 'MutationObserver', 'ResizeObserver'],
          },
          {
            kind: 'fact',
            title: '5000+ інтерфейсів',
            related: ['MDN', 'Baseline', 'deprecated'],
          },
        ],
      },
    ],
  },
]

export const mockMiniCourses: MockCourse[] = [
  { name: 'Promises без болю', icon: '⇢', count: 8, progress: 62, category: 'JavaScript' },
  { name: 'CSS Grid за 10 карток', icon: '▦', count: 10, progress: 30, category: 'CSS' },
  { name: 'Web APIs: старт', icon: '◈', count: 6, progress: 0, category: 'Web APIs' },
  { name: 'Async/Await глибоко', icon: '↻', count: 7, progress: 0, category: 'JavaScript' },
]

export const mockAllPosts: MockSearchHit[] = mockCategories.flatMap((category) =>
  category.topics.flatMap((topic) => topic.posts.map((post) => ({ topic, post })))
)

export function filterMockSearchHits(query: string, limit = 20): MockSearchHit[] {
  const normalizedQuery = query.trim().toLowerCase()

  if (!normalizedQuery) {
    return []
  }

  return mockAllPosts
    .filter(
      ({ topic, post }) =>
        post.title.toLowerCase().includes(normalizedQuery) ||
        topic.name.toLowerCase().includes(normalizedQuery) ||
        post.related.some((related) => related.toLowerCase().includes(normalizedQuery))
    )
    .slice(0, limit)
}
