namespace Domain.Topics;

public sealed record TopicFeedPageView(
  long Id,
  string Slug,
  string Title,
  int PostCount,
  double? Popularity,
  DateTime CreatedAt);
