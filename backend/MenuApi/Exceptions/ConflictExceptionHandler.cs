namespace MenuApi.Exceptions;

public class ConflictExceptionHandler : ProblemDetailsExceptionHandler<ConflictException>
{
    protected override int StatusCode => StatusCodes.Status409Conflict;

    protected override string Title => "Conflict";

    protected override string Type => "https://tools.ietf.org/html/rfc9110#section-15.5.10";
}
