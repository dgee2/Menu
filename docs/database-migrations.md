# Database Migrations

This project uses **Entity Framework Core** with SQL Server to manage the database schema for `MenuDbContext`. Migrations are stored under `backend/MenuDB/Migrations/` and are applied automatically at startup when running inside .NET Aspire.

---

## Prerequisites

Install the EF Core global tool if you don't already have it:

```bash
dotnet tool install --global dotnet-ef
```

Or update it to the latest version:

```bash
dotnet tool update --global dotnet-ef
```

Verify installation:

```bash
dotnet ef --version
```

> The `Microsoft.EntityFrameworkCore.Design` package is referenced in `MenuDB.csproj` and is required for the tooling to work.

---

## Creating a New Migration

All `dotnet ef` commands must be run from the **backend solution root** (`C:\git\Menu\backend\`) and target the `MenuDB` project, which contains `MenuDbContext` and `MenuDbContextFactory`. `MenuApi` is used as the startup project to provide the runtime host.

After modifying any entity in `backend/MenuDB/Data/` or the model configuration in `backend/MenuDB/MenuDbContext.cs`, create a new migration:

```bash
dotnet ef migrations add <MigrationName> --project MenuDB --startup-project MenuApi
```

Replace `<MigrationName>` with a short, descriptive name in `PascalCase` that describes what changed, for example:

```bash
dotnet ef migrations add AddRecipeDescription --project MenuDB --startup-project MenuApi
```

This generates three files under `MenuDB/Migrations/`:

| File | Purpose |
|---|---|
| `<Timestamp>_<MigrationName>.cs` | The `Up` and `Down` migration logic |
| `<Timestamp>_<MigrationName>.Designer.cs` | EF Core snapshot metadata (do not edit) |
| `MenuDbContextModelSnapshot.cs` | Updated cumulative model snapshot (do not edit) |

---

## Reviewing Migrations Before Applying

Always review the generated migration file before applying it:

- Confirm the `Up()` method reflects only the intended schema changes.
- Confirm the `Down()` method correctly reverts those changes.
- Check that seed data changes (`HasData`) are included if you modified them in `OnModelCreating`.
- Verify no unintended table or column drops are present.

To preview the SQL that would be executed without touching the database:

```bash
dotnet ef migrations script --idempotent --project MenuDB --startup-project MenuApi
```

The `--idempotent` flag generates `IF NOT EXISTS` guards so the script is safe to run against any database state.

---

## Applying Migrations

### Via the Migration Service (Aspire / Development)

Migrations are applied by the dedicated **`Menu.MigrationService`** worker, not by the API. When you run `Menu.AppHost`, Aspire starts the services in this order:

1. SQL Server container starts
2. `Menu.MigrationService` runs — applies all pending migrations, then exits
3. `MenuApi` starts — guaranteed to find the schema already up to date

This is enforced in `backend/Menu.AppHost/Program.cs`:

```csharp
var menuDb = sql.AddDatabase("menu");

var migrations = builder.AddProject<Projects.Menu_MigrationService>("migrations")
    .WithReference(menuDb)
    .WaitFor(menuDb);

var menuApi = builder.AddProject<Projects.MenuApi>("apiservice")
    .WithReference(menuDb)
    .WaitForCompletion(migrations); // API will not start until migrations finish
```

The migration service uses an **execution strategy** to handle transient SQL Server errors, and reports progress via OpenTelemetry traces visible in the Aspire dashboard.

### Manually via CLI

To apply pending migrations against a specific database:

```bash
dotnet ef database update --project MenuDB --startup-project MenuApi
```

To apply up to a specific migration (useful for rolling back to a known state):

```bash
dotnet ef database update <MigrationName> --project MenuDB --startup-project MenuApi
```

To revert all migrations (drops all tables managed by EF Core):

```bash
dotnet ef database update 0 --project MenuDB --startup-project MenuApi
```

---

## Design-Time Connection String

`MenuDbContextFactory` provides a design-time connection string used exclusively by the EF Core tooling:

```csharp
.UseSqlServer("Server=localhost;Database=menu;Trusted_Connection=True;")
```

This allows `dotnet ef` commands to run without starting the application. If your local SQL Server instance uses a different name or authentication method, update `MenuDbContextFactory.cs` locally (do not commit personal connection strings).

---

## Removing the Last Migration

If you created a migration but have not yet applied it to any database, you can remove it cleanly:

```bash
dotnet ef migrations remove --project MenuDB --startup-project MenuApi
```

> This only works if the migration has **not** been applied. If it has been applied, revert the database first with `dotnet ef database update <PreviousMigrationName>`.

---

## Best Practices

### Naming
- Use `PascalCase` descriptive names: `AddRecipeDescription`, `RenameIngredientColumn`, `AddIndexOnRecipeName`.
- Avoid generic names like `Update` or `Changes`.

### One concern per migration
- Keep each migration focused on a single logical change (e.g., adding a table, adding a column, adding an index). This makes rollbacks precise and history readable.

### Never edit applied migrations
- Once a migration has been applied to any shared environment (staging, production), treat it as immutable. Create a new migration to correct it instead.

### Seed data
- Seed data is managed via `HasData()` in `OnModelCreating`. Changes to seed data are detected by EF Core and included automatically in the next migration.
- Keep seed data deterministic: always use fixed IDs (as the existing `UnitType` and `Unit` seeds do) so migrations are idempotent.

### Additive changes preferred
- Prefer adding nullable columns or columns with defaults over dropping or renaming existing columns, especially in a running production system.
- When a rename is needed, do it in two migrations: add the new column, migrate data, drop the old column.

### Review generated SQL
- Always run `dotnet ef migrations script --idempotent` and review the output before deploying to staging or production.

### Source control
- Commit migration files (`.cs` and `MenuDbContextModelSnapshot.cs`) together with the model change that caused them in the same commit.
- Never commit a migration without the corresponding model change, and vice versa.

### Production deployments
- For production, prefer running `dotnet ef migrations script` to generate a `.sql` file and apply it via your deployment pipeline rather than relying on `MigrateAsync()` at runtime.
- Use `--idempotent` so the script is safe to re-run.

---

## Useful Commands Summary

| Task | Command |
|---|---|
| Add a migration | `dotnet ef migrations add <Name> --project MenuDB --startup-project MenuApi` |
| Remove last migration | `dotnet ef migrations remove --project MenuDB --startup-project MenuApi` |
| Apply all pending migrations | `dotnet ef database update --project MenuDB --startup-project MenuApi` |
| Revert to a specific migration | `dotnet ef database update <Name> --project MenuDB --startup-project MenuApi` |
| Revert all migrations | `dotnet ef database update 0 --project MenuDB --startup-project MenuApi` |
| Generate idempotent SQL script | `dotnet ef migrations script --idempotent --project MenuDB --startup-project MenuApi` |
| List all migrations and status | `dotnet ef migrations list --project MenuDB --startup-project MenuApi` |
