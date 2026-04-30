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
- Treat a package as part of `eslint` when its name is `eslint`, starts with `@eslint/` or `@typescript-eslint/`, starts with `eslint-`, or starts with `@vue/eslint-config-`.
- Treat a package as part of `quasar` when its name is `quasar`, starts with `@quasar/`, or is exactly `@quasar/app-vite`.
- Treat a package as part of `tanstack` when its name starts with `@tanstack/`.
- Treat a package as part of `vue` when its name is `vue`, `vue-router`, `vue-tsc`, or `@vitejs/plugin-vue`, starts with `@vue/`, or is exactly `eslint-plugin-vue`, `vite-plugin-vue-devtools`, or `@vue/test-utils`.
- **Simple version bump**: only `package.json` and `pnpm-lock.yaml` need version updates.
- **Tool-driven update**: a `storybook` update when the official Storybook upgrade workflow recommends running `pnpm dlx storybook@latest upgrade` or equivalent automated migrations.
- **Code-changing update**: lint failures, type errors, tests, or upgrade guidance require source changes beyond dependency and lock-file edits.
- `eslint`, `quasar`, `vue`, and `tanstack` are not automatically tool-driven. Treat them as simple version bumps unless the update evidence shows a dedicated upgrade workflow or source changes are required.

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

## Validation

Run these commands from the repository root, in this order:

```bash
cd backend
dotnet restore MenuApi.sln
dotnet build MenuApi.sln --configuration Release --no-restore
cd ../ui/menu-website
corepack enable pnpm
pnpm install --frozen-lockfile
pnpm run generate-openapi
pnpm run lint
pnpm run build
pnpm run test
```

- If any required validation command fails for a planned pull request, do not create or finalise that pull request. Record the failure, continue processing the remaining planned pull requests for this ecosystem, and exit with a non-zero status code after all planned pull requests have been attempted.