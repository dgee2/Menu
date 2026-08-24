---
name: add-api-endpoint
description: Step-by-step guide for adding a new Minimal API endpoint to MenuApi following the layered model pattern (EF Entity → DBModel → ViewModel, Mapperly, repository, service, MapGroup registration).
---

# Adding a New API Endpoint

## Purpose

Use this skill when adding any new HTTP endpoint to `MenuApi`. It enforces the layered model pattern and prevents common mistakes such as layer-mixing, missing Mapperly attributes, or unregistered DI services.

## Repository facts

- Backend solution root: `backend/`
- API project: `backend/MenuApi/`
- DB project: `backend/MenuDB/`

## The three model layers

Never mix these layers. Each has a distinct location and purpose:

| Layer | Location | Purpose |
|---|---|---|
| EF Entities | `backend/MenuDB/Data/` | Database rows (e.g. `RecipeEntity`) |
| DB Models | `backend/MenuApi/DBModel/` | Intermediate records with Vogen value objects (e.g. `DBModel.Recipe`) |
| ViewModels | `backend/MenuApi/ViewModel/` | API request/response DTOs (e.g. `ViewModel.Recipe`, `NewRecipe`) |

## Steps

### 1. Add ViewModel DTOs

Create or update files in `backend/MenuApi/ViewModel/`. Each DTO is a C# `class` (not a `record`) to support DataAnnotations such as `[Required]`. Use Vogen value objects for all typed properties (see `add-value-object` skill); never use raw `int`, `string`, or `Guid` for domain identifiers or constrained values.

### 2. Add DB model records (if needed)

If the endpoint requires a new data shape that differs from existing DB models, create the corresponding `record` in `backend/MenuApi/DBModel/`.

### 3. Update Mapperly mappings

Open `backend/MenuApi/MappingProfiles/ViewModelMapper.cs` and add or update `[MapProperty]` attributes for any renamed or new properties. Mapperly is source-generated and zero-reflection; missing attributes cause compile errors that `TreatWarningsAsErrors` will surface immediately.

### 4. Add the repository method

- Declare the method signature on the interface: `backend/MenuApi/Repositories/I<Name>Repository.cs`
- Implement it in: `backend/MenuApi/Repositories/<Name>Repository.cs`
- Use `.Value` to unwrap Vogen types when writing EF queries.
- Use `TypeName.From(x)` to wrap raw values returned from EF into Vogen types.
- Add `ConfigureAwait(false)` on every `await` call.

### 5. Add the service method

- Declare the method on the interface: `backend/MenuApi/Services/I<Name>Service.cs`
- Implement it in: `backend/MenuApi/Services/<Name>Service.cs`
- Services call repositories and apply business rules. Add `ConfigureAwait(false)` on every `await` call.

### 6. Register the endpoint

Add the endpoint in the relevant `backend/MenuApi/Recipes/*Api.cs` file using the `MapGroup` pattern. Use `.Produces<T>(statusCode)` and `.ProducesProblem(statusCode)` to document response types — the existing endpoints do not use `.WithName()` or `.WithOpenApi()`. Example:

```csharp
group.MapGet("/{id}", GetByIdAsync)
    .Produces<FullRecipe>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);
```

### 7. Register DI services

Add any new repository and service registrations to `backend/MenuApi/Program.cs`:

```csharp
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
```

## Validation

After implementing, run from `backend/`:

```bash
dotnet restore MenuApi.sln
dotnet build MenuApi.sln --configuration Release --no-restore
dotnet test --project MenuApi.Tests/MenuApi.Tests.csproj --configuration Release --no-build
```

Then regenerate the OpenAPI spec and validate the frontend (see `openapi-sync` skill):

```bash
cd ../ui/menu-website
pnpm run generate-openapi
pnpm run lint
pnpm run build
```
