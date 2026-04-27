---
name: dependency-update-classification
description: Categorises pending dependency updates for the Menu repository into simple version bumps, tool-driven updates, and updates requiring code changes.
user-invokable: false
---

# Classify Dependency Updates

## Purpose

Use this skill to decide how each pending dependency update should be handled before any pull request is created or reused.

## Runtime parameters

Assume the current workspace is the target repository and the current working directory is the repository root.

When available, the caller should provide this runtime context in plain text:

- `Ecosystem`
- `Branch prefix` override
- `Default branch` override

## Categories

- **Simple version bump**: Only version numbers change in project or lock files. No upgrade tool needs to run and no application or test code needs to change.
- **Tool-driven update**: The package belongs to a named dependency group whose update should be prepared with a dedicated upgrade tool or workflow.
- **Code-changing update**: The update requires application or test source changes beyond version-number edits, regardless of whether it belongs to a named group.

## Classification rules

1. Classify every pending update in the provided ecosystem into exactly one category.
2. Do not skip any pending update.
3. Use `.github/dependabot.yml` to identify named groups.
4. Do not assume that every named group is automatically tool-driven. Mark it tool-driven only when the package or its ecosystem normally expects a dedicated upgrade tool or upgrade workflow.
5. If an update needs code changes, classify it as code-changing even if it also belongs to a named group.

## Menu repository guidance

- `aspire` NuGet updates are tool-driven when the workload or supporting tooling should be updated as part of the change.
- `storybook` npm updates are tool-driven when the Storybook upgrade workflow recommends running its upgrade tooling.
- `eslint`, `quasar`, `vue`, and `tanstack` are named groups, but they are not automatically tool-driven. Treat them as simple bumps unless the update evidence shows a dedicated upgrade tool or code changes are required.

## Pull request boundaries

- Create one pull request for all simple version bumps in this ecosystem using the selected branch prefix plus `/simple`.
- Create one pull request per tool-driven group using the selected branch prefix plus `/tool-<group-name>`.
- Create one pull request per code-changing dependency using the selected branch prefix plus `/code-change-<package-name>`.
- Reuse an existing open pull request for the same branch when one already exists.