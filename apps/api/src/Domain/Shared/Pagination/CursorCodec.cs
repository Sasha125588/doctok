using System.Text;
using System.Text.Json;

namespace Domain.Shared;

public sealed record FeedCursor(double? Popularity, long Id, DateTimeOffset CreatedAt);

public sealed record TopicPostsCursor(int? KindRank, int? Position, long? Id);

public sealed record SavedPostsCursor(long? Id, DateTimeOffset? CreatedAt);

public static class CursorCodec
{
    public static string Encode<T>(T cursor)
    {
        var json = JsonSerializer.Serialize(cursor);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static T? Decode<T>(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return default;

        try
        {
          var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));

          return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
          return default;
        }
    }
}
