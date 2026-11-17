using System.Text.Json.Nodes;
using MenuApi.Recipes;
using MenuApi.ValueObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MenuApi.OpenApi;

internal sealed class MenuApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        if (!metadata.OfType<IAuthorizeData>().Any() || metadata.OfType<IAllowAnonymous>().Any())
        {
            return;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, context.Document)] = [],
            },
        ];

        var responses = operation.Responses ??= [];
        responses["401"] = new OpenApiResponse { Description = "Unauthorized" };
        responses["500"] = new OpenApiResponse
        {
            Description = "Internal server error. The body can be empty if the Accept header excludes JSON.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository),
                },
            },
            Extensions = AllowEmptyBody(),
        };

        var canFailBinding = operation.RequestBody is not null;
        foreach (var parameter in operation.Parameters?.OfType<OpenApiParameter>() ?? [])
        {
            if (parameter.Schema is not OpenApiSchema schema)
            {
                continue;
            }

            switch (parameter.In, parameter.Name)
            {
                case (ParameterLocation.Path, "recipeId"):
                    schema.Pattern = MenuApiSchemaFilter.CanonicalGuidPattern;
                    canFailBinding = true;
                    break;
                case (ParameterLocation.Query, "scope") when operation.OperationId == RecipeApi.GetRecipesOperationId:
                    parameter.Required = true;
                    parameter.Description = $"One of {string.Join(", ", RecipeListScopeParser.AllValues)}, matched case-insensitively.";
                    schema.Pattern = CaseInsensitivePattern(RecipeListScopeParser.AllValues);
                    break;
                case (ParameterLocation.Query, "take") when operation.OperationId == RecipeApi.GetRecipesOperationId:
                    schema.Default = JsonValue.Create(RecipeApi.DefaultTake);
                    canFailBinding = true;
                    break;
            }
        }

        if (canFailBinding)
        {
            if (responses.TryGetValue("400", out var response) && response is OpenApiResponse badRequest)
            {
                badRequest.Description = "Validation error, or an empty response when request binding fails.";
                badRequest.Extensions = AllowEmptyBody();
            }
            else
            {
                responses["400"] = new OpenApiResponse { Description = "Request binding failed" };
            }
        }

        if (operation.RequestBody is not null)
        {
            responses["415"] = new OpenApiResponse { Description = "Unsupported Media Type" };
        }
    }

    private static string CaseInsensitivePattern(IEnumerable<string> values)
    {
        var alternatives = values.Select(value => string.Concat(
            value.Select(character => $"[{character}{char.ToUpperInvariant(character)}]")));
        return $"^(?:{string.Join("|", alternatives)})(?![\\s\\S])";
    }

    // OpenAPI cannot express a response whose content is optional without an extension.
    private static Dictionary<string, IOpenApiExtension> AllowEmptyBody() =>
        new() { ["x-allow-empty-body"] = new JsonNodeExtension(JsonValue.Create(true)) };
}
