---
name: "duplicate-conflict-problem-details"
description: "Expose only business-significant duplicate writes as 409 conflict or 400 validation Problem Details responses"
domain: "backend"
confidence: "high"
source: "earned"
---

## Context
Use this pattern when Menu's duplicate-handling policy distinguishes between idempotent duplicates that should be silently normalized and duplicate inputs that would change business meaning if they were accepted or collapsed.

## Patterns
- Keep exact set-like duplicates and equivalent canonical writes idempotent; do not surface them as client-visible errors.
- Throw `ConflictException` when a duplicate collides with an existing canonical or business-unique resource definition, such as a same-name ingredient with a different unit set or a same-name recipe targeting a different record.
- Throw `RequestValidationException` when the conflict exists entirely inside one request payload, such as duplicate recipe ingredient keys carrying different amounts.
- Register dedicated exception handlers that emit RFC 9110 Problem Details / ValidationProblemDetails responses, and advertise the new 409 response shapes in endpoint metadata where the contract is intentionally changing.
- Cover both success-path normalization and explicit duplicate errors with integration tests so the API contract stays narrow and deliberate.

## Examples
```csharp
if (await recipeRepository.RecipeNameExistsAsync(newRecipe.Name, recipeId).ConfigureAwait(false))
{
    throw new ConflictException($"Recipe '{newRecipe.Name.Value}' already exists.");
}

if (conflictingDuplicates.Length != 0)
{
    throw new RequestValidationException(new Dictionary<string, string[]>
    {
        ["ingredients"] = conflictingDuplicates,
    });
}
```

## Anti-Patterns
- Returning 409 for duplicate-equivalent requests that should remain idempotent.
- Silently collapsing conflicting duplicates inside a single payload and hiding the caller mistake.
- Reusing generic 422 business-validation responses when the API now has a clearer duplicate-conflict contract.
