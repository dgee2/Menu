---
name: dependency-update-dotnet
description: Discover, classify, group, and validate .NET / NuGet dependency updates for the Menu repository by using the dotnet CLI.
user-invokable: false
context: fork
---

# .NET / NuGet Dependency Updates

## Purpose

Use this skill when the selected ecosystem is `.NET / NuGet`.

## Runtime parameters

Assume the current workspace is the target repository and the current working directory is the repository root.

When available, the caller should provide this runtime context in plain text:

- `Ecosystem`
- `Branch prefix` override
- `Default branch` override
- `Prerelease policy`

## Repository facts

- Work from `backend/`.
- The solution to inspect is `MenuApi.sln`.
- Repository-wide rule: create temporary git worktrees only under `worktrees/` at the repository root.
- When the caller does not provide a branch-prefix override, use `copilot/dependency-update/dotnet`.
- Branches must use the selected branch prefix with one of these suffixes:
  - `/simple`
  - `/tool-aspire`
  - `/code-change-<package-name>`

## Discovery

Use the .NET CLI as the source of truth for direct NuGet updates.

Run from the repository root:

```bash
cd backend
dotnet restore MenuApi.sln
dotnet package list MenuApi.sln --outdated --format json
```

If the installed SDK does not support `dotnet package list`, fall back to:

```bash
cd backend
dotnet restore MenuApi.sln
dotnet list MenuApi.sln package --outdated --format json
```

- Use the JSON output to enumerate direct package references that can be updated in this repository.
- Ignore transitive-only updates unless a direct package update is blocked or a validation failure needs explaining.
- Prefer stable target versions by default. If the currently referenced package version is prerelease, allow prerelease candidates for that package and record that decision.

## Grouping and classification

- The only named tool-driven group in this repository is `aspire`.
- Treat a package as part of `aspire` when its package ID starts with `Aspire.`, contains `.Aspire.`, or is exactly `CommunityToolkit.Aspire.Hosting.JavaScript.Extensions`.
- **Simple version bump**: only project-file version edits are required.
- **Tool-driven update**: an `aspire` update when release notes or local evidence show related hosting, workload, or generated changes should be applied as part of the update.
- **Code-changing update**: build output, tests, or upgrade guidance require application or test source changes beyond version edits.

## Label management

### Labels to apply

Apply both of these labels to every pull request created or updated by this skill:

- `dependency-update`
- `dotnet`

If either label does not exist in the repository, create it before applying:

```bash
gh label create "dependency-update" --color "0075ca" --description "Automated dependency update PR"
gh label create "dotnet" --color "512BD4" --description ".NET / NuGet ecosystem dependency update"
```

When creating a pull request use `--label dependency-update --label dotnet`. For an existing pull request use `gh pr edit <number> --add-label dependency-update --add-label dotnet`.

### Checking for existing PRs

Before creating any pull requests, retrieve all currently open pull requests that carry **both** `dependency-update` and `dotnet` labels:

```bash
gh pr list --label dependency-update --label dotnet --state open --json number,headRefName,title
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
- Create one pull request for the `aspire` tool-driven group using the selected branch prefix plus `/tool-aspire`.
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

**Worktree cleanup:** `git worktree remove --force` may still fail on Windows when a directory is large. If that happens, delete the directory first, then prune:

```powershell
Remove-Item <worktree-path> -Recurse -Force
git worktree prune
```

## Validation

Run these commands from the repository root, in this order:

```bash
cd backend
dotnet restore MenuApi.sln
dotnet build MenuApi.sln --configuration Release --no-restore
dotnet test --project MenuApi.Tests/MenuApi.Tests.csproj --configuration Release --no-build
cd ../ui/menu-website
corepack enable pnpm
pnpm install --frozen-lockfile
pnpm run generate-openapi
pnpm run lint
pnpm run build
pnpm run test
```

- If any required validation command fails for a planned pull request, do not create or finalise that pull request. Record the failure, continue processing the remaining planned pull requests for this ecosystem, and exit with a non-zero status code after all planned pull requests have been attempted.

**Running tests on Windows:** `pnpm run test` launches Playwright/Chromium browser tests. On Windows agents the process will appear to block the shell. Use PowerShell's `Start-Job` pattern — see the Windows / PowerShell Notes section in AGENTS.md for the canonical pattern.