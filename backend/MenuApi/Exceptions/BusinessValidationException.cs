namespace MenuApi.Exceptions;

public class BusinessValidationException : Exception
{
    public BusinessValidationException(string message) : base(message) { }
}
