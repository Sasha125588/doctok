import type { HomeArticle, HomeLinkGroup, HomeSection } from '../-types'

export const homeSections: HomeSection[] = [
  {
    slug: 'web-apis',
    title: 'Web APIs',
    eyebrow: 'interfaces',
    description: 'Browser capabilities, events, storage, network, navigation, and device APIs.',
    icon: 'lucide:blocks',
    tone: 'api',
    query: 'Web APIs',
    tracks: ['DOM', 'Fetch', 'Storage'],
  },
  {
    slug: 'javascript',
    title: 'JavaScript',
    eyebrow: 'language',
    description: 'Core syntax, async flows, objects, modules, browser runtime, and patterns.',
    icon: 'lucide:braces',
    tone: 'js',
    query: 'JavaScript',
    tracks: ['Promises', 'Modules', 'Runtime'],
  },
  {
    slug: 'css',
    title: 'CSS',
    eyebrow: 'styling',
    description: 'Layout, cascade, animation, responsive design, selectors, and modern CSS.',
    icon: 'lucide:palette',
    tone: 'css',
    query: 'CSS',
    tracks: ['Grid', 'Flexbox', 'Cascade'],
  },
  {
    slug: 'html',
    title: 'HTML',
    eyebrow: 'structure',
    description: 'Semantic documents, forms, media, metadata, accessibility, and elements.',
    icon: 'lucide:file-code-2',
    tone: 'html',
    query: 'HTML',
    tracks: ['Forms', 'Semantics', 'Media'],
  },
  {
    slug: 'http',
    title: 'HTTP',
    eyebrow: 'protocol',
    description: 'Requests, responses, caching, headers, status codes, security, and cookies.',
    icon: 'lucide:radio-tower',
    tone: 'http',
    query: 'HTTP',
    tracks: ['Headers', 'Caching', 'Security'],
  },
  {
    slug: 'accessibility',
    title: 'Accessibility',
    eyebrow: 'inclusive web',
    description: 'ARIA, keyboard flows, semantic structure, contrast, and assistive technology.',
    icon: 'lucide:accessibility',
    tone: 'a11y',
    query: 'Accessibility',
    tracks: ['ARIA', 'Keyboard', 'Semantics'],
  },
  {
    slug: 'performance',
    title: 'Performance',
    eyebrow: 'speed',
    description: 'Loading, rendering, Core Web Vitals, resource strategy, and runtime cost.',
    icon: 'lucide:gauge',
    tone: 'perf',
    query: 'Performance',
    tracks: ['Loading', 'Rendering', 'Vitals'],
  },
]

export const featuredArticles: HomeArticle[] = [
  {
    title: 'Navigation API',
    category: 'Web APIs',
    description: 'A modern API for intercepting and managing navigations in app-like experiences.',
    href: 'https://developer.mozilla.org/en-US/docs/Web/API/Navigation_API',
  },
  {
    title: 'Trusted Types API',
    category: 'Security',
    description: 'A browser-level guardrail for reducing DOM XSS surfaces in larger applications.',
    href: 'https://developer.mozilla.org/en-US/docs/Web/API/Trusted_Types_API',
  },
  {
    title: 'Celebrating 20 years of MDN',
    category: 'Blog',
    description: 'A look back at MDN as a living, community-built resource for the web platform.',
    href: 'https://developer.mozilla.org/en-US/blog/',
  },
]

export const recentContributions: HomeArticle[] = [
  {
    title: 'CSS anchor positioning',
    category: 'CSS',
    description: 'Position UI relative to another element without custom geometry code.',
    href: 'https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_anchor_positioning',
  },
  {
    title: 'Baseline web features',
    category: 'Reference',
    description: 'Understand which web platform features are stable across modern browsers.',
    href: 'https://developer.mozilla.org/en-US/docs/Glossary/Baseline/Compatibility',
  },
]

export const ecosystemLinks: HomeLinkGroup[] = [
  {
    title: 'MDN',
    links: [
      { label: 'About MDN', href: 'https://developer.mozilla.org/en-US/about' },
      { label: 'MDN Blog', href: 'https://developer.mozilla.org/en-US/blog/' },
      { label: 'MDN Discord', href: 'https://mdn.dev/discord' },
      { label: 'MDN on GitHub', href: 'https://github.com/mdn' },
    ],
  },
  {
    title: 'Reference',
    links: [
      { label: 'Web technology', href: 'https://developer.mozilla.org/en-US/docs/Web' },
      { label: 'Learn web development', href: 'https://developer.mozilla.org/en-US/docs/Learn' },
      { label: 'Developer guides', href: 'https://developer.mozilla.org/en-US/docs/MDN/Guides' },
      { label: 'Tutorials', href: 'https://developer.mozilla.org/en-US/docs/MDN/Tutorials' },
    ],
  },
]
