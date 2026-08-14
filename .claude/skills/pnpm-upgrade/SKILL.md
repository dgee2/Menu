---
name: pnpm-upgrade
description: Use when upgrading the pnpm dependencies in ui/menu-website — surveying what is outdated, splitting major bumps into a reviewable stack of PRs, and the verification order and dependency traps that decide whether the lint/test results can be trusted.
---

# Upgrading pnpm packages

Use this skill for any dependency bump in `ui/menu-website`, from a single package to a full sweep. The mechanics (`pnpm up`) are trivial; everything below is about the parts that silently produce wrong answers.

## Generate the OpenAPI types before you lint or build

**Do this first, every time, in every worktree.** `src/generated/open-api/menu-api.ts` is gitignored, so a fresh worktree does not have it.

```bash
cd backend && dotnet restore MenuApi.sln && dotnet build MenuApi.sln --configuration Release --no-restore
cd ui/menu-website && pnpm generate-openapi
```

Without it, `data` from `openapi-fetch` resolves to an error type. Lint and type-check still *run* — they just answer a different question than CI does. In practice this means rules like `@typescript-eslint/no-unnecessary-type-assertion` stay quiet locally and fire in CI, so a lint error gets "fixed" in whichever PR you happened to be on when you finally saw it, rather than the one that introduced it.

If a lint or build result surprises you, confirm this file exists before believing the result. See [frontend-typescript-build-deps](../frontend-typescript-build-deps/SKILL.md).

## Survey and group

```bash
cd ui/menu-website && pnpm outdated
```

Split the work into one PR per reviewable unit, ordered safest first:

