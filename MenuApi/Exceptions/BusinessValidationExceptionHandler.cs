using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MenuApi.Exceptions;

public class BusinessValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessValidationException bve)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Unprocessable Entity",
            Detail = bve.Message,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.21"
        }, cancellationToken);

        return true;
    }
}
