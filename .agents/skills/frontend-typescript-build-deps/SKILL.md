---
name: frontend-typescript-build-deps
description: Diagnose missing generated OpenAPI imports in a fresh checkout or worktree when pnpm build, vue-tsc, or pnpm lint fails.
---

# Frontend TypeScript Build Dependencies

A module-resolution error under `@/generated/open-api/` in a fresh checkout or worktree can mean the gitignored Orval output has yet to be generated.

## The dependency chain

| Artifact | Produced by |
|---|---|
| `open-api/menu-api.json` (repo root) | `dotnet build` on `MenuApi` (the `GenerateOpenApiDocuments` MSBuild target) |
| `ui/menu-website/src/generated/open-api/` | `pnpm generate-openapi`, which reads `open-api/menu-api.json` |

Worktrees share git history but not gitignored build outputs, so this hits every new worktree under `worktrees/` on first use.

## Steps

1. **Check the output directory.** Confirm which imports are missing from `ui/menu-website/src/generated/open-api/`. If the files exist, investigate TypeScript resolution or operation naming.
2. **Build the backend first.**
   ```bash
   cd backend
   dotnet restore MenuApi.sln
   dotnet build MenuApi.sln --configuration Release --no-restore
   ```
   Completion: `open-api/menu-api.json` exists at the repo root.
3. **Regenerate the frontend API artifacts.**
   ```bash
   cd ui/menu-website
   pnpm generate-openapi
   ```
   Completion: clients, validators, and mock handlers exist under `src/generated/open-api/`.
4. **Re-run the command that failed** (`pnpm build`, `pnpm lint`, or `vue-tsc`). Completion: the module-resolution error is gone.

The order matters: step 3 reads the file step 2 produces. Running `pnpm generate-openapi` first regenerates against a stale or missing spec.

## Related

[openapi-sync](../openapi-sync/SKILL.md) — the same regeneration mechanics, for use after *intentionally* changing a backend endpoint rather than diagnosing a fresh checkout or worktree.
