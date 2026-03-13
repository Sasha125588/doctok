export type PostKindConfig = {
  label: string
  icon: string
  cssColor: string
  cssColorRgb: string
}

const kindMap: Record<string, PostKindConfig> = {
  summary: {
    label: '# Summary',
    icon: 'lucide:book-open',
    cssColor: 'var(--kind-summary)',
    cssColorRgb: '59,130,246',
  },
  example: {
    label: '> Example',
    icon: 'lucide:code',
    cssColor: 'var(--kind-example)',
    cssColorRgb: '34,197,94',
  },
  fact: {
    label: '! Fact',
    icon: 'lucide:lightbulb',
    cssColor: 'var(--kind-fact)',
    cssColorRgb: '249,115,22',
  },
}

const defaultKind: PostKindConfig = {
  label: '~ Post',
  icon: 'lucide:file-text',
  cssColor: 'var(--text-secondary)',
  cssColorRgb: '136,153,170',
}

export function getPostKind(kind: string): PostKindConfig {
  return kindMap[kind] ?? defaultKind
}
