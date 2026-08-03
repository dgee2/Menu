using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MenuApi.Exceptions;

public abstract class ProblemDetailsExceptionHandler<TException> : IExceptionHandler
    where TException : Exception
{
    protected abstract int StatusCode { get; }

    protected abstract string Title { get; }

    protected abstract string Type { get; }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not TException typedException)
            return false;

        httpContext.Response.StatusCode = StatusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCode,
            Title = Title,
            Detail = typedException.Message,
            Type = Type
        }, cancellationToken);

        return true;
    }
}