1. **One bulk PR** for everything staying within its current major.
2. **One PR per major bump**, or per group of majors that must move together because they peer-depend on each other (e.g. `quasar` + `@quasar/app-vite` + `@quasar/vite-plugin` all require each other's new majors).

A major that needs source changes belongs in its own PR with those changes, not folded into the bulk one. Ship the stack with [gh-stack](../gh-stack/SKILL.md); each layer stays independently reviewable and revertible.

Before bumping a major, check what consumes it — a compiler is gated by its type-checker, not just itself. `typescript` cannot move to 7 while `vue-tsc` (which wraps the TS compiler API and runs this repo's `type-check`) has no release supporting it.

## Verify every branch

Run the full set on each branch, not just the top of the stack. Later layers rebase onto earlier ones, so a broken lower layer breaks everything above it.

```bash
pnpm install
pnpm run lint
pnpm run build          # runs type-check + vite build
pnpm run test:unit
pnpm run test:storybook
```

`pnpm run test:e2e` is a stale scaffold asserting `You did it!` and fails on `main`; it is not a signal. For UI-affecting bumps (Quasar, Storybook, MSW) also smoke-check the dev server in a browser and confirm the console is clean.

## Dependency traps seen in this repo

| Symptom | Cause | Fix |
|---|---|---|
| A direct dep breaks after bumping something unrelated | pnpm resolves the newest version satisfying every range in the graph, so a transitive peer can float a package past what a direct consumer supports. Bumping `vitest` pulled `msw` to 2.15 via `@vitest/mocker`, breaking `msw-storybook-addon@2`'s use of `worker.context`. | Pin it in `overrides` in `pnpm-workspace.yaml`, with a comment saying when the pin can go |
| Type errors on a plugin you did not touch | A package declaring a framework as a regular (non-peer) dependency gets its own copy. `@auth0/auth0-vue` does this with `vue`, producing two structurally incompatible `App`/`Plugin` types and breaking `app.use(createAuth0())` | `overrides` entry forcing a single version; confirm with `pnpm why vue` reporting one version |
| `ERR_PNPM_UNUSED_PATCH` | The patched package upgraded and the patch no longer applies | Check whether the bug is fixed upstream. If so, delete the patch and its `patchedDependencies` entry rather than re-rolling it |
| Lint fails on code you did not change | eslint/plugin bumps make existing suppressions and assertions redundant (unused `eslint-disable`, unnecessary type assertions) | Remove them in the PR that bumps the linter, since that PR is what made them stale |
| `mockServiceWorker.js` shows as modified with an empty diff | msw's postinstall rewrites it with different line endings | `git restore` it. After a real msw bump, regenerate deliberately: `pnpm exec msw init public --save` |

Use `pnpm why <pkg>` to see what is pulling a version and whether more than one copy is installed.

## Tests that pass for the wrong reason

A green suite is not evidence a mock is working. Storybook stories asserting `No recipes found.` passed both when MSW served an empty list *and* when MSW served nothing at all, because the component renders the same fallback either way.

When a bump touches mocking, probe before trusting it: add an assertion that can only pass if the mock is actually serving data (`await canvas.findByText('Chocolate Cake')`), and watch it fail before your fix and pass after. Runtimes are a hint too — a story that "passes" in 2.4s and drops to 190ms once the mock works was previously passing on a timeout.

## Node targeting in package.json

Two fields describe which Node the project targets; move them together.

`engines.node` should be the intersection of what the upgraded packages declare. Derive it rather than guessing — run this from `ui/menu-website` with the final versions installed:

```bash
node -e "
const fs=require('fs'),path=require('path');
const pkg=JSON.parse(fs.readFileSync('package.json','utf8'));
for(const n of [...Object.keys(pkg.dependencies||{}),...Object.keys(pkg.devDependencies||{})].sort()){
  try{const j=JSON.parse(fs.readFileSync(path.join('node_modules',...n.split('/'),'package.json'),'utf8'));
  if(j.engines&&j.engines.node)console.log(n.padEnd(32),j.engines.node);}catch(e){}
}"
```

Watch for packages restricting themselves to even-numbered (LTS) majors — `@quasar/app-vite@3` does, which excludes odd majors like 25 and 27 from a strict intersection.

`@types/node` tracks Node release lines 1:1, so its major should match a line `engines.node` actually supports. Moving it from 25.x to 24.x reads as a downgrade but is the correct direction when 24 is the LTS line being targeted — 25.x types the non-LTS "Current" line.

CI pins `node-version: 22.x` in `.github/workflows/main.yml`; check the resolved version in a job log before assuming a raised floor is safe. pnpm does not enforce `engines` (`engine-strict` is unset), so a mismatch warns rather than fails.

## Rebasing the stack

Every branch touches `pnpm-lock.yaml`, so expect a conflict per layer. Do not hand-merge the lockfile — take the incoming branch's copy and let pnpm reconcile it against the resolved `package.json`:

```bash
git checkout --theirs ui/menu-website/pnpm-lock.yaml
(cd ui/menu-website && pnpm install)
git add ui/menu-website/pnpm-lock.yaml
```

Resolve `package.json` by hand: keep the layer's own bumps *and* whatever earlier layers introduced on adjacent lines.

A clean rebase is not proof of a correct one. Afterwards verify both:

```bash
# the change survived on every branch, not just the top
for b in <branches>; do echo -n "$b "; git show "$b:ui/menu-website/package.json" | grep '"@types/node"'; done

# the final state is unchanged by the restructure
git diff <old-top-sha> <new-top-branch>
```

An empty second diff means only history moved. A non-empty one means the rebase changed the result.

## Committing

Follow [agent-identity](../agent-identity/SKILL.md) — every commit ends with a `Co-authored-by:` trailer. State *why* a version was chosen when it is not obvious (a pin, an override, an apparent downgrade), since the next person will otherwise read it as a mistake and revert it.

## Related

- [frontend-typescript-build-deps](../frontend-typescript-build-deps/SKILL.md) — the missing-generated-types red herring
- [openapi-sync](../openapi-sync/SKILL.md) — regenerating API types after a backend change
- [gh-stack](../gh-stack/SKILL.md) — creating and rebasing stacked PRs
