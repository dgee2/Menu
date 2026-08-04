---
name: ef-migration
description: Canonical commands and safety rules for EF Core migration management in the Menu repository — adding, removing, and applying migrations correctly.
user-invokable: true
context: inline
---

# EF Core Migration Management

## Purpose

Use this skill when adding, removing, or reasoning about EF Core migrations in the Menu repository. The project layout requires specific flags that differ from the EF CLI defaults.

## Repository facts

- Migrations project: `backend/MenuDB` (contains `MenuDbContext` and the `Migrations/` folder)
- Startup project: `backend/MenuApi` (required for EF tool configuration)
- Always run EF CLI commands from the **backend solution root** (`backend/`), not from a project subfolder.

## Commands

### Add a migration

```bash
cd backend
dotnet ef migrations add <MigrationName> --project MenuDB --startup-project MenuApi
```

`<MigrationName>` should be PascalCase and describe the schema change, e.g. `AddRecipeIngredientsTable`.

### Remove the most recent migration (before it is applied)

```bash
cd backend
dotnet ef migrations remove --project MenuDB --startup-project MenuApi
```

Only remove a migration that has **not** been applied to any environment. If the migration is already in the database, revert it first with a new migration.

### List applied and pending migrations

```bash
cd backend
dotnet ef migrations list --project MenuDB --startup-project MenuApi
```

## How migrations are applied at runtime

`Menu.MigrationService` is a `BackgroundService` that applies all pending EF migrations on startup and then exits. `MenuApi` is configured with `WaitForCompletion` on `Menu.MigrationService`, so the API never starts before migrations complete.

**Do not run `dotnet ef database update` against a shared or production database.** Let `Menu.MigrationService` manage migration application at runtime.

## EF entity configuration

Entity mappings live in `backend/MenuDB/Data/`. Column constraints (max length, required, etc.) are configured inside `MenuDbContext.OnModelCreating`. After adding a migration, always inspect the generated migration file to confirm the SQL it will execute matches your intent before committing.

## Validation

After adding a migration, build the solution to confirm there are no EF model snapshot conflicts:

```bash
cd backend
dotnet restore MenuApi.sln
dotnet build MenuApi.sln --configuration Release --no-restore
```
