---
name: dependency-update-github-actions
description: Discover, classify, group, and validate GitHub Actions dependency updates for the Menu repository by using workflow references and GitHub release or tag data.
user-invokable: false
context: fork
---

# GitHub Actions Dependency Updates

## Purpose

Use this skill when the selected ecosystem is `GitHub Actions`.

## Runtime parameters

Assume the current workspace is the target repository and the current working directory is the repository root.

When available, the caller should provide this runtime context in plain text:

- `Ecosystem`
- `Branch prefix` override
- `Default branch` override
- `GitHub Actions discovery scope`
- `Prerelease policy`

## Repository facts

- Work from `.github/workflows/`.
- Repository-wide rule: create temporary git worktrees only under `worktrees/` at the repository root.
- When the caller does not provide a branch-prefix override, use `copilot/dependency-update/github-actions`.
- Branches must use the selected branch prefix with one of these suffixes:
  - `/simple`
  - `/code-change-<owner-repo>`

## Discovery

Use workflow references plus GitHub release or tag data as the source of truth.

- Parse `.github/workflows/*.yml`.
- Collect step-level `uses:` action references and, when the discovery scope includes reusable workflows, `jobs.<job>.uses` references.
- Ignore local references that begin with `./`.
- Ignore Docker image references that begin with `docker://`.
- For each `owner/repo@ref` reference, determine whether the current ref is a tag, a major alias such as `v4`, or a pinned SHA.
- Query the upstream repository with `gh api` or the GitHub MCP tools to find the newest available release or tag for that action or reusable workflow.
- Prefer stable releases by default. If the current ref is prerelease, allow prerelease candidates for that specific reference.
- For SHA-pinned refs that include an adjacent version comment such as `# v3.0.2`, use that version comment as the current version when comparing against available tags, and preserve SHA pinning when updating the workflow.

## Grouping and classification

- This repository has no named tool-driven GitHub Actions groups.
- **Simple version bump**: only workflow refs and adjacent version comments change.
- **Code-changing update**: workflow syntax, required inputs, or job structure must change beyond ref updates.

## Pull request boundaries

- Create one pull request for all simple version bumps using the selected branch prefix plus `/simple`.
- Create one pull request per code-changing dependency using the selected branch prefix plus `/code-change-<owner-repo>`.
- Reuse an existing open pull request for the same branch when one already exists.

## Parallel execution

- Once pull request boundaries are decided, independent planned pull requests may be updated and validated in parallel by separate subagents.
- Create one git worktree per planned pull request branch under `worktrees/` and keep a strict one-to-one mapping between the branch and the worktree.
- Use each subagent's worktree to apply workflow changes and run validation for only that planned pull request.
- After each pull request is created or updated, remove the corresponding temporary worktree and run `git worktree prune`.

## Validation

Run one of the following YAML validation commands from the repository root, depending on which toolchain is available:

```bash
pwsh -NoLogo -NoProfile -Command "Get-ChildItem '.github/workflows/*.yml' | ForEach-Object { Get-Content $_.FullName -Raw | ConvertFrom-Yaml | Out-Null }"
```

```bash
python -c "from pathlib import Path; import yaml; [yaml.safe_load(path.read_text(encoding='utf-8')) for path in Path('.github/workflows').glob('*.yml')]"
```

```bash
python3 -c "from pathlib import Path; import yaml; [yaml.safe_load(path.read_text(encoding='utf-8')) for path in Path('.github/workflows').glob('*.yml')]"
```

Then run these repository validation commands from the repository root, in this order:

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