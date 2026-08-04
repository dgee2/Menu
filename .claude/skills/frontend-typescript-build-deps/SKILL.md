---
name: frontend-typescript-build-deps
description: Diagnose "Cannot find module '@/generated/open-api/menu-api'" and similar frontend TypeScript build/lint failures in a fresh checkout or worktree — the frontend build depends on generated artifacts that are not checked into git and must be regenerated in the right order.
---

# Frontend TypeScript Build Dependencies

## Purpose

In a fresh checkout or new git worktree, `pnpm build`, `vue-tsc`, or `pnpm lint` in `ui/menu-website` can fail with:

```
Cannot find module '@/generated/open-api/menu-api' or its corresponding type declarations.
```

This **looks like a real regression** (missing export, broken import, bad merge) but usually isn't. It means the generated OpenAPI artifacts simply don't exist yet in this checkout. Use this skill to recognize and resolve that quickly instead of debugging the wrong thing.

## Why this happens

Two artifacts that the frontend TypeScript build depends on are **not checked into git**, and only exist after running specific commands:

| Artifact | Produced by | Notes |
|---|---|---|
| `open-api/menu-api.json` (repo root) | `dotnet build` on `MenuApi` (the `GenerateOpenApiDocuments` MSBuild target) | Gitignored (`open-api/.gitignore` ignores `*.json`) |
| `ui/menu-website/src/generated/open-api/menu-api.ts` | `pnpm generate-openapi` (from `ui/menu-website/`), which reads `open-api/menu-api.json` | Gitignored; entire `src/generated/` folder is regenerated, never hand-edited |

`src/services/recipe-api.ts` and other service-layer files import types from `@/generated/open-api/menu-api`. If that file was never generated in this checkout/worktree, the import fails — which surfaces as a TypeScript module-resolution error in `vue-tsc`, `pnpm build`, or `pnpm lint`, not as an obviously-missing-file error.

This bites most often in:
- A fresh `git clone`.
- A new git worktree created under `worktrees/` (per repo convention) — worktrees share history but not gitignored build outputs.
- CI-like scenarios where only `ui/menu-website` was checked out or `pnpm install` was run without a prior backend build.

## Fix: two-step regeneration, in order

The backend must be built **before** the frontend generation step — the frontend step reads the JSON the backend produces.

### 1. Build the backend first

```bash
cd backend
dotnet restore MenuApi.sln
dotnet build MenuApi.sln --configuration Release --no-restore
```

This writes `open-api/menu-api.json` at the repo root.

### 2. Then regenerate the frontend types

```bash
cd ui/menu-website
pnpm generate-openapi
```

This reads `../../open-api/menu-api.json` and writes `src/generated/open-api/menu-api.ts`.

### 3. Re-run the frontend build/lint

```bash
cd ui/menu-website
pnpm build
pnpm lint
```

The module-resolution error should be gone.

## Diagnosis checklist

Before assuming a real code regression when frontend TypeScript checks fail:

1. Does `ui/menu-website/src/generated/open-api/menu-api.ts` exist at all? If not, this is the missing-artifact issue, not a code bug.
2. Does `open-api/menu-api.json` exist at the repo root? If not, the backend hasn't been built yet in this checkout/worktree — start at step 1 above.
3. Only after both artifacts exist and the error persists should you treat it as a genuine regression (e.g. an endpoint was renamed/removed and frontend code wasn't updated to match).

## Related

See the [openapi-sync](../openapi-sync/SKILL.md) skill for the full regeneration workflow to use **after intentionally changing a backend endpoint** (this skill is about recognizing and resolving the fresh-checkout/worktree case specifically).
