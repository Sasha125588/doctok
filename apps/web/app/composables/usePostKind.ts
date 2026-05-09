export interface PostKindConfig {
  label: string
  icon: string
  cssColor: string
  cssColorRgb: string
}

const kinds: Record<string, PostKindConfig> = {
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
  concept: {
    label: '! Concept',
    icon: 'lucide:brain',
    cssColor: 'var(--kind-concept)',
    cssColorRgb: '168,85,247',
  },
  tip: {
    label: '* Tip',
    icon: 'lucide:lightbulb',
    cssColor: 'var(--kind-tip)',
    cssColorRgb: '234,179,8',
  },
}

const defaultKind: PostKindConfig = {
  label: '~ Post',
  icon: 'lucide:file-text',
  cssColor: 'var(--text-secondary)',
  cssColorRgb: '136,153,170',
}

export const usePostKind = (kind: MaybeRefOrGetter<string>) =>
  computed(() => kinds[toValue(kind)] ?? defaultKind)
