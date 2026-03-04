namespace Api.Errors;

public static class ProblemDetailsMetadata
{
  public static string GetTitle(int statusCode)
    => statusCode switch
    {
      StatusCodes.Status400BadRequest => "Bad Request",
      StatusCodes.Status401Unauthorized => "Unauthorized",
      StatusCodes.Status403Forbidden => "Forbidden",
      StatusCodes.Status404NotFound => "Not Found",
      StatusCodes.Status409Conflict => "Conflict",
      _ => "Internal Server Error",
    };

  public static string GetTypeLink(int statusCode)
    => statusCode switch
    {
      StatusCodes.Status400BadRequest => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1",
      StatusCodes.Status401Unauthorized => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.2",
      StatusCodes.Status403Forbidden => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.4",
      StatusCodes.Status404NotFound => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.5",
      StatusCodes.Status409Conflict => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.10",
      _ => "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.1",
    };
}
