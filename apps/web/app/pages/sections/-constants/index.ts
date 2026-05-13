import type { SectionIndexGroup, SectionWorkspace } from '../-types'

export const sectionIndexGroups: SectionIndexGroup[] = [
  {
    title: 'Core web languages',
    description: 'The foundations you write and read every day.',
    items: [
      {
        title: 'HTML',
        description: 'Semantic structure, forms, media, metadata, and accessible documents.',
        icon: 'lucide:file-code-2',
        tone: 'html',
        query: 'HTML',
        workspaceSlug: 'html',
      },
      {
        title: 'CSS',
        description: 'Layout, cascade, selectors, animation, responsive UI, and visual systems.',
        icon: 'lucide:palette',
        tone: 'css',
        query: 'CSS',
        workspaceSlug: 'css',
      },
      {
        title: 'JavaScript',
        description: 'Language fundamentals, async work, objects, modules, and browser runtime.',
        icon: 'lucide:braces',
        tone: 'js',
        query: 'JavaScript',
        workspaceSlug: 'javascript',
      },
      {
        title: 'WebAssembly',
        description: 'Portable low-level modules for performance-sensitive code on the web.',
        icon: 'lucide:binary',
        tone: 'runtime',
        query: 'WebAssembly',
      },
    ],
  },
  {
    title: 'Platform APIs',
    description: 'Browser capabilities, protocols, installable apps, and automation surfaces.',
    items: [
      {
        title: 'Web APIs',
        description:
          'DOM, events, storage, network, navigation, devices, and browser capabilities.',
        icon: 'lucide:blocks',
        tone: 'api',
        query: 'Web APIs',
        workspaceSlug: 'web-apis',
      },
      {
        title: 'HTTP',
        description: 'Requests, responses, headers, caching, cookies, status codes, and security.',
        icon: 'lucide:radio-tower',
        tone: 'http',
        query: 'HTTP',
        workspaceSlug: 'http',
      },
      {
        title: 'URI',
        description: 'URLs, identifiers, parsing, encoding, origins, and navigation targets.',
        icon: 'lucide:link',
        tone: 'http',
        query: 'URI',
      },
      {
        title: 'WebDriver',
        description: 'Browser automation, sessions, commands, and cross-browser testing.',
        icon: 'lucide:bot',
        tone: 'tooling',
        query: 'WebDriver',
      },
      {
        title: 'Web Extensions',
        description:
          'Extension APIs, manifests, permissions, content scripts, and browser add-ons.',
        icon: 'lucide:puzzle',
        tone: 'tooling',
        query: 'Web Extensions',
      },
      {
        title: 'Web App Manifests',
        description: 'Installable web app metadata, icons, display modes, and app identity.',
        icon: 'lucide:badge-check',
        tone: 'api',
        query: 'Web App Manifests',
      },
      {
        title: 'PWA',
        description: 'Installability, offline behavior, service workers, caching, and app UX.',
        icon: 'lucide:smartphone',
        tone: 'api',
        query: 'PWA',
      },
      {
        title: 'OpenSearch',
        description: 'Search engine discovery metadata and browser search integration.',
        icon: 'lucide:scan-search',
        tone: 'tooling',
        query: 'OpenSearch',
      },
    ],
  },
  {
    title: 'Trust and quality',
    description: 'Cross-cutting practices that shape safe, inclusive, resilient products.',
    items: [
      {
        title: 'Accessibility',
        description: 'Semantic UI, keyboard flows, ARIA, contrast, and assistive technology.',
        icon: 'lucide:accessibility',
        tone: 'a11y',
        query: 'Accessibility',
        workspaceSlug: 'accessibility',
      },
      {
        title: 'Performance',
        description: 'Loading, rendering, Core Web Vitals, runtime cost, and user-perceived speed.',
        icon: 'lucide:gauge',
        tone: 'perf',
        query: 'Performance',
        workspaceSlug: 'performance',
      },
      {
        title: 'Privacy',
        description:
          'Tracking prevention, permissions, storage boundaries, and user data controls.',
        icon: 'lucide:eye-off',
        tone: 'privacy',
        query: 'Privacy',
      },
      {
        title: 'Security',
        description: 'Browser security model, CSP, secure contexts, XSS defenses, and isolation.',
        icon: 'lucide:shield-check',
        tone: 'security',
        query: 'Security',
      },
    ],
  },
  {
    title: 'Media and graphics',
    description: 'Documents beyond text: images, audio, video, vector graphics, and formulas.',
    items: [
      {
        title: 'Media',
        description: 'Audio, video, codecs, capture, playback, tracks, and media APIs.',
        icon: 'lucide:film',
        tone: 'media',
        query: 'Media',
      },
      {
        title: 'SVG',
        description: 'Vector graphics, paths, shapes, filters, symbols, and interactive diagrams.',
        icon: 'lucide:pen-tool',
        tone: 'graphics',
        query: 'SVG',
      },
      {
        title: 'MathML',
        description: 'Semantic math notation for equations and technical documents on the web.',
        icon: 'lucide:sigma',
        tone: 'graphics',
        query: 'MathML',
      },
    ],
  },
  {
    title: 'Data and documents',
    description: 'Structured data formats, document transformation, and query languages.',
    items: [
      {
        title: 'XML',
        description: 'Structured markup, namespaces, parsing, serialization, and document data.',
        icon: 'lucide:file-text',
        tone: 'data',
        query: 'XML',
      },
      {
        title: 'XSLT',
        description: 'Transform XML documents into other structures and presentation formats.',
        icon: 'lucide:workflow',
        tone: 'data',
        query: 'XSLT',
      },
      {
        title: 'XPath',
        description: 'Select and query nodes inside XML and structured document trees.',
        icon: 'lucide:route',
        tone: 'data',
        query: 'XPath',
      },
      {
        title: 'EXSLT',
        description: 'Community extensions for XSLT and XPath transformation workflows.',
        icon: 'lucide:package-plus',
        tone: 'data',
        query: 'EXSLT',
      },
    ],
  },
]

