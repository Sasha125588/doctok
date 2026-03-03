using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Errors;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken)
  {
    var (status, title) = exception switch
    {
      ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
      FormatException => (StatusCodes.Status400BadRequest, "Bad Request"),
      KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
      UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
      _ => (0, string.Empty)
    };

    if (status == 0)
    {
      return false;
    }

    logger.LogWarning(exception, "Handled API exception with status code {StatusCode}", status);

    var details = new ProblemDetails
    {
      Status = status,
      Title = title,
      Detail = exception.Message
    };

    httpContext.Response.StatusCode = status;
    await httpContext.Response.WriteAsJsonAsync(details, cancellationToken);
    return true;
  }
}
