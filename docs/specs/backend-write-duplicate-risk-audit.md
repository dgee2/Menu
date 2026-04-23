# Backend Write-Path Duplicate-Risk Audit

**Issue:** Audit duplicate-risk across all update/insert paths
**Parent:** [#887](https://github.com/dgee2/Menu/issues/887)
**Status:** Draft
**Created:** 2026-04-21

## Scope

This audit inventories every backend write endpoint under `backend/MenuApi/Recipes/*.cs`, traces the service and repository calls they make, identifies the tables they write, and records the duplicate-row protections that currently exist in `MenuDbContext` and the initial migration.

## Write Endpoint Inventory

| Endpoint | API method | Service call chain | Repository call chain | Tables written |
|---|---|---|---|---|
| `POST /api/ingredient` | `IngredientApi.CreateIngredientAsync` | `IngredientService.CreateIngredientAsync` | `IngredientRepository.CreateIngredientAsync` | `Ingredient`, `IngredientUnit` |
| `POST /api/recipe` | `RecipeApi.CreateRecipeAsync` | `RecipeService.CreateRecipeAsync` | `RecipeRepository.CreateRecipeAsync` → `RecipeRepository.UpsertRecipeIngredientsAsync` | `Recipe`, `RecipeIngredient` |
| `PUT /api/recipe/{recipeId}` | `RecipeApi.UpdateRecipeAsync` | `RecipeService.UpdateRecipeAsync` | `RecipeRepository.UpdateRecipeAsync` → `RecipeRepository.UpsertRecipeIngredientsAsync` | `Recipe`, `RecipeIngredient` |

## Write-Path Behaviour

### `POST /api/ingredient`

- Inserts one `IngredientEntity`
- Inserts one `IngredientUnitEntity` row per submitted `UnitIds` entry
- Uses a single `SaveChangesAsync()` call
- Does not check whether another ingredient already uses the same `Name`
- Does not deduplicate `UnitIds` before building `IngredientUnitEntity` rows

### `POST /api/recipe`

- Inserts one `RecipeEntity`
- Calls `UpsertRecipeIngredientsAsync` to:
  - load existing `RecipeIngredient` rows for the recipe
  - delete rows no longer present
  - update `Amount` on existing `(RecipeId, IngredientId, UnitId)` rows
  - insert new `(RecipeId, IngredientId, UnitId)` rows
- Runs inside an EF execution strategy and explicit transaction
- Does not check whether another recipe already uses the same `Name`
- Does not deduplicate repeated `(IngredientName, UnitName)` pairs in the request payload

### `PUT /api/recipe/{recipeId}`

- Updates `Recipe.Name` with `ExecuteUpdateAsync`
- Reuses the same `UpsertRecipeIngredientsAsync` behaviour as create
- Runs inside an EF execution strategy and explicit transaction
- Does not check whether the new recipe name duplicates another recipe
- Does not deduplicate repeated `(IngredientName, UnitName)` pairs in the request payload

## Endpoint / Table / Risk Matrix

| Endpoint | Table / entity | Existing uniqueness rule | Duplicate-risk classification | Notes |
|---|---|---|---|---|
| `POST /api/ingredient` | `Ingredient` / `IngredientEntity` | Primary key on `Id` only | **High** | `Ingredient.Name` has no unique index or repository pre-check, so multiple rows with the same name can be inserted. |
| `POST /api/ingredient` | `IngredientUnit` / `IngredientUnitEntity` | Composite primary key on `(IngredientId, UnitId)` | **Low** | Stored duplicates are blocked by the composite key, but duplicate `UnitIds` in a single request can still attempt duplicate inserts and fail at save time. |
| `POST /api/recipe` | `Recipe` / `RecipeEntity` | Primary key on `Id` only | **High** | `Recipe.Name` has no unique index or repository pre-check, so multiple rows with the same name can be inserted. |
| `POST /api/recipe` | `RecipeIngredient` / `RecipeIngredientEntity` | Composite primary key on `(RecipeId, IngredientId, UnitId)` | **Low** | Stored duplicates are blocked by the composite key, but duplicate `(ingredient, unit)` pairs in one payload can still attempt duplicate inserts and fail at save time. |
| `PUT /api/recipe/{recipeId}` | `Recipe` / `RecipeEntity` | Primary key on `Id` only | **High** | Updating one recipe name to match another recipe name is allowed because no uniqueness rule exists on `Recipe.Name`. |
| `PUT /api/recipe/{recipeId}` | `RecipeIngredient` / `RecipeIngredientEntity` | Composite primary key on `(RecipeId, IngredientId, UnitId)` | **Low** | The same duplicate payload risk exists as create; the table itself still blocks persisted duplicate rows. |

## Current Uniqueness Rules

### Primary keys

| Table | Current rule |
|---|---|
| `Recipe` | Primary key on `Id` |
| `Ingredient` | Primary key on `Id` |
| `UnitType` | Primary key on `Id` |
| `Unit` | Primary key on `Id` |
| `IngredientUnit` | Composite primary key on `(IngredientId, UnitId)` |
| `RecipeIngredient` | Composite primary key on `(RecipeId, IngredientId, UnitId)` |

### Supporting non-unique indexes

| Table | Index |
|---|---|
| `IngredientUnit` | `IX_IngredientUnit_UnitId` |
| `RecipeIngredient` | `IX_RecipeIngredient_IngredientId` |
| `RecipeIngredient` | `IX_RecipeIngredient_UnitId` |
| `Unit` | `IX_Unit_UnitTypeId` |

## Uniqueness Gaps

1. `Recipe.Name` is not unique in either the EF model or the migration.
2. `Ingredient.Name` is not unique in either the EF model or the migration.
3. `Unit.Name` and `UnitType.Name` also lack unique indexes, although there are currently no write endpoints for them.
4. `NewIngredientValidator` checks that `UnitIds` are present and positive, but it does not reject duplicate unit IDs in the same request.
5. `NewRecipeValidator` and `RecipeIngredientValidator` validate shape and value ranges, but they do not reject duplicate `(ingredient, unit)` pairs in the same request.
6. The repository write paths rely on database primary keys to reject duplicate join-table rows instead of preventing duplicate attempts earlier.

## Duplicate-Handling Policy Buckets

> **Note:** The sections below describe **target / intended** behaviour, not the current runtime behaviour described above. The current write paths do not yet implement these policies.

### Set-like child / junction rows

- Applies to join rows whose identity is fully described by their parent key plus referenced child key(s)
- Duplicate input does not represent an additional business object
- **Default policy:** silently collapse exact duplicates before writing
- **Escalation:** reject the request when repeated keys carry conflicting payload for the same logical row

### Canonical / reference rows

- Applies to shared vocabulary rows that other write paths resolve by natural key
- Duplicate storage is harmful because later lookups become ambiguous
- **Default policy:** reuse the existing row when the incoming payload describes the same canonical value
- **Escalation:** reject the request when the incoming payload tries to redefine an existing canonical row

### Business-significant duplicates

- Applies to aggregates where each row is intended to represent a distinct client-visible object
- Reusing or silently collapsing these rows would hide user intent
- **Default policy:** reject duplicates explicitly rather than silently reusing or ignoring them

## Endpoint-by-Endpoint Duplicate Policy

> **Note:** These are **target / intended** behaviours. Three distinct HTTP status codes are used depending on *why* the request is rejected:
> - `400 Bad Request` — the *request payload itself* is internally inconsistent (`Results.ValidationProblem()`)
> - `409 Conflict` — the request is well-formed but conflicts with the current state of an *existing resource* (a new `ConflictException` / dedicated `IExceptionHandler` will be required; endpoints must add `.ProducesProblem(409)`)
> - `422 Unprocessable Entity` — retained for semantic processing failures such as referencing a non-existent ingredient (`BusinessValidationException`)
>
> `POST /api/ingredient` does not yet advertise `.ProducesProblem(409)` or `.ProducesProblem(422)` and would need both added when the reject cases are implemented.

| Endpoint | Duplicate scenario | Classification | Policy | HTTP response | Notes |
|---|---|---|---|---|---|
| `POST /api/ingredient` | `Ingredient.Name` matches an existing ingredient and the effective `UnitIds` set is the same after deduplication | Canonical / reference row | **Reuse existing row** | `200 OK` | Return the existing ingredient instead of inserting a second `Ingredient` row. |
| `POST /api/ingredient` | `Ingredient.Name` matches an existing ingredient but the effective `UnitIds` set differs | Canonical / reference row | **Reject** | `409 Conflict` | The request is well-formed but conflicts with the current canonical definition stored on the server. |
| `POST /api/ingredient` | Duplicate `UnitIds` within one request body | Set-like child / junction row | **Ignore duplicate input** | `200 OK` | Silently collapse repeated unit IDs before building `IngredientUnit` rows. |
| `POST /api/recipe` | `Recipe.Name` matches an existing recipe | Business-significant duplicate | **Reject** | `409 Conflict` | The request is well-formed but a recipe with that name already exists on the server. Add `.ProducesProblem(409)` alongside the existing `.ProducesProblem(422)`, since `422` is still needed for semantic failures such as missing referenced data. |
| `POST /api/recipe` | Repeated `(IngredientName, UnitName)` pair with the same `Amount` in one request body | Set-like child / junction row | **Ignore duplicate input** | `200 OK` | Silently collapse the exact duplicate before upserting `RecipeIngredient` rows. |
| `POST /api/recipe` | Repeated `(IngredientName, UnitName)` pair with a different `Amount` in one request body | Set-like child / junction row | **Reject** | `400 Bad Request` | `Results.ValidationProblem()` pattern — the payload is internally inconsistent with no conflict against server state. |
| `PUT /api/recipe/{recipeId}` | New recipe name matches a different existing recipe | Business-significant duplicate | **Reject** | `409 Conflict` | The request is well-formed but the target name already belongs to a different recipe on the server. Add `.ProducesProblem(409)` alongside the existing `.ProducesProblem(422)`, since `422` is still needed for semantic failures such as missing referenced data. |
| `PUT /api/recipe/{recipeId}` | Repeated `(IngredientName, UnitName)` pair with the same `Amount` in one request body | Set-like child / junction row | **Ignore duplicate input** | `200 OK` | Silently collapse the exact duplicate before applying the update. |
| `PUT /api/recipe/{recipeId}` | Repeated `(IngredientName, UnitName)` pair with a different `Amount` in one request body | Set-like child / junction row | **Reject** | `400 Bad Request` | `Results.ValidationProblem()` pattern — the payload is internally inconsistent with no conflict against server state. |

## Decision Record: Silent vs Explicit Handling

1. **Silent handling is limited to set-like duplicates inside a single request payload.** Repeated `UnitIds` and exact duplicate recipe ingredient keys do not create additional meaning, so the server should normalize them away before persistence.
2. **Canonical rows should be reused, not duplicated, when the incoming request matches the existing canonical definition.** `Ingredient` names are later resolved by name in recipe writes, so multiple stored rows with the same name would make repository lookups ambiguous.
3. **Canonical row redefinition must be explicit.** When a request reuses an ingredient name but changes the associated unit set, the server should reject the request rather than implicitly mutating or widening the existing canonical ingredient.
4. **Business-significant duplicates must stay client-visible.** Recipe creation and recipe rename operations should reject same-name collisions because a recipe is an end-user aggregate, not shared reference data.
5. **Conflicting duplicates inside one payload are validation failures, not candidates for silent normalization.** Two entries for the same logical `RecipeIngredient` with different amounts represent ambiguous intent and should be reported back to the client.
6. **HTTP status code selection follows resource-state semantics.** `409 Conflict` is used when a well-formed request conflicts with the *current state of an existing server resource* (duplicate recipe name, canonical ingredient redefinition). `400 Bad Request` is used when the *payload itself* is internally inconsistent (two conflicting amounts for the same recipe ingredient) — no existing resource state is involved. `422 Unprocessable Entity` is reserved for semantic failures that reference missing data (e.g., an ingredient that does not exist).
7. **Concurrency remains out of scope.** These policies describe single-request duplicate handling only; races between concurrent writers are intentionally deferred to follow-up work.

## Summary

- Every backend write endpoint is accounted for: one ingredient insert path, one recipe insert path, and one recipe update path.
- Persisted duplicate-row risk is currently concentrated in the base tables that use surrogate keys only: `Ingredient` and `Recipe`.
- Join tables (`IngredientUnit`, `RecipeIngredient`) already have composite primary keys, so persisted duplicates are blocked there, but duplicate attempts can still be constructed by request payloads.
- Every audited duplicate-risk path now has an assigned handling policy: reuse for canonical ingredient matches, silent collapse for set-like duplicates, and explicit rejection for recipe-name collisions or conflicting duplicate payload entries.
