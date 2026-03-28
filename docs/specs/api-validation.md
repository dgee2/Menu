# API Request Validation Spec

**Issue:** [#886 — Validation of api requests](https://github.com/dgee2/Menu/issues/886)
**Status:** Draft
**Created:** 2026-03-28

## Problem Statement

All API requests should be validated with appropriate responses returned to the caller. Currently:

- ViewModels use only `[Required]` DataAnnotations — no length, range, or format constraints
- Business logic failures (e.g., referencing a non-existent ingredient) throw `InvalidOperationException` → 500 Internal Server Error
- `GET /api/recipe/{recipeId}` returns `null` (200) instead of 404 when the recipe doesn't exist
- Vogen value objects accept any value with no validation (empty strings, negative amounts, etc.)

## Approach

Use **FluentValidation** with Minimal API **endpoint filters** to validate all request DTOs before they reach business logic. Error responses follow **RFC 9457** (Problem Details for HTTP APIs) — the current standard (supersedes RFC 7807). ASP.NET Core's built-in `Results.ValidationProblem()` and `Results.Problem()` already produce RFC 9457-compliant responses.

## HTTP Status Code Strategy

| Scenario | Status Code | Example |
|---|---|---|
| Missing/malformed required fields | **400 Bad Request** | Empty recipe name, missing ingredients list |
| String too long, value out of range | **400 Bad Request** | Recipe name > 500 chars, amount ≤ 0 |
| Business rule violation | **422 Unprocessable Entity** | Ingredient "X" does not exist, Unit "Y" not found |
| Resource not found (GET by ID) | **404 Not Found** | `GET /api/recipe/99999` |
| Valid request, success | **200 OK** / **201 Created** | Normal operation |

## Error Response Format (RFC 9457)

All error responses use the `application/problem+json` content type.

### Validation Error (400)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["'Name' must not be empty."],
    "Ingredients": ["'Ingredients' must not be empty."]
  }
}
```

This is the standard ASP.NET Core validation problem details format produced by `Results.ValidationProblem()`.

### Business Rule Error (422)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.21",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "Ingredient 'Flour' does not exist."
}
```

