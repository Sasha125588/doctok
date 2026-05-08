import { useRouteQuery } from '@vueuse/router'

export type FeedMode = 'focus' | 'browse'

interface FeedRoutePost {
  id: number
  topicSlug: string
}

interface FeedRouteNavigateOptions {
  mode?: FeedMode
  replace?: boolean
}

export const useFeedRouteState = () => {
  const topicSlug = useRouteQuery<string | null>('topicSlug', null, {
    transform: {
      get: (value) => (value?.trim() ? value : null),
      set: (value) => value,
    },
  })
  const postId = useRouteQuery('postId', null, {
    transform: {
      get: (value) => {
        if (!value) return null

        const id = Number(value)
        return Number.isFinite(id) ? id : null
      },
    },
  })
  const mode = useRouteQuery<FeedMode>('mode', 'focus')

  const setMode = (nextMode: FeedMode) => {
    if (mode.value === nextMode) return
    mode.value = nextMode
  }

  const clearPostId = () => (postId.value = null)

  const openTopic = (slug: string, options: FeedRouteNavigateOptions = {}) =>
    navigateTo(
      {
        name: 'feed',
        query: {
          topicSlug: slug,
          mode: options.mode ?? mode.value,
        },
      },
      { replace: options.replace ?? true }
    )

  const openPost = ({ id, topicSlug }: FeedRoutePost, options: FeedRouteNavigateOptions = {}) =>
    navigateTo(
      {
        name: 'feed',
        query: {
          topicSlug: topicSlug,
          postId: id,
          mode: options.mode ?? 'focus',
        },
      },
      { replace: options.replace ?? true }
    )

  return {
    topicSlug,
    postId,
    mode,
    setMode,
    clearPostId,
    openTopic,
    openPost,
  }
}
