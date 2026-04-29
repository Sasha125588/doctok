export type PostContentVariant = 'original' | 'ai_simple' | 'ai_senior'

const STORAGE_KEY = 'postContentVariant'

function isValidVariant(value: unknown): value is PostContentVariant {
  return value === 'original' || value === 'ai_simple' || value === 'ai_senior'
}

export function usePostContentVariant() {
  const variant = useState<PostContentVariant>('post-content-variant', () => 'original')

  if (import.meta.client) {
    const saved = localStorage.getItem(STORAGE_KEY)
    if (isValidVariant(saved)) {
      variant.value = saved
    }

    watch(variant, (value) => {
      localStorage.setItem(STORAGE_KEY, value)
    })
  }

  return { variant }
}
