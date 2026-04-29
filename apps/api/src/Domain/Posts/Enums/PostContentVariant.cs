using System.Text.Json.Serialization;

namespace Domain.Posts;

public enum PostContentVariant
{
  [JsonStringEnumMemberName("original")]
  Original,

  [JsonStringEnumMemberName("ai_simple")]
  AiSimple,

  [JsonStringEnumMemberName("ai_senior")]
  AiSenior,
}
