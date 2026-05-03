# Project Context

- **Owner:** Daniel Gee
- **Project:** Menu
- **Stack:** Vue 3, Quasar, TypeScript, C#/.NET, EF Core, Aspire
- **Description:** Recipe management app for customers to save and access recipes easily.
- **Created:** 2026-05-03T21:56:25.759+01:00

## Team Assignments (2026-05-03T21:56:25.759+01:00)

### Active Work

**Issues:** #977, #978, #979, #980 (duplicate prevention epic)

- **#977** (Prevent duplicate inserts — set-like) — PR opened / clean handoff
- **#978** (Prevent duplicate inserts — canonical) — Implemented
- **#979** (Add uniqueness constraints) — Depends on #977/#978
- **#980** (Expose explicit duplicate conflicts) — Final step

**Label:** `squad:basher`

## Learnings

- Backend stack is C#/.NET with Aspire orchestration.
- Backend code lives under `backend/`.
- Recipe persistence and API behavior are core product responsibilities.
- Duplicate handling work is sequenced: service/repo logic → schema constraints → API contract.
- Exact duplicate handling for set-like writes now normalizes request payloads in `backend/MenuApi/Services/IngredientService.cs` and `backend/MenuApi/Services/RecipeService.cs` before repository calls.
- Repository write paths also stay defensive: `backend/MenuApi/Repositories/IngredientRepository.cs` deduplicates `UnitIds`, and `backend/MenuApi/Repositories/RecipeRepository.cs` deduplicates exact `RecipeIngredient` records before upsert.
- Regression coverage for duplicate-equivalent write requests lives in `backend/MenuApi.Tests/Services/IngredientServiceTests.cs`, `backend/MenuApi.Tests/Repositories/RecipeRepositoryTests.cs`, `backend/MenuApi.Tests/Services/RecipeServiceTests.cs`, `backend/MenuApi.Integration.Tests/IngredientIntegrationTests.cs`, and `backend/MenuApi.Integration.Tests/RecipeWithIngredientsIntegrationTests.cs`.
- Canonical ingredient writes now normalize `UnitIds` in `backend/MenuApi/Services/IngredientService.cs` before repository comparison so equivalent payloads match existing rows regardless of order or repetition.
- `backend/MenuApi/Repositories/IngredientRepository.cs` now performs lookup-before-insert by ingredient name, reuses equivalent canonical rows, and blocks differing unit-set redefinitions before a duplicate row is written.
- Canonical-row reuse coverage lives in `backend/MenuApi.Tests/Services/IngredientServiceTests.cs`, `backend/MenuApi.Tests/Repositories/IngredientRepositoryTests.cs`, and `backend/MenuApi.Integration.Tests/IngredientIntegrationTests.cs`.

### Issue #977: Duplicate Prevention (2026-05-03T21:56:25.759+01:00)

- **Status:** ✅ PR opened / clean handoff
- **Decision:** Treat exact duplicate entries on set-like writes as idempotent in both service and repository layers
- **Scope:** `POST /api/ingredient`, `POST /api/recipe`, and `PUT /api/recipe/{recipeId}`
- **Rationale:** Normalization keeps existing success responses unchanged; prevents duplicate inserts against composite-key child tables

### Issue #978: Canonical Ingredient Reuse (2026-05-03T21:56:25.759+01:00)

- **Status:** ✅ Implemented
- **Decision:** Canonical ingredient creation now reuses an existing same-name row only when the normalized unit-id set is equivalent; otherwise the write is rejected before insert.
- **Scope:** `POST /api/ingredient`
- **Rationale:** Ingredient names are later resolved by name in recipe writes, so allowing multiple persisted definitions for the same canonical ingredient would keep lookups ambiguous and block safe uniqueness constraints in #979.
