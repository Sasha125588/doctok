using System.Text;
using System.Text.Json;

namespace Domain.Common;

public sealed record FeedCursor(double? Popularity, long Id);

public static class CursorCodec
{
    public static string Encode(FeedCursor cursor)
    {
        var json = JsonSerializer.Serialize(new { p = cursor.Popularity, id = cursor.Id });
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static FeedCursor? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return null;
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var id = root.GetProperty("id").GetInt64();

            double? popularity = null;
            if (root.TryGetProperty("p", out var pEl) && pEl.ValueKind == JsonValueKind.Number)
                popularity = pEl.GetDouble();

            return new FeedCursor(popularity, id);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
