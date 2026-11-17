using System.Globalization;
using MenuApi.ValueObjects;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MenuApi.OpenApi;

internal sealed class MenuApiSchemaFilter : ISchemaFilter
{
    // Use the canonical .NET GUID spelling without imposing UUID version or variant bits.
    internal const string CanonicalGuidPattern = @"^[0-9a-fA-F]{8}(?:-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12}(?![\s\S])";

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema openApiSchema)
        {
            return;
        }

        // Vogen maps the int-backed ingredient ID to a number even though it serializes as an integer.
        if (context.Type == typeof(IngredientId))
        {
            openApiSchema.Type = JsonSchemaType.Integer;
        }

        if (openApiSchema.Format == "uuid")
        {
            openApiSchema.Format = null;
            openApiSchema.Pattern = CanonicalGuidPattern;
        }

        if (openApiSchema.Format == "int32")
        {
            openApiSchema.Minimum ??= int.MinValue.ToString(CultureInfo.InvariantCulture);
            openApiSchema.Maximum ??= int.MaxValue.ToString(CultureInfo.InvariantCulture);
        }

        if (context.Type == typeof(HttpValidationProblemDetails))
        {
            (openApiSchema.Required ??= new HashSet<string>()).Add("errors");
        }
    }
}
