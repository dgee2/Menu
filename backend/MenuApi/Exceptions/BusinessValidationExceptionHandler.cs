namespace MenuApi.Exceptions;

public class BusinessValidationExceptionHandler : ProblemDetailsExceptionHandler<BusinessValidationException>
{
    protected override int StatusCode => StatusCodes.Status422UnprocessableEntity;

    protected override string Title => "Unprocessable Entity";

    protected override string Type => "https://tools.ietf.org/html/rfc9110#section-15.5.21";
}
