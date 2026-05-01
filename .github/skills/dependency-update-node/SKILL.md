---
name: dependency-update-node
description: Discover, classify, group, and validate Node dependency updates for the Menu repository by using pnpm.
user-invokable: false
context: fork
---

# Node / npm / pnpm Dependency Updates

## Purpose

Use this skill when the selected ecosystem is `Node / npm / pnpm`.

## Runtime parameters

Assume the current workspace is the target repository and the current working directory is the repository root.

When available, the caller should provide this runtime context in plain text:

- `Ecosystem`
- `Branch prefix` override
- `Default branch` override
- `Prerelease policy`

## Repository facts

- Work from `ui/menu-website/`.
- The dependency manifest is `package.json` and the lock file is `pnpm-lock.yaml`.
- Repository-wide rule: create temporary git worktrees only under `worktrees/` at the repository root.
- When the caller does not provide a branch-prefix override, use `copilot/dependency-update/node`.
- Branches must use the selected branch prefix with one of these suffixes:
  - `/simple`
  - `/tool-storybook`
  - `/code-change-<package-name>`

## Discovery

Use `pnpm` as the source of truth for direct dependency updates.

Run from the repository root:

```bash
cd ui/menu-website
corepack enable pnpm
pnpm install --frozen-lockfile
pnpm outdated --format json
```

If the installed `pnpm` version does not support `--format json`, fall back to:

```bash
cd ui/menu-website
corepack enable pnpm
pnpm install --frozen-lockfile
pnpm outdated --json
```

- Use the output to enumerate direct dependencies and devDependencies from `package.json`.
- Prefer stable target versions by default.
- If the currently referenced package version is prerelease and the `pnpm outdated` output does not surface a prerelease target, query that package with `pnpm info <package-name> --json` before classifying it.

## Grouping and classification

- Named groups in this repository are `storybook`, `eslint`, `quasar`, `vue`, and `tanstack`.
- Assign each package to at most one named group. When more than one rule could match, use the first matching rule in this order: `storybook`, `eslint`, `quasar`, `tanstack`, `vue`.
- Treat a package as part of `storybook` when its name is `storybook`, starts with `@storybook/`, or is exactly `eslint-plugin-storybook` or `msw-storybook-addon`.
- **`msw` coupling rule:** `msw` and `msw-storybook-addon` share a strict peer dependency contract. When both have pending updates in the same run, move `msw` out of the `/simple` PR and into the same PR as `msw-storybook-addon`. Upgrading either package without the other breaks the Storybook test suite at runtime.
- Treat a package as part of `eslint` when its name is `eslint`, starts with `@eslint/` or `@typescript-eslint/`, starts with `eslint-`, or starts with `@vue/eslint-config-`.
- Treat a package as part of `quasar` when its name is `quasar`, starts with `@quasar/`, or is exactly `@quasar/app-vite`.
- Treat a package as part of `tanstack` when its name starts with `@tanstack/`.
- Treat a package as part of `vue` when its name is `vue`, `vue-router`, `vue-tsc`, or `@vitejs/plugin-vue`, starts with `@vue/`, or is exactly `eslint-plugin-vue`, `vite-plugin-vue-devtools`, or `@vue/test-utils`.
- **Simple version bump**: only `package.json` and `pnpm-lock.yaml` need version updates.
- **Tool-driven update**: a `storybook` update when the official Storybook upgrade workflow recommends running `pnpm dlx storybook@latest upgrade` or equivalent automated migrations.
- **Code-changing update**: lint failures, type errors, tests, or upgrade guidance require source changes beyond dependency and lock-file edits.
- `eslint`, `quasar`, `vue`, and `tanstack` are not automatically tool-driven. Treat them as simple version bumps unless the update evidence shows a dedicated upgrade workflow or source changes are required.

## Label management

### Labels to apply

Apply both of these labels to every pull request created or updated by this skill:

- `dependency-update`
- `node`

If either label does not exist in the repository, create it before applying:

```bash
gh label create "dependency-update" --color "0075ca" --description "Automated dependency update PR"
gh label create "node" --color "026e00" --description "Node / npm / pnpm ecosystem dependency update"
```

