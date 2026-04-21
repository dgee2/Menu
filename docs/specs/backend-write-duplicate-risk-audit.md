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

## Summary

- Every backend write endpoint is accounted for: one ingredient insert path, one recipe insert path, and one recipe update path.
- Persisted duplicate-row risk is currently concentrated in the base tables that use surrogate keys only: `Ingredient` and `Recipe`.
- Join tables (`IngredientUnit`, `RecipeIngredient`) already have composite primary keys, so persisted duplicates are blocked there, but duplicate attempts can still be constructed by request payloads.
