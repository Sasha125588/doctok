import type { PostItem, TopicFeedItem } from '#api/types.gen'

export function toPreviewPost(topic: TopicFeedItem): PostItem {
  return {
    id: +topic.preview.postId,
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