When creating a pull request use `--label dependency-update --label node`. For an existing pull request use `gh pr edit <number> --add-label dependency-update --add-label node`.

### Checking for existing PRs

Before creating any pull requests, retrieve all currently open pull requests that carry **both** `dependency-update` and `node` labels:

```bash
gh pr list --label dependency-update --label node --state open --json number,headRefName,title
```

Record this list as the _initial open set_. Use it to:

- Reuse an existing open PR when it targets the same planned branch (push changes to that branch; rebase onto the default branch if it is behind).
- Identify stale PRs (those not part of this run) after all planned PRs have been created or updated.

### Closing stale PRs

After all planned pull requests for this ecosystem have been created or updated, close every PR in the _initial open set_ that was **not** created or updated during this run:

```bash
gh pr close <number> --comment "Superseded by current dependency update run. This update is no longer pending or has already been merged."
```

## Pull request boundaries

- Create one pull request for all simple version bumps using the selected branch prefix plus `/simple`.
- Create one pull request for the `storybook` tool-driven group using the selected branch prefix plus `/tool-storybook` when a tool-driven update is required.
- Create one pull request per code-changing dependency using the selected branch prefix plus `/code-change-<package-name>`.
- Reuse an existing open pull request for the same branch when one already exists.

## Parallel execution

- Once pull request boundaries are decided, independent planned pull requests may be updated and validated in parallel by separate subagents.
- Create one git worktree per planned pull request branch under `worktrees/` and keep a strict one-to-one mapping between the branch and the worktree.
- Use each subagent's worktree to apply package changes and run validation for only that planned pull request.
- After each pull request is created or updated, remove the corresponding temporary worktree and run `git worktree prune`.

**Pushing from a worktree:** When pushing from a detached HEAD worktree, branch names containing `/` require the full refspec:

```bash
git push origin HEAD:refs/heads/<branch-name>
```

Using the short form (`HEAD:<branch-name>`) will fail with a refspec error.

**Worktree cleanup:** `git worktree remove` may fail when `node_modules` is present due to directory size. If this happens, delete the directory first, then prune:

```powershell
Remove-Item <worktree-path> -Recurse -Force
git worktree prune
```

## Validation

**Prerequisite — `open-api/menu-api.json`:** `pnpm run generate-openapi` reads `../../open-api/menu-api.json` relative to `ui/menu-website/`. This file is generated by `dotnet build`. When working in a worktree that has not run the dotnet build, copy it from a worktree that has:

```powershell
Copy-Item <dotnet-worktree>/open-api/menu-api.json <node-worktree>/open-api/menu-api.json
```

If no prior build output is available, run the dotnet build step first.

Run these commands from the repository root, in this order:

```bash
cd backend
dotnet restore MenuApi.sln
dotnet build MenuApi.sln --configuration Release --no-restore
cd ../ui/menu-website
corepack enable pnpm
pnpm install
pnpm run generate-openapi
pnpm run lint
pnpm run build
pnpm run test
```

**Lockfile workflow:** After editing `package.json`, run `pnpm install` (without `--frozen-lockfile`) to regenerate the lockfile. Then verify consistency with `pnpm install --frozen-lockfile` — this must exit 0 before committing.

**Running tests on Windows:** `pnpm run test` launches Playwright/Chromium browser tests for all 18 Storybook story files and blocks the shell until complete. On Windows agents, use PowerShell's `Start-Job` pattern to run with a timeout and capture partial output:

```powershell
$job = Start-Job { cd <worktree>/ui/menu-website; pnpm exec vitest run --passWithNoTests --reporter=verbose 2>&1 }
Start-Sleep 45
$output = Receive-Job $job
Stop-Job $job; Remove-Job $job -Force
$output | Select-String "FAIL|Error:|failed|passed|RUN"
```

A healthy run completes all 18 test files in ~20–30 seconds once the browser starts. If output shows `Test Files N passed` with no FAIL lines, tests pass. Note: filtering with `--project='!storybook'` reports zero files — all tests are browser/Storybook tests.

- If any required validation command fails for a planned pull request, do not create or finalise that pull request. Record the failure, continue processing the remaining planned pull requests for this ecosystem, and exit with a non-zero status code after all planned pull requests have been attempted.