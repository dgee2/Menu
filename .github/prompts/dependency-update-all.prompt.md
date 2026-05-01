---
name: dependency-update-all
description: Create or update dependency update pull requests across all supported ecosystems in the current repository.
argument-hint: optional default branch override
---

# Dependency update mission for all ecosystems

You are responsible for processing all pending dependency updates in the current repository across `.NET / NuGet`, `Node / npm / pnpm`, and `GitHub Actions`.

## Runtime context

Use the current workspace as the target repository and assume the current working directory is the repository root.

The caller should provide or confirm this runtime context in plain text before you act:

- Default branch, when it should override the repository default
- Interaction mode, when the run is unattended
- GitHub Actions discovery scope
- Prerelease policy

## Clarifying questions

- If any required context is missing or ambiguous and the run is interactive, ask concise clarifying questions before changing files, opening branches, or creating pull requests.
- If the run is unattended, derive the missing answers from the provided runtime parameters and the repository state, record the assumptions in your notes or pull request body, and continue without waiting.

## Execution order

Process ecosystems in this order:

1. `.NET / NuGet`
2. `Node / npm / pnpm`
3. `GitHub Actions`

For each ecosystem:

- Follow the same workflow contract as `.github/prompts/dependency-update.prompt.md`.
- Apply `dependency-update-pr-description` and exactly one ecosystem skill:
  - `dependency-update-dotnet`
  - `dependency-update-node`
  - `dependency-update-github-actions`
- Use the ecosystem's default branch prefix convention unless the caller explicitly overrides it:
  - `.NET / NuGet` -> `copilot/dependency-update/dotnet`
  - `Node / npm / pnpm` -> `copilot/dependency-update/node`
  - `GitHub Actions` -> `copilot/dependency-update/github-actions`

## Operating rules

- Discover pending updates with each ecosystem's own toolchain and repository manifests.
- Do not rely on repository bot metadata files as update inputs.
- Follow the repository-wide rule that temporary git worktrees must be created only under `worktrees/` at the repository root.
- Discovery, classification, and pull request boundary decisions should be completed before parallel execution begins.
- Once the planned pull request groups are decided, independent groups may be updated and validated in parallel by separate subagents, each using its own git worktree under `worktrees/`.
- Keep a strict one-to-one mapping between a planned pull request branch and its worktree.
- After each pull request is created or updated, remove the corresponding temporary worktree and run `git worktree prune`. If `git worktree remove` fails due to directory size (e.g. `node_modules` present), delete the directory first (`Remove-Item <path> -Recurse -Force` on Windows, `rm -rf <path>` on Unix) then run `git worktree prune`.
- When pushing from a detached HEAD worktree, branch names containing `/` require the full refspec — always use `git push origin HEAD:refs/heads/<branch-name>`.
- Every pending update must end up in exactly one pull request for its ecosystem.
- Reuse an existing open pull request for the same branch when one already exists.
- If any required validation command fails for an ecosystem, do not create or finalise the affected pull request for that ecosystem. Record the failure, continue processing the remaining ecosystems, and exit with a non-zero status code after all ecosystems have been attempted.

## Label management

Each pull request must be labelled with `dependency-update` and the ecosystem-specific label. Before processing each ecosystem, check for existing open PRs with those labels to reuse them or to identify stale ones for closure after the run.

| Ecosystem | Label |
|---|---|
| `.NET / NuGet` | `dotnet` |
| `Node / npm / pnpm` | `node` |
| `GitHub Actions` | `github-actions` |

Detailed label management rules (label creation, PR discovery, stale PR closure) are specified in each ecosystem skill.