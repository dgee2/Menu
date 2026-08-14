# Resolution traps

pnpm resolves the newest version satisfying every range in the graph. That single rule produces most of the failures below, and they surface far from the package you bumped, which is what makes them read as application bugs.

Start with `pnpm why <pkg>`: it names what pulls a version in, and reveals when more than one copy is installed.

## A direct dependency breaks after bumping something unrelated

A transitive peer can **float** a package past what a direct consumer supports. Bumping `vitest` pulled `msw` to 2.15 through `@vitest/mocker`, and `msw-storybook-addon@2` relies on `worker.context`, which 2.15 removed — so every Storybook test died on `Cannot set properties of undefined (setting 'activationPromise')`.

Pin the floated package in `overrides` in `pnpm-workspace.yaml`, with a comment naming the condition that retires the pin. Here that condition was the addon reaching v3, at which point the pin came out and both moved together.

## Type errors on a plugin you did not touch

A package that declares a framework as a regular dependency rather than a peer gets its own copy of it. `@auth0/auth0-vue` does this with `vue`, so bumping `vue` left two copies installed, producing two structurally incompatible `App`/`Plugin` types and breaking `app.use(createAuth0())` under `vue-tsc` with `provides no match for the signature`.

Force a single version through `overrides`. Confirm with `pnpm why vue` reporting `Found 1 version`.

## ERR_PNPM_UNUSED_PATCH

The patched package moved and the patch no longer applies. Check whether the bug was fixed upstream before re-rolling it — `@storybook/addon-vitest`'s Windows absolute-path bug was fixed a different way in 10.5.7, so the patch and its `patchedDependencies` entry were deleted rather than rebuilt.

## Lint fails on code you did not change

Linter and plugin bumps make existing suppressions redundant: unused `eslint-disable` directives, and assertions that `@typescript-eslint/no-unnecessary-type-assertion` now recognises as no-ops. Clear them in the PR that bumps the linter, since that bump is what made them stale.

Verify this one with the generated OpenAPI types present, or the result is **blind** — see step 1 of [SKILL.md](SKILL.md).

## mockServiceWorker.js shows as modified with an empty diff

msw's postinstall rewrites the file with different line endings. `git restore` it. After a deliberate msw bump, regenerate it properly:

```bash
pnpm exec msw init public --save
```
