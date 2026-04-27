---
name: dependency-update-validation
description: Defines the required validation commands for automated dependency update pull requests in the Menu repository.
user-invokable: false
---

# Validate Dependency Update Pull Requests

## Purpose

Use this skill before creating or finalising each dependency update pull request.

## Runtime parameters

Assume the current workspace is the target repository and the current working directory is the repository root.

When available, the caller should provide this runtime context in plain text:

- `Ecosystem`
- `Branch prefix` override
- `Default branch` override

## Validation policy

- Run the validation commands that match the pull request ecosystem.
- Confirm every required command succeeds before creating or finalising the pull request.
- If any required command fails, do not create or finalise that pull request.
- Exit with a non-zero status code so the workflow fails clearly.

## .NET / NuGet pull requests

Run these commands from the repository root, in this order:

```bash
cd backend
dotnet workload install aspire
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

## Node / npm / pnpm pull requests

Run these commands from the repository root, in this order:

```bash
cd backend
dotnet workload install aspire
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

## GitHub Actions pull requests

Run these commands from the repository root, in this order:

```bash
ruby -e 'require "yaml"; Dir[".github/workflows/*.yml"].each { |path| YAML.load_file(path, aliases: true) }'
```

Then run all `.NET / NuGet` validation commands and all `Node / npm / pnpm` validation commands listed above.