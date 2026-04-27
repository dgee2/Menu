---
name: dependency-update-pr-description
description: Defines the required pull request description contract for automated dependency update pull requests in the Menu repository.
user-invokable: false
---

# Write Dependency Update Pull Request Descriptions

## Purpose

Use this skill when drafting or updating the pull request description for a dependency update change.

## Runtime parameters

Assume the current workspace is the target repository and the current working directory is the repository root.

When available, the caller should provide this runtime context in plain text:

- `Ecosystem`
- `Branch prefix` override
- `Default branch` override

## Required contents

Every pull request description must explicitly include:

- Why the dependencies in this pull request were grouped together.
- Whether the pull request is a `simple version bump`, `tool-driven update`, or `an update requiring code changes`.
- Any best-practice change introduced or suggested by the update, with an explicit decision of `implement now` or `do not implement now` and the reasoning for that decision.
- The validation commands that were run and whether they succeeded.

## Best-practice note

- If no best-practice change was introduced or suggested, state that explicitly.
- Record the decision as `do not implement now` when no new best-practice action is required.