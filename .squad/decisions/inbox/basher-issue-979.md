# Basher decision — issue #979

- Date: 2026-05-03T21:56:25.759+01:00
- Decision: Enforce uniqueness at the database level for `Ingredient.Name` and `Recipe.Name`, and gate the migration with duplicate-data checks that fail fast with remediation instructions.
- Scope: `backend/MenuDB\MenuDbContext.cs`, `backend/MenuDB\Migrations\20260503213945_AddBusinessUniqueIndexes.cs`, and `docs/specs/backend-write-duplicate-risk-audit.md`
- Rationale: These are the confirmed business-unique names in the duplicate-prevention epic. Failing the migration before index creation keeps deployment risk explicit and prepares #980 to translate remaining duplicate-name writes into deliberate API responses.
