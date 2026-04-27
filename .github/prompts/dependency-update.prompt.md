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

## Operating rules

- Use the available GitHub and repository tools to identify the pending dependency updates for the selected ecosystem.
- Read `.github/dependabot.yml` to understand the repo-defined dependency groups, but do not treat that file as the source of truth for what updates are pending.
- Complete the work in this order: classify updates, decide pull request boundaries, validate each planned pull request, then create or update the pull request.
- Every pending update must end up in exactly one pull request.
- Reuse an existing open pull request for the same branch when one already exists.
- Target the repository default branch unless the caller provides an explicit default branch override.
- Apply the attached classification, validation, PR-description, and repo-context skills.
- If any required validation command fails, do not create or finalise the pull request. Exit with a non-zero status code so the workflow fails clearly.