import type { PostItem } from '../../client/types.gen'

export type TopicFeedPreview = {
  postId: number | string
  kind: string
  title: null | string
  body: string
}

export type TopicFeedItem = {
  slug: string
  title: string
  lang: string
  postCount: number
  preview: TopicFeedPreview
}

export type TopicFeedResponse = {
  items: TopicFeedItem[]
  nextCursor: null | string
}

export function toPreviewPost(topic: TopicFeedItem): PostItem {
  return {
    id: topic.preview.postId,
    kind: topic.preview.kind,
    title: topic.preview.title ?? topic.title,
    body: topic.preview.body,
    position: 0,
    likeCount: 0,
    dislikeCount: 0,
    commentCount: 0,
    topicSlug: topic.slug,
    topicTitle: topic.title,
    myVote: 'none',
    popularity: null,
  }
}
