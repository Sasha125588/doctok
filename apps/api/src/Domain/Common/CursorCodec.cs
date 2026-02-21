using System.Text;
using System.Text.Json;

namespace Domain.Common;

public static class CursorCodec
{
    public static string Encode(long id)
    {
        var json = JsonSerializer.Serialize(new { id });
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static long? Decode(string? cursor)
    {
      if (string.IsNullOrWhiteSpace(cursor))
      {
            return null;
      }

      var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
      var doc = JsonDocument.Parse(json);
      return doc.RootElement.GetProperty("id").GetInt64();
    }
}
