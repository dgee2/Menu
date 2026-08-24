---
name: frontend-typescript-build-deps
description: Use when `pnpm build`, `vue-tsc`, or `pnpm lint` in ui/menu-website fails with "Cannot find module '@/generated/open-api/menu-api'" in a fresh checkout or new worktree — a red herring that looks like a code regression but is actually missing generated build artifacts.
---

# Frontend TypeScript Build Dependencies

`Cannot find module '@/generated/open-api/menu-api'` is a **red herring**: it reads like a broken import or bad merge, but in a fresh checkout or worktree it almost always means two gitignored build artifacts were never generated.

## The dependency chain

| Artifact | Produced by |
|---|---|
| `open-api/menu-api.json` (repo root) | `dotnet build` on `MenuApi` (the `GenerateOpenApiDocuments` MSBuild target) |
| `ui/menu-website/src/generated/open-api/menu-api.ts` | `pnpm generate-openapi`, which reads `open-api/menu-api.json` |

Both are gitignored. `src/services/recipe-api.ts` and other service-layer files import from `@/generated/open-api/menu-api`; if it was never generated in this checkout, that import fails as a TypeScript module-resolution error rather than an obvious "file missing" message.

Worktrees share git history but not gitignored build outputs, so this hits every new worktree under `worktrees/` on first use.

## Steps

1. **Confirm it's the red herring.** Check whether `ui/menu-website/src/generated/open-api/menu-api.ts` exists. If it exists and the error still occurs, stop — this is a genuine regression (e.g. a renamed or removed backend endpoint the frontend wasn't updated for), not a missing-artifact issue. Completion: you know which case you're in.
2. **Build the backend first.**
   ```bash
   cd backend
   dotnet restore MenuApi.sln
   dotnet build MenuApi.sln --configuration Release --no-restore
   ```
   Completion: `open-api/menu-api.json` exists at the repo root.
3. **Regenerate the frontend types.**
   ```bash
   cd ui/menu-website
   pnpm generate-openapi
   ```
   Completion: `src/generated/open-api/menu-api.ts` exists and is non-empty.
4. **Re-run the command that failed** (`pnpm build`, `pnpm lint`, or `vue-tsc`). Completion: the module-resolution error is gone.

The order matters: step 3 reads the file step 2 produces. Running `pnpm generate-openapi` first regenerates against a stale or missing spec.

## Related

[openapi-sync](../openapi-sync/SKILL.md) — the same regeneration mechanics, for use after *intentionally* changing a backend endpoint rather than diagnosing a fresh checkout or worktree.
