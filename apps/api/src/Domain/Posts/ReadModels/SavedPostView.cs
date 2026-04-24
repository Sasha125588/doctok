namespace Domain.Posts;

public sealed record SavedPostView(
  long PostId,
  string Title,
  string Kind,
  string TopicSlug,
  string TopicTitle,
  DateTime SavedAt
);