export const sectionWorkspaces: SectionWorkspace[] = [
  {
    slug: 'web-apis',
    title: 'Web APIs',
    eyebrow: 'interfaces',
    summary:
      'Browser capabilities for apps that react to users, the network, storage, devices, and navigation.',
    icon: 'lucide:blocks',
    tone: 'api',
    searchQuery: 'Web APIs',
    stats: [
      { label: 'mode', value: 'reference + learn' },
      { label: 'clusters', value: 'DOM · Fetch · Storage' },
      { label: 'source', value: 'MDN' },
    ],
    tracks: [
      {
        title: 'Start with the browser runtime',
        description: 'Understand Window, Document, events, and how APIs surface browser state.',
        steps: ['Window', 'Document', 'Events', 'EventTarget'],
      },
      {
        title: 'Move data through the network',
        description: 'Learn request lifecycles, response handling, cancellation, and streaming.',
        steps: ['Fetch', 'Headers', 'AbortController', 'Streams'],
      },
    ],
    clusters: [
      {
        title: 'DOM and events',
        description: 'The interaction layer for documents and UI.',
        topics: ['Document', 'Element', 'EventTarget', 'MutationObserver'],
      },
      {
        title: 'Storage and offline',
        description: 'State that survives reloads and unreliable connections.',
        topics: ['localStorage', 'IndexedDB', 'Cache API', 'Service Worker'],
      },
      {
        title: 'Navigation and history',
        description: 'Modern app navigation, URLs, history entries, and transitions.',
        topics: ['Navigation API', 'History API', 'URL', 'View Transition API'],
      },
    ],
    reference: [
      { title: 'Fetch API', meta: 'network · promises' },
      { title: 'Intersection Observer', meta: 'visibility · performance' },
      { title: 'Resize Observer', meta: 'layout · measurement' },
      { title: 'Web Storage API', meta: 'storage · sync' },
    ],
    featured: [
      {
        title: 'Navigation API',
        category: 'Web APIs',
        description: 'Intercept and manage browser navigations in app-like experiences.',
        href: 'https://developer.mozilla.org/en-US/docs/Web/API/Navigation_API',
      },
    ],
  },
  {
    slug: 'javascript',
    title: 'JavaScript',
    eyebrow: 'language',
    summary:
      'The programming layer of the web: values, control flow, async work, modules, and browser integration.',
    icon: 'lucide:braces',
    tone: 'js',
    searchQuery: 'JavaScript',
    stats: [
      { label: 'mode', value: 'language + runtime' },
      { label: 'clusters', value: 'Async · Objects · Modules' },
      { label: 'source', value: 'MDN' },
    ],
    tracks: [
      {
        title: 'Refresh the core language',
        description: 'Rebuild the mental model for values, scope, objects, and functions.',
        steps: ['Values', 'Scope', 'Objects', 'Functions'],
      },
      {
        title: 'Understand async JavaScript',
        description: 'Connect promises, async functions, microtasks, and browser APIs.',
        steps: ['Promises', 'async/await', 'Event loop', 'fetch()'],
      },
    ],
    clusters: [
      {
        title: 'Async flow',
        description: 'How JavaScript waits, schedules, and resumes work.',
        topics: ['Promise', 'async function', 'Microtask', 'AbortController'],
      },
      {
        title: 'Objects and prototypes',
        description: 'The object model underneath classes and built-ins.',
        topics: ['Object', 'Prototype', 'Class', 'this'],
      },
    ],
    reference: [
      { title: 'Promise', meta: 'async · control flow' },
      { title: 'Array', meta: 'collections · methods' },
      { title: 'Map', meta: 'collections · key-value' },
      { title: 'Modules', meta: 'import · export' },
    ],
    featured: [
      {
        title: 'JavaScript modules',
        category: 'JavaScript',
        description: 'Structure browser and application code with import/export boundaries.',
        href: 'https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules',
      },
    ],
  },
  {
    slug: 'css',
    title: 'CSS',
    eyebrow: 'styling',
    summary:
      'The visual system of the web: layout, cascade, selectors, animation, responsive design, and UI polish.',
    icon: 'lucide:palette',
    tone: 'css',
    searchQuery: 'CSS',
    stats: [
      { label: 'mode', value: 'layout + visual system' },
      { label: 'clusters', value: 'Grid · Cascade · Motion' },
      { label: 'source', value: 'MDN' },
    ],
    tracks: [
      {
        title: 'Build a layout vocabulary',
        description: 'Start with flow layout, then layer flex, grid, and responsive constraints.',
        steps: ['Flow', 'Flexbox', 'Grid', 'Container queries'],
      },
      {
        title: 'Make the cascade predictable',
        description: 'Learn specificity, inheritance, layers, custom properties, and scoping.',
        steps: ['Specificity', 'Inheritance', '@layer', 'Custom properties'],
      },
    ],
    clusters: [
      {
        title: 'Layout systems',
        description: 'Tools for placing content with intent and constraints.',
        topics: ['Flexbox', 'Grid', 'Subgrid', 'Box alignment'],
      },
      {
        title: 'Responsive UI',
        description: 'Adapting components to containers, media, and input modes.',
        topics: ['Media queries', 'Container queries', 'minmax()', 'clamp()'],
      },
      {
        title: 'Motion and states',
        description: 'Transitions, animations, and stateful interaction styling.',
        topics: ['Transitions', 'Animations', ':has()', 'View transitions'],
      },
    ],
    reference: [
      { title: 'display', meta: 'layout · formatting context' },
      { title: 'grid-template-columns', meta: 'grid · tracks' },
      { title: 'position', meta: 'layout · placement' },
      { title: 'color-mix()', meta: 'color · functions' },
    ],
    featured: [
      {
        title: 'CSS anchor positioning',
        category: 'CSS',
        description: 'Position UI relative to another element without custom geometry code.',
        href: 'https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_anchor_positioning',
      },
    ],
  },
  {
    slug: 'html',
    title: 'HTML',
    eyebrow: 'structure',
    summary:
      'The semantic foundation of web documents: content structure, forms, media, metadata, and accessibility.',
    icon: 'lucide:file-code-2',
    tone: 'html',
    searchQuery: 'HTML',
    stats: [
      { label: 'mode', value: 'semantics + documents' },
      { label: 'clusters', value: 'Forms · Media · Metadata' },
      { label: 'source', value: 'MDN' },
    ],
    tracks: [
      {
        title: 'Structure documents semantically',
        description: 'Use elements that communicate meaning to browsers, users, and tools.',
        steps: ['Headings', 'Sections', 'Landmarks', 'Links'],
      },
      {
        title: 'Build forms that behave',
        description: 'Connect labels, validation, input types, and accessible error states.',
        steps: ['label', 'input', 'Constraint validation', 'FormData'],
      },
    ],
    clusters: [
      {
        title: 'Document structure',
        description: 'Elements that define page meaning and navigation.',
        topics: ['main', 'article', 'section', 'nav'],
      },
      {
        title: 'Forms',
        description: 'Collecting user input with native behavior and validation.',
        topics: ['form', 'input', 'select', 'button'],
      },
    ],
    reference: [
      { title: '<form>', meta: 'forms · submission' },
      { title: '<dialog>', meta: 'interactive · modal' },
      { title: '<template>', meta: 'markup · cloning' },
      { title: '<picture>', meta: 'media · responsive' },
    ],
    featured: [
      {
        title: 'HTML forms guide',
        category: 'HTML',
        description: 'A practical path through native form controls and validation behavior.',
        href: 'https://developer.mozilla.org/en-US/docs/Learn/Forms',
      },
    ],
  },
  {
    slug: 'http',
    title: 'HTTP',
    eyebrow: 'protocol',
    summary:
      'The request and response layer of the web: methods, headers, caching, cookies, security, and status codes.',
    icon: 'lucide:radio-tower',
    tone: 'http',
    searchQuery: 'HTTP',
    stats: [
      { label: 'mode', value: 'protocol + debugging' },
      { label: 'clusters', value: 'Caching · Headers · Security' },
      { label: 'source', value: 'MDN' },
    ],
    tracks: [
      {
        title: 'Understand request flow',
        description:
          'Follow what browsers send, what servers return, and how status shapes behavior.',
        steps: ['Methods', 'Status codes', 'Headers', 'Bodies'],
      },
      {
        title: 'Cache intentionally',
        description: 'Use HTTP caching, validation, and freshness without accidental stale UI.',
        steps: ['Cache-Control', 'ETag', 'Vary', '304'],
      },
    ],
    clusters: [
      {
        title: 'Requests and responses',
        description: 'The basic grammar of web communication.',
        topics: ['GET', 'POST', 'Headers', 'Status codes'],
      },
      {
        title: 'Caching',
        description: 'Freshness, validation, and intermediaries.',
        topics: ['Cache-Control', 'ETag', 'Vary', 'Expires'],
      },
    ],
    reference: [
      { title: 'HTTP headers', meta: 'metadata · protocol' },
      { title: 'HTTP response status codes', meta: 'debugging · semantics' },
      { title: 'Cache-Control', meta: 'caching · freshness' },
      { title: 'Set-Cookie', meta: 'state · security' },
    ],
    featured: [
      {
        title: 'HTTP caching',
        category: 'HTTP',
        description: 'Control freshness, validation, and cache behavior across the network.',
        href: 'https://developer.mozilla.org/en-US/docs/Web/HTTP/Caching',
      },
    ],
  },
  {
    slug: 'accessibility',
    title: 'Accessibility',
    eyebrow: 'inclusive web',
    summary:
      'Practical patterns for semantic structure, keyboard interaction, ARIA, contrast, and assistive technology.',
    icon: 'lucide:accessibility',
    tone: 'a11y',
    searchQuery: 'Accessibility',
    stats: [
      { label: 'mode', value: 'semantics + interaction' },
      { label: 'clusters', value: 'ARIA · Keyboard · Contrast' },
      { label: 'source', value: 'MDN' },
    ],
    tracks: [
      {
        title: 'Start with semantic HTML',
        description: 'Use native elements and document structure before adding ARIA.',
        steps: ['Landmarks', 'Headings', 'Labels', 'Buttons'],
      },
      {
        title: 'Make interaction reachable',
        description: 'Design keyboard flows, focus states, and accessible feedback.',
        steps: ['Focus', 'Keyboard', 'Dialog', 'Live regions'],
      },
    ],
    clusters: [
      {
        title: 'ARIA patterns',
        description: 'Use roles and states only when native HTML is not enough.',
        topics: ['aria-label', 'role', 'aria-expanded', 'aria-live'],
      },
      {
        title: 'Keyboard UX',
        description: 'Navigation, focus order, shortcuts, and escape hatches.',
        topics: ['tabindex', 'focus-visible', 'Escape key', 'Roving tabindex'],
      },
    ],
    reference: [
      { title: 'ARIA', meta: 'semantics · assistive tech' },
      { title: 'Keyboard accessible', meta: 'interaction · focus' },
      { title: 'color contrast', meta: 'visual · readability' },
      { title: 'dialog accessibility', meta: 'modal · focus' },
    ],
    featured: [
      {
        title: 'Accessibility',
        category: 'Accessibility',
        description: 'MDN guidance for building interfaces more people can use.',
        href: 'https://developer.mozilla.org/en-US/docs/Web/Accessibility',
      },
    ],
  },
  {
    slug: 'performance',
    title: 'Performance',
    eyebrow: 'speed',
    summary:
      'How pages load, render, react, and stay responsive under real-world network and device constraints.',
    icon: 'lucide:gauge',
    tone: 'perf',
    searchQuery: 'Performance',
    stats: [
      { label: 'mode', value: 'loading + runtime' },
      { label: 'clusters', value: 'Vitals · Rendering · Assets' },
      { label: 'source', value: 'MDN' },
    ],
    tracks: [
      {
        title: 'Improve loading behavior',
        description: 'Prioritize critical resources and avoid blocking the first useful screen.',
        steps: ['Critical path', 'Images', 'Fonts', 'Preload'],
      },
      {
        title: 'Keep runtime smooth',
        description: 'Watch layout, scripting, long tasks, and rendering cost.',
        steps: ['Long tasks', 'Layout', 'Paint', 'INP'],
      },
    ],
    clusters: [
      {
        title: 'Core Web Vitals',
        description: 'User-centered metrics for loading, interaction, and stability.',
        topics: ['LCP', 'INP', 'CLS', 'PerformanceObserver'],
      },
      {
        title: 'Rendering cost',
        description: 'How CSS, layout, paint, and compositing affect responsiveness.',
        topics: ['Reflow', 'Paint', 'Compositing', 'will-change'],
      },
    ],
    reference: [
      { title: 'Performance API', meta: 'measurement · browser' },
      { title: 'PerformanceObserver', meta: 'metrics · runtime' },
      { title: 'Lazy loading', meta: 'assets · loading' },
      { title: 'Critical rendering path', meta: 'rendering · loading' },
    ],
    featured: [
      {
        title: 'Web performance',
        category: 'Performance',
        description: 'MDN guides for measuring and improving real-world web speed.',
        href: 'https://developer.mozilla.org/en-US/docs/Web/Performance',
      },
    ],
  },
]

export const findSectionWorkspace = (slug: string | string[] | undefined) => {
  const normalizedSlug = Array.isArray(slug) ? slug[0] : slug

  return sectionWorkspaces.find((section) => section.slug === normalizedSlug)
}
