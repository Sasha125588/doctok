using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Errors;

public sealed class ApiExceptionHandler(
  ILogger<ApiExceptionHandler> logger,
  IProblemDetailsService problemDetailsService)
  : IExceptionHandler
{
  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken)
  {
    var status = exception switch
    {
      ApplicationException =>  StatusCodes.Status500InternalServerError,
      ArgumentException or FormatException => StatusCodes.Status400BadRequest,
      KeyNotFoundException => StatusCodes.Status404NotFound,
      UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
      _ => StatusCodes.Status500InternalServerError,
    };

    logger.LogWarning(exception, "Handled API exception with status code {StatusCode}", status);

    httpContext.Response.StatusCode = status;

    var details = new ProblemDetails
    {
      Status = status,
      Title = ProblemDetailsMetadata.GetTitle(status),
      Detail = exception.Message,
      Type = ProblemDetailsMetadata.GetTypeLink(status),
    };

    await problemDetailsService.WriteAsync(new ProblemDetailsContext
    {
      HttpContext = httpContext,
      ProblemDetails = details,
      Exception = exception,
    });

    return true;
  }
}
