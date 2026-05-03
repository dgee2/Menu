# Basher decision — issue #980

- Date: 2026-05-03T21:56:25.759+01:00
- Decision: Surface only business-significant duplicate writes as explicit API errors: return 409 conflicts for canonical redefinitions and recipe-name collisions, and return a 400 validation problem for conflicting duplicate recipe ingredient amounts within one payload.
- Scope: `backend/MenuApi\Recipes\IngredientApi.cs`, `backend/MenuApi\Recipes\RecipeApi.cs`, `backend/MenuApi\Services\RecipeService.cs`, `backend/MenuApi\Repositories\IngredientRepository.cs`, and `backend/MenuApi\Exceptions\`.
- Rationale: The duplicate-handling policy already treats exact set-like duplicates and equivalent canonical rows as idempotent. Making only the meaning-changing cases client-visible keeps the contract narrow, aligns with the new schema guarantees from #979, and gives callers actionable problem details when a write would otherwise redefine an existing business concept.
