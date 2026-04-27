---
name: dependency-update-repo-context
description: Provides Menu repository-specific dependency update context, including Dependabot groups, branch naming rules, and how to interpret pending updates during automated Copilot dependency workflows.
user-invokable: false
---

# Dependency Update Repo Context

## Purpose

Use this skill to ground dependency update work in the Menu repository's actual automation rules before classifying updates or opening pull requests.

## Runtime parameters

Assume the current workspace is the target repository and the current working directory is the repository root.

When available, the caller should provide this runtime context in plain text:

- `Ecosystem`
- `Branch prefix` override
- `Default branch` override

## Repository facts

- Read `.github/dependabot.yml` to understand dependency groups.
- `nuget` updates have one named group: `aspire`.
- `npm` updates have named groups: `storybook`, `eslint`, `quasar`, `vue`, and `tanstack`.
- `github-actions` updates currently do not define named groups in `.github/dependabot.yml`.
- When the caller does not provide a branch-prefix override, use these repository conventions:
  - `.NET / NuGet` -> `copilot/dependency-update/dotnet`
  - `Node / npm / pnpm` -> `copilot/dependency-update/node`
  - `GitHub Actions` -> `copilot/dependency-update/github-actions`
- Branches must use the selected branch prefix with one of these suffixes:
  - `/simple`
  - `/tool-<group-name>`
  - `/code-change-<package-name>`

## Pending update source of truth

- Use the GitHub tools available in this run to identify which updates are currently pending for the selected ecosystem.
- Treat `.github/dependabot.yml` as grouping policy, not as the list of pending updates.
- Treat existing open pull requests or branches under the selected branch prefix as in-flight work that may need to be reused rather than recreated.

## CI alignment

- The validation commands in this workflow are a pre-check only.
- The authoritative gate is the repository CI in `.github/workflows/main.yml` once the pull request is created.
- Do not weaken the pre-checks. If a required command fails, stop and fail the job.