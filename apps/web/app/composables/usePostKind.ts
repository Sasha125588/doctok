import { getPostKind } from '~/lib/post-kind'

import type { PostKindConfig } from '~/lib/post-kind'

export function usePostKind(kind: MaybeRefOrGetter<string>): ComputedRef<PostKindConfig> {
  return computed(() => getPostKind(toValue(kind)))
}
