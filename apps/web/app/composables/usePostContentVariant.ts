export type PostContentVariant = 'original' | 'ai_simple' | 'ai_senior'

const STORAGE_KEY = 'dt:post-content-variant'

const isValidVariant = (value: unknown): value is PostContentVariant =>
  value === 'original' || value === 'ai_simple' || value === 'ai_senior'

const variant = useLocalStorage<PostContentVariant>(STORAGE_KEY, 'original', {
  initOnMounted: true,
  shallow: true,
  serializer: {
    read: (value): PostContentVariant => (isValidVariant(value) ? value : 'original'),
    write: (value) => value,
  },
})

export const usePostContentVariant = () => variant
