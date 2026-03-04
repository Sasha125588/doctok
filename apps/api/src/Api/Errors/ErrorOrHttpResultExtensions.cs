using ErrorOr;

namespace Api.Errors;

public static class ErrorOrHttpResultExtensions
{
  public static IResult ToResponse<TValue>(
    this ErrorOr<TValue> result,
    Func<TValue, IResult> onSuccess)
  {
    return result.Match(onSuccess, ToProblem);
  }

  public static IResult ToProblem(this List<Error> errors)
  {
    if (errors.Count == 0)
    {
      return Results.Problem(
        statusCode: StatusCodes.Status500InternalServerError,
        title: ProblemDetailsMetadata.GetTitle(StatusCodes.Status500InternalServerError),
        type: ProblemDetailsMetadata.GetTypeLink(StatusCodes.Status500InternalServerError));
    }

    var statusCode = GetStatusCode(errors[0].Type);
    var detail = errors.Count == 1
      ? errors[0].Description
      : "One or more errors occurred.";

    var extensions = new Dictionary<string, object?>
    {
      ["errors"] = errors.Select(error => new
      {
        code = error.Code, description = error.Description, type = error.Type.ToString(),
      }).ToArray(),
    };

    return Results.Problem(
      statusCode: statusCode,
      title: ProblemDetailsMetadata.GetTitle(statusCode),
      detail: detail,
      type: ProblemDetailsMetadata.GetTypeLink(statusCode),
      extensions: extensions);
  }

  private static int GetStatusCode(ErrorType errorType)
    => errorType switch
    {
      ErrorType.Validation => StatusCodes.Status400BadRequest,
      ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
      ErrorType.Forbidden => StatusCodes.Status403Forbidden,
      ErrorType.NotFound => StatusCodes.Status404NotFound,
      ErrorType.Conflict => StatusCodes.Status409Conflict,
      _ => StatusCodes.Status500InternalServerError,
    };
}
