namespace MenuApi.Exceptions;

public class ForbiddenAccessExceptionHandler : ProblemDetailsExceptionHandler<ForbiddenAccessException>
{
    protected override int StatusCode => StatusCodes.Status403Forbidden;

    protected override string Title => "Forbidden";

    protected override string Type => "https://tools.ietf.org/html/rfc9110#section-15.5.4";
}
