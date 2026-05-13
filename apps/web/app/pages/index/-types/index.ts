export interface HomeSection {
  slug: string
  title: string
  eyebrow: string
  description: string
  icon: string
  tone: 'api' | 'js' | 'css' | 'html' | 'http' | 'a11y' | 'perf'
  query: string
  tracks: string[]
}

export interface HomeArticle {
  title: string
  category: string
  description: string
  href: string
}

export interface HomeLinkGroup {
  title: string
  links: Array<{
    label: string
    href: string
  }>
}
