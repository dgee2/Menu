---
name: openapi-sync
description: Regenerate Orval clients, Zod Mini validators, and MSW handlers after MenuApi endpoint or contract changes.
---

# OpenAPI Sync

## Purpose

Use this skill after changes to `MenuApi` routes, inputs, or responses. The backend OpenAPI document is the source for client generation and runtime validation.

## How it works

1. The .NET build writes `open-api/menu-api.json` at the repository root.
2. `pnpm generate-openapi` (from `ui/menu-website/`) generates request functions, models, Zod Mini validators, and MSW handlers in `src/generated/open-api/`.
3. `src/services/recipe-api.ts` calls the generated operations through an authenticated factory and a validating request function.

Update the backend contract or generator configuration to change generated output.

## Steps

### 1. Build the backend to produce the spec

```bash
cd backend
dotnet restore MenuApi.sln
dotnet build MenuApi.sln --configuration Release --no-restore
```

This writes `open-api/menu-api.json`.

### 2. Regenerate the frontend API artifacts

```bash
cd ui/menu-website
pnpm generate-openapi
```

The OpenAPI document and Orval output are gitignored. CI passes the document from the backend build to the frontend jobs, which run the same generation command with the frozen lockfile.

## Validation

After regeneration, verify operation IDs and the documented inputs, response status codes, media types, and bodyless outcomes against endpoint behavior. Then run the frontend checks:

```bash
cd ui/menu-website
pnpm run lint
pnpm run build
pnpm run test:unit
pnpm run test:storybook
```

For changes to the authenticated recipe flow, run the real Playwright suite using the [E2E skill](../e2e-tests/SKILL.md).
