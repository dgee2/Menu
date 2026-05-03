---
name: "unique-index-migration-guards"
description: "Add duplicate-data guard SQL before creating business-unique indexes in Menu migrations"
domain: "backend"
confidence: "high"
source: "earned"
---

## Context
Use this pattern when Menu adds a new unique index for a business key and existing shared environments may already contain duplicate rows that would make the migration fail at deploy time.

## Patterns
- Add the unique index in `MenuDbContext` so the EF model, snapshot, and generated migration stay aligned.
- Keep the migration focused on the unique-index change, but prepend explicit SQL duplicate checks for each constrained table before `CreateIndex(... unique: true)`.
- Make the guard SQL throw a specific remediation message naming the affected business key so deployment failures explain exactly what data must be cleaned up.
- Document the pre-deployment duplicate audit queries and cleanup steps in existing backend docs when the migration affects shared data.

## Examples
```csharp
migrationBuilder.Sql(
    """
    IF EXISTS (
        SELECT [Name]
        FROM [Recipe]
        GROUP BY [Name]
        HAVING COUNT(*) > 1
    )
    BEGIN
        THROW 50000, 'Cannot apply AddBusinessUniqueIndexes while duplicate Recipe.Name rows exist. Remediate duplicate recipe names before retrying the migration.', 1;
    END;
    """);
```

## Anti-Patterns
- Adding a unique index without checking whether existing shared data can satisfy it.
- Relying on raw SQL Server error 1505 output as the only remediation signal for deployments.
- Documenting cleanup steps only in PR text instead of in durable repo docs tied to the migration.
