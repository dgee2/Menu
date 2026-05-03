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
- Confirmed business-unique columns are currently `backend/MenuDB\Data\IngredientEntity.Name` and `backend/MenuDB\Data\RecipeEntity.Name`; `backend/MenuDB\MenuDbContext.cs` now enforces both with unique indexes.
- Migration `backend/MenuDB\Migrations\20260503213945_AddBusinessUniqueIndexes.cs` now pre-checks for duplicate `Ingredient.Name` and `Recipe.Name` rows before creating unique indexes so deployments fail with actionable remediation guidance instead of opaque SQL index errors.
- Schema hardening verification now includes model-metadata tests in `backend/MenuApi.Tests\Database\MenuDbContextTests.cs` plus an idempotent migration script generation check (`dotnet ef migrations script --idempotent --project MenuDB --startup-project MenuApi`).
- Duplicate policy is now contract-visible only where meaning changes: `backend/MenuApi/Repositories/IngredientRepository.cs` throws `ConflictException` for canonical ingredient redefinition, while `backend/MenuApi/Services/RecipeService.cs` maps same-name recipe collisions to 409 conflicts and conflicting duplicate recipe ingredient amounts to 400 validation problems.
- API contract metadata for duplicate-visible writes now advertises 409 responses in `backend/MenuApi/Recipes/IngredientApi.cs` and `backend/MenuApi/Recipes/RecipeApi.cs`, and custom exception handlers in `backend/MenuApi/Exceptions/` keep conflict and validation payloads aligned with Problem Details responses.

### Issue #977: Duplicate Prevention (2026-05-03T21:56:25.759+01:00)

- **Status:** ✅ PR opened / clean handoff
- **Decision:** Treat exact duplicate entries on set-like writes as idempotent in both service and repository layers
- **Scope:** `POST /api/ingredient`, `POST /api/recipe`, and `PUT /api/recipe/{recipeId}`
- **Rationale:** Normalization keeps existing success responses unchanged; prevents duplicate inserts against composite-key child tables

### Issue #978: Canonical Ingredient Reuse (2026-05-03T21:56:25.759+01:00)

- **Status:** ✅ PR opened / clean handoff
- **Decision:** Canonical ingredient creation now reuses an existing same-name row only when the normalized unit-id set is equivalent; otherwise the write is rejected before insert.
- **Scope:** `POST /api/ingredient`
- **Rationale:** Ingredient names are later resolved by name in recipe writes, so allowing multiple persisted definitions for the same canonical ingredient would keep lookups ambiguous and block safe uniqueness constraints in #979.

### Issue #979: Schema Hardening (2026-05-03T21:56:25.759+01:00)

- **Status:** ✅ PR opened / clean handoff
- **Decision:** Add database-enforced unique indexes for `Ingredient.Name` and `Recipe.Name`, and make the migration fail early with explicit remediation guidance if duplicate data already exists.
- **Scope:** `backend/MenuDB\MenuDbContext.cs`, `backend/MenuDB\Migrations\20260503213945_AddBusinessUniqueIndexes.cs`, and `docs/specs/backend-write-duplicate-risk-audit.md`
- **Rationale:** Application-layer normalization now covers canonical ingredients, and the remaining business-unique names need schema enforcement before duplicate handling can be completed cleanly in #980.

### Issue #980: Explicit Duplicate Conflicts (2026-05-03T21:56:25.759+01:00)

- **Status:** ✅ Implemented / validated
- **Decision:** Surface only business-significant duplicates as client-visible errors: canonical redefinitions and recipe-name collisions return 409 conflict responses, while conflicting duplicate ingredient amounts inside one recipe payload return a 400 validation problem.
- **Scope:** `POST /api/ingredient`, `POST /api/recipe`, `PUT /api/recipe/{recipeId}`, and the duplicate-specific exception handling in `backend/MenuApi/Exceptions/`.
- **Rationale:** Exact duplicate set-like items and equivalent canonical rows remain idempotent, so the API contract only changes for duplicates that would otherwise hide a real business conflict.
