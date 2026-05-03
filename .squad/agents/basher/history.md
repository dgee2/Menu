# Project Context

- **Owner:** Daniel Gee
- **Project:** Menu
- **Stack:** Vue 3, Quasar, TypeScript, C#/.NET, EF Core, Aspire
- **Description:** Recipe management app for customers to save and access recipes easily.
- **Created:** 2026-05-03T21:34:28.467+01:00

## Team Assignments (2026-05-03)

### Active Work

**Issues:** #977, #978, #979, #980 (duplicate prevention epic)

- **#977** (Prevent duplicate inserts — set-like) — In Progress
- **#978** (Prevent duplicate inserts — canonical) — Ready when #977 complete
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
