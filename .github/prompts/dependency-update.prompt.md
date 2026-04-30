---
name: dependency-update
description: Create or update dependency update pull requests for a single ecosystem.
argument-hint: ecosystem, optional branch prefix, optional default branch
---

# Dependency update mission

You are responsible for all pending dependency updates for the selected ecosystem in the current repository.

## Runtime context

Use the current workspace as the target repository and assume the current working directory is the repository root.

The caller should provide or confirm this runtime context in plain text before you act:

- Ecosystem
- Branch prefix, when it should override the repository convention
- Default branch, when it should override the repository default
- Interaction mode, when the run is unattended
- GitHub Actions discovery scope, when the ecosystem is `GitHub Actions`
- Prerelease policy

## Clarifying questions

- If any required context is missing or ambiguous and the run is interactive, ask concise clarifying questions before changing files, opening branches, or creating pull requests.
- If the run is unattended, derive the missing answers from the provided runtime parameters and the repository state, record the assumptions in your notes or pull request body, and continue without waiting.

## Ecosystem skill selection

Apply `dependency-update-pr-description` for every pull request and exactly one ecosystem skill:

- `dependency-update-dotnet` for `.NET / NuGet`
- `dependency-update-node` for `Node / npm / pnpm`
- `dependency-update-github-actions` for `GitHub Actions`

## Operating rules

- Discover pending updates with the selected ecosystem's own toolchain and the repository manifests. Do not rely on repository bot metadata files as update inputs.
- Follow the repository-wide rule that temporary git worktrees must be created only under `worktrees/` at the repository root.
- Complete the work in this order: discover updates, classify updates, decide pull request boundaries, apply the update, validate each planned pull request, then create or update the pull request.
- Once pull request boundaries are decided, independent planned pull requests may be updated and validated in parallel by separate subagents, each using its own git worktree under `worktrees/`.
- Keep a strict one-to-one mapping between a planned pull request branch and its worktree.
- After each pull request is created or updated, remove the corresponding temporary worktree and run `git worktree prune`.
- Every pending update must end up in exactly one pull request.
- Reuse an existing open pull request for the same branch when one already exists.
- Target the repository default branch unless the caller provides an explicit default branch override.
- If any required validation command fails for a planned pull request, do not create or finalise that pull request. Record the failure, continue processing the remaining planned pull requests for the ecosystem, and exit with a non-zero status code after all planned pull requests have been attempted.