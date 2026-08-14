---
name: pnpm-upgrade
description: Upgrade pnpm dependencies in ui/menu-website. Use when the user wants to update frontend packages or act on `pnpm outdated`, or when a dependency bump has broken lint, types, or tests.
---

# Upgrading pnpm packages

The work is making the verification mean something: in this repo both the type-checker and the Storybook suite will report success while measuring nothing.

## 1. Make the toolchain sighted

`src/generated/open-api/menu-api.ts` is gitignored, so a fresh checkout or worktree lacks it. Without it, `data` from `openapi-fetch` resolves to an error type and lint and type-check run **blind** — they pass while answering a different question than CI does. Rules like `@typescript-eslint/no-unnecessary-type-assertion` stay quiet locally and fire in CI, which lands the fix in whichever branch you were on when you finally saw it.

```bash
cd backend && dotnet restore MenuApi.sln && dotnet build MenuApi.sln --configuration Release --no-restore
cd ui/menu-website && pnpm generate-openapi
```

Completion: `src/generated/open-api/menu-api.ts` exists and is non-empty. Full diagnosis in [frontend-typescript-build-deps](../frontend-typescript-build-deps/SKILL.md).

## 2. Survey and group

```bash
cd ui/menu-website && pnpm outdated
```

Assign every outdated package to a unit: one bulk unit for everything staying within its major, then one per major bump — or one per set of majors that peer-depend on each other, like `quasar` + `@quasar/app-vite` + `@quasar/vite-plugin`.

For each major, check what consumes it before committing to the bump. A compiler is gated by its type-checker: `typescript` cannot reach 7 while `vue-tsc`, which wraps the TS compiler API and runs `type-check`, has no release supporting it.

Completion: every package from `pnpm outdated` sits in a named unit, and each major has had its consumers checked.

## 3. Bump and install

Edit the ranges in `package.json`, then:

```bash
pnpm install
```

Completion: install exits clean, and `pnpm why <pkg>` reports a single version of any framework package the bump touched.

When a bump breaks something you did not touch, read [TRAPS.md](TRAPS.md) before debugging — pnpm's resolution produces several failures that look like application bugs.

## 4. Verify, and make the green load-bearing

```bash
pnpm run lint
pnpm run build          # type-check + vite build
pnpm run test:unit
pnpm run test:storybook
```

Skip `pnpm run test:e2e`: it asserts a scaffold string (`You did it!`) and fails on `main`, so it carries no signal either way.

An assertion is **load-bearing** when it can only pass if the thing under test actually works. Storybook stories asserting `No recipes found.` passed identically whether MSW served an empty list or served nothing at all, because the component renders the same fallback either way — so the suite stayed green through a completely dead mock.

For any bump touching mocking, rendering, or the test environment, add an assertion that reaches for mocked data (`await canvas.findByText('Chocolate Cake')`) and watch it fail before the fix and pass after. Runtime is a second tell: a story that "passes" in 2.4s and drops to 190ms once the mock works was previously passing on a timeout.

Completion: every suite green, and for bumps in those areas, at least one load-bearing assertion observed failing first.

## 5. Commit

Record why a version was chosen whenever the choice reads as a mistake — a pin, an override, an apparent downgrade — so the next person keeps it instead of reverting it.

Completion: the commit ends with the `Co-authored-by:` trailer from [agent-identity](../agent-identity/SKILL.md), and each pin or override states its rationale.

## Sweeps that span several majors

When the upgrade is large enough to ship as more than one PR, read [MAJOR-SWEEP.md](MAJOR-SWEEP.md): stacking the units into reviewable PRs, resolving the lockfile conflict that appears on every rebase, and moving `engines.node` and `@types/node` together.
