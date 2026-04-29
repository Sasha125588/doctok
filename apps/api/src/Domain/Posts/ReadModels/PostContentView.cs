namespace Domain.Posts;

public sealed record PostContentView(
  long PostId,
  string VariantCode,
  string Title,
  string Body,
  string BodyHtml
);
