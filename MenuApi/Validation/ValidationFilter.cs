using FluentValidation;

namespace MenuApi.Validation;

public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
            return await next(context);

        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null)
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["Body"] = ["Request body is required."]
                });

        try
        {
            var result = await validator.ValidateAsync(argument, context.HttpContext.RequestAborted);
            if (!result.IsValid)
            {
                return Results.ValidationProblem(
                    result.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()));
            }
        }
        catch (Exception ex) when (IsVogenValueObjectException(ex))
        {
            // Vogen value objects throw when accessing .Value on uninitialized structs
            // (e.g. missing properties in JSON). Treat as a validation failure.
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["Body"] = ["Request body contains invalid or missing fields."]
                });
        }

        return await next(context);
    }

    private static bool IsVogenValueObjectException(Exception ex) =>
        ex.Message.Contains("uninitialized Value Object", StringComparison.OrdinalIgnoreCase) ||
        ex.GetType().Name.Contains("ValueObjectValidation", StringComparison.Ordinal);
}