### Not Found (404)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Recipe with ID 99999 was not found."
}
```

## Package Dependencies

| Package | Purpose |
|---|---|
| `FluentValidation` | Core validation library |
| `FluentValidation.DependencyInjectionExtensions` | `AddValidatorsFromAssemblyContaining<T>()` for DI registration |

> **Note:** Do _not_ use `FluentValidation.AspNetCore` — it is deprecated and targets MVC. Minimal APIs use endpoint filters instead.

## Validation Rules

### 1. `NewRecipe` Validator

| Property | Rule | Constraint Source |
|---|---|---|
| `Name` | Must not be empty/whitespace | Business logic |
| `Name` | Maximum 500 characters | DB: `Recipe.Name varchar(500)` |
| `Ingredients` | Must not be null | Required field |
| `Ingredients` | Must contain at least one item | Business logic — a recipe with no ingredients is meaningless |
| `Ingredients[*]` | Each item must pass `RecipeIngredientValidator` | Nested validation |

### 2. `RecipeIngredient` Validator

| Property | Rule | Constraint Source |
|---|---|---|
| `Name` | Must not be empty/whitespace | Business logic |
| `Name` | Maximum 50 characters | DB: `Ingredient.Name varchar(50)` |
| `Unit` | Must not be empty/whitespace | Business logic |
| `Unit` | Maximum 50 characters | DB: `Unit.Name varchar(50)` |
| `Amount` | Must be greater than 0 | Business logic — zero/negative amounts are invalid |
| `Amount` | Must have at most 10 digits total, 4 decimal places | DB: `Amount decimal(10,4)` |

### 3. `NewIngredient` Validator

| Property | Rule | Constraint Source |
|---|---|---|
| `Name` | Must not be empty/whitespace | Business logic |
| `Name` | Maximum 50 characters | DB: `Ingredient.Name varchar(50)` |
| `UnitIds` | Must not be null | Required field |
| `UnitIds` | Must contain at least one item | Business logic — an ingredient needs at least one unit |
| `UnitIds[*]` | Each value must be greater than 0 | DB: IDs are positive integers |

## Vogen Value Object Validation

Add `Validate` methods to value objects so they reject invalid values at creation time:

| Value Object | Underlying Type | Validation Rule |
|---|---|---|
| `RecipeName` | `string` | Must not be null/empty/whitespace |
| `IngredientName` | `string` | Must not be null/empty/whitespace |
| `IngredientAmount` | `decimal` | Must be greater than 0 |
| `IngredientUnitName` | `string` | Must not be null/empty/whitespace |
| `IngredientUnitAbbreviation` | `string` | No additional validation (nullable) |
| `IngredientUnitType` | `string` | Must not be null/empty/whitespace |
| `RecipeId` | `int` | Must be greater than 0 |
| `IngredientId` | `int` | Must be greater than 0 |

Vogen supports a static `Validate` method on each value object:

```csharp
[ValueObject<string>]
public readonly partial struct RecipeName
{
    private static Validation Validate(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("Recipe name must not be empty.")
            : Validation.Ok;
}
```

## Endpoint Filter Implementation

Create a generic, reusable `ValidationFilter<T>` that:

1. Extracts the `T` argument from the endpoint invocation context
2. Resolves `IValidator<T>` from DI
3. Runs validation
4. Returns `Results.ValidationProblem(errors)` (400) if validation fails
5. Calls `next(context)` if validation passes

```csharp
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

        var result = await validator.ValidateAsync(argument);
        if (!result.IsValid)
        {
            return Results.ValidationProblem(
                result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        return await next(context);
    }
}
```

Attach to endpoints:

```csharp
group.MapPost("/", CreateRecipeAsync)
    .AddEndpointFilter<ValidationFilter<NewRecipe>>();

group.MapPut("{recipeId}", UpdateRecipeAsync)
    .AddEndpointFilter<ValidationFilter<NewRecipe>>();
```

## Endpoint Changes for 404 and 422

### GET endpoints — return 404 for missing resources

**`GET /api/recipe/{recipeId}`** — Currently returns `FullRecipe?` (null → 200). Change to:

```csharp
public static async Task<IResult> GetRecipeAsync(IRecipeService recipeService, RecipeId recipeId)
{
    var recipe = await recipeService.GetRecipeAsync(recipeId);
    return recipe is not null
        ? Results.Ok(recipe)
        : Results.Problem(
            detail: $"Recipe with ID {recipeId} was not found.",
            statusCode: StatusCodes.Status404NotFound);
}
```

### Business logic errors — return 422 instead of 500

Replace `InvalidOperationException` throws in `RecipeRepository` with a result pattern or custom exception type that the endpoint can catch and convert to 422:

**Option: Custom exception + endpoint filter approach**

Define a `BusinessValidationException`:

```csharp
public class BusinessValidationException : Exception
{
    public BusinessValidationException(string message) : base(message) { }
}
```

Register a custom `IExceptionHandler` to map `BusinessValidationException` → 422. ASP.NET Core's `IExceptionHandler` pipeline runs inside `UseExceptionHandler()` and is the idiomatic way to handle specific exception types:

```csharp
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
```

Register in `Program.cs`:

```csharp
builder.Services.AddExceptionHandler<BusinessValidationExceptionHandler>();
```

The existing `app.UseExceptionHandler()` in `MapDefaultApiEndpoints` will invoke this handler automatically. Unhandled exceptions continue to fall through to the default Problem Details handler (500).

Then change the repository to throw `BusinessValidationException` instead of `InvalidOperationException`:

```csharp
if (!ingredientLookup.TryGetValue(item.IngredientName.Value, out var ingredientId))
    throw new BusinessValidationException($"Ingredient '{item.IngredientName.Value}' does not exist.");
```

## OpenAPI Documentation

Add `.Produces()` and `.ProducesValidationProblem()` to all mutating endpoints so the generated OpenAPI spec documents possible error responses:

```csharp
group.MapPost("/", CreateRecipeAsync)
    .AddEndpointFilter<ValidationFilter<NewRecipe>>()
    .Produces<FullRecipe>(StatusCodes.Status200OK)
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
```

## File Structure

New/modified files:

```
MenuApi/
├── Validation/
│   ├── ValidationFilter.cs              # Generic endpoint filter
│   ├── NewRecipeValidator.cs            # FluentValidation rules for NewRecipe
│   ├── RecipeIngredientValidator.cs     # FluentValidation rules for RecipeIngredient
│   └── NewIngredientValidator.cs        # FluentValidation rules for NewIngredient
├── Exceptions/
│   ├── BusinessValidationException.cs   # Custom exception for 422 responses
│   └── BusinessValidationExceptionHandler.cs  # IExceptionHandler → 422
├── ValueObjects/
│   ├── RecipeName.cs                    # Add Validate method
│   ├── RecipeId.cs                      # Add Validate method
│   ├── IngredientName.cs                # Add Validate method
│   ├── IngredientId.cs                  # Add Validate method
│   ├── IngredientAmount.cs              # Add Validate method
│   ├── IngredientUnitName.cs            # Add Validate method
│   └── IngredientUnitType.cs            # Add Validate method
├── Recipes/
│   ├── RecipeApi.cs                     # Add filters, 404 handling, .Produces()
│   └── IngredientApi.cs                 # Add filters, .Produces()
├── Repositories/
│   └── RecipeRepository.cs              # Throw BusinessValidationException
└── Program.cs                           # Register FluentValidation, exception handler, configure ProblemDetails
```

## Test Requirements

### Unit Tests (MenuApi.Tests)

| Test | Validates |
|---|---|
| `NewRecipeValidator_EmptyName_Fails` | Name must not be empty |
| `NewRecipeValidator_NameTooLong_Fails` | Name max 500 chars |
| `NewRecipeValidator_EmptyIngredients_Fails` | Ingredients list must have ≥1 item |
| `NewRecipeValidator_ValidRecipe_Passes` | Happy path |
| `RecipeIngredientValidator_EmptyName_Fails` | Ingredient name required |
| `RecipeIngredientValidator_NameTooLong_Fails` | Ingredient name max 50 chars |
| `RecipeIngredientValidator_EmptyUnit_Fails` | Unit required |
| `RecipeIngredientValidator_ZeroAmount_Fails` | Amount must be > 0 |
| `RecipeIngredientValidator_NegativeAmount_Fails` | Amount must be > 0 |
| `RecipeIngredientValidator_TooManyDecimalPlaces_Fails` | Max 4 decimal places |
| `RecipeIngredientValidator_ValidIngredient_Passes` | Happy path |
| `NewIngredientValidator_EmptyName_Fails` | Name required |
| `NewIngredientValidator_NameTooLong_Fails` | Name max 50 chars |
| `NewIngredientValidator_EmptyUnitIds_Fails` | UnitIds must have ≥1 item |
| `NewIngredientValidator_ZeroUnitId_Fails` | UnitIds must be > 0 |
| `NewIngredientValidator_ValidIngredient_Passes` | Happy path |
| `ValidationFilter_InvalidRequest_Returns400` | Filter returns ValidationProblem |
| `ValidationFilter_ValidRequest_CallsNext` | Filter passes through |
| `ValidationFilter_NullBody_Returns400` | Missing body handled |

### Integration Tests (MenuApi.Integration.Tests)

| Test | Validates |
|---|---|
| `CreateRecipe_EmptyBody_Returns400` | Empty/null body → 400 |
| `CreateRecipe_EmptyName_Returns400WithProblemDetails` | Validation error format is RFC 9457 |
| `CreateRecipe_NonExistentIngredient_Returns422` | Business rule → 422 |
| `GetRecipe_NonExistentId_Returns404` | Missing resource → 404 |
| `CreateIngredient_EmptyName_Returns400` | Validation error |
| `CreateIngredient_EmptyUnitIds_Returns400` | Validation error |
| `UpdateRecipe_EmptyName_Returns400` | Validation on PUT |
| `UpdateRecipe_NonExistentIngredient_Returns422` | Business rule on PUT |

## Migration Notes

- Existing `[Required]` DataAnnotations on ViewModels can remain — they serve as documentation and OpenAPI schema hints. FluentValidation will be the primary validation mechanism at runtime.
- The `ArgumentNullException.ThrowIfNull()` calls in services can remain as defensive programming guards.
- All changes are backward-compatible for valid requests — only invalid requests that previously returned 500 will now return 400/404/422.
