export type SectionTone =
  | 'api'
  | 'js'
  | 'css'
  | 'html'
  | 'http'
  | 'a11y'
  | 'perf'
  | 'privacy'
  | 'security'
  | 'media'
  | 'graphics'
  | 'data'
  | 'tooling'
  | 'runtime'

export interface SectionTrack {
  title: string
  description: string
  steps: string[]
}

export interface SectionCluster {
  title: string
  description: string
  topics: string[]
}

export interface SectionReferenceItem {
  title: string
  meta: string
}

export interface SectionArticle {
  title: string
  category: string
  description: string
  href: string
}

export interface SectionWorkspace {
  slug: string
  title: string
  eyebrow: string
  summary: string
  icon: string
  tone: SectionTone
  searchQuery: string
  stats: Array<{
    label: string
    value: string
  }>
  tracks: SectionTrack[]
  clusters: SectionCluster[]
  reference: SectionReferenceItem[]
  featured: SectionArticle[]
}

export interface SectionIndexItem {
  title: string
  description: string
  icon: string
  tone: SectionTone
  query: string
  workspaceSlug?: string
}

export interface SectionIndexGroup {
  title: string
  description: string
  items: SectionIndexItem[]
}
