---
name: add-value-object
description: How to create a new Vogen value object in MenuApi — covering the attribute, assembly-wide defaults, EF Core value converter registration, Swagger mapping, and repository wrapping conventions.
---

# Adding a New Vogen Value Object

## Purpose

Use this skill when wrapping a primitive type in a Vogen value object. All domain identifiers and constrained values in `MenuApi` must be Vogen types — never raw `int`, `string`, or `Guid`.

## Vogen or a plain C# enum?

**Vogen for open-ended constrained values. A plain C# enum for closed sets.**

A closed set is one where the API contract is the complete list of permitted names — recipe visibility, a permission level, a status. Reach for an enum there, not a `[ValueObject<string>]`:

- Vogen's Swagger mapping (`OpenApiSchemaCustomizations.GenerateSwashbuckleMappingExtensionMethod` in `VogenDefaults.cs`) emits a `[ValueObject<string>]` as a bare `string` schema. The permitted values never reach the OpenAPI document, so the frontend has to keep a hand-written mirror of them that nothing checks — exactly the drift that `RecipeAccessScope` used to have in `recipe-api.ts`.
- A C# enum with `JsonStringEnumConverter` emits a named string-enum schema, and `openapi-typescript` turns it into `"Private" | "AuthenticatedUsers"`. The mirror can be deleted.

Precedents in `backend/MenuApi/ValueObjects/`: `RecipeAccessScope` and `RecipeListScope` are enums; `RecipeId`, `RecipeTitle` and `IngredientAmount` are Vogen types.

When the enum is also persisted, back it with a lookup table (`ValueGeneratedNever()` + `HasData`, unique index on `Name`) as `RecipeAccessScope` does, and add a drift-guard unit test asserting each enum member's numeric value and name match a seeded row — nothing in the compiler or in EF Core catches divergence. Keep the lookup id off the wire: the enum name is the contract.

## Repository facts

- Value objects live in: `backend/MenuApi/ValueObjects/`
- Assembly-wide Vogen defaults: `backend/MenuApi/VogenDefaults.cs` (note: at the project root, not inside the `ValueObjects/` folder)
- The assembly-level attribute in `VogenDefaults.cs` enables EF Core value converters and Swagger schema mapping automatically for all value objects in the assembly.

## Steps

### 1. Create the value object file

Add a new file in `backend/MenuApi/ValueObjects/`, e.g. `RecipeId.cs`:

```csharp
[ValueObject<int>]
public readonly partial struct RecipeId { }
```

For a string-backed value object:

```csharp
[ValueObject<string>]
public readonly partial struct RecipeName { }
```

Supported backing types include `int`, `long`, `string`, `Guid`, `decimal`, and any other primitive that Vogen supports.

### 2. No additional EF or Swagger registration needed

`VogenDefaults.cs` contains an assembly-level attribute that instructs Vogen to generate EF Core value converters and Swagger schema mappings for every value object in the assembly. No manual registration is required.

### 3. Use the value object in DB models and ViewModels

- **DB models** (`backend/MenuApi/DBModel/`): use the Vogen type directly as the property type.
- **ViewModels** (`backend/MenuApi/ViewModel/`): use the Vogen type for request and response DTOs.
- **EF entities** (`backend/MenuDB/Data/`): store the raw primitive; EF will use the generated value converter automatically.

### 4. Wrap and unwrap in repositories

In repository implementations, convert between the raw EF primitive and the Vogen type:

- **Wrap** (raw → Vogen): `RecipeId.From(entity.Id)`
- **Unwrap** (Vogen → raw): `recipeId.Value`

```csharp
// Wrapping when reading from EF
var recipeId = RecipeId.From(entity.Id);

// Unwrapping when writing an EF query predicate
var entity = await _context.Recipes
    .FirstOrDefaultAsync(r => r.Id == recipeId.Value)
    .ConfigureAwait(false);
```

### 5. Update Mapperly if needed

If the new value object appears in a mapping profile, open `backend/MenuApi/MappingProfiles/ViewModelMapper.cs` and add the appropriate `[MapProperty]` attribute. Mapperly handles Vogen conversions automatically when both sides use the same Vogen type, but explicit attributes are needed for renamed properties.

## Validation

```bash
cd backend
dotnet restore MenuApi.sln
dotnet build MenuApi.sln --configuration Release --no-restore
```

`TreatWarningsAsErrors` is enabled, so any Mapperly mapping gap or Vogen configuration error will surface as a build failure.
