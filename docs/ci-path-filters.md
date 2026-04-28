# CI Path Filters

## Overview

The CI workflow uses conservative path filters to skip unaffected work while ensuring no required validation is missed for cross-stack changes.

## Implementation

The workflow uses the [dorny/paths-filter](https://github.com/dorny/paths-filter) action to detect which files have changed in pull requests and conditionally runs jobs based on those changes.

### Path Filter Configuration

#### Create OpenAPI Spec Job

The Create OpenAPI spec job (`backend-build`) runs when any of these paths change:

- `backend/**` - All backend projects, solution files, and backend-specific configuration grouped under the `backend/` directory
- `ui/**` - Frontend application changes that should validate against a freshly generated OpenAPI document
- `.github/workflows/main.yml` - The workflow file itself

#### Backend Test Jobs

Backend test jobs (`backend-tests`, `backend-integration-tests`) run when any of these paths change:

- `backend/**` - All backend projects, solution files, and backend-specific configuration grouped under the `backend/` directory
- `.github/workflows/main.yml` - The workflow file itself

#### Frontend Job

The frontend job (`frontend`) runs when any of these files change, or when the Create OpenAPI spec job runs:

- `backend/**` - Backend changes regenerate the OpenAPI document and require frontend validation against the new artifact
- `ui/**` - Any file in the frontend directory
- `open-api/**` - OpenAPI specification files (triggers type regeneration)
- `.github/workflows/main.yml` - The workflow file itself

## Behavior by Event Type

### Pull Requests

- **Changes detected**: Only jobs matching the changed files run
- **No changes detected**: Backend/frontend build and test jobs are skipped when no relevant files change
- **Workflow file changes**: Both frontend and backend jobs run (conservative approach)

### Push Events (main/master branches)

- **All jobs always run**: Path filters are only applied to pull requests
- This ensures complete validation on the main branches

### Workflow Dispatch

- **All jobs always run**: Manual triggers run complete validation

## Cross-Stack Scenarios

### Scenario: Backend-only changes (e.g., only files under `backend/`)

- ✅ Create OpenAPI spec runs
- ✅ Backend test jobs run
- ✅ Frontend job runs
- The frontend validates against the freshly generated OpenAPI spec artifact

### Scenario: Frontend-only changes (e.g., only `ui/` files)

- ✅ Create OpenAPI spec runs
- ❌ Backend test jobs are skipped
- ✅ Frontend job runs
- The frontend validates against the freshly generated OpenAPI spec artifact

### Scenario: OpenAPI file changes (e.g., only `open-api/` files)

- ❌ Create OpenAPI spec is skipped
- ❌ Backend test jobs are skipped
- ✅ Frontend job runs to regenerate types
- The frontend job does **not** get a freshly generated OpenAPI spec artifact in this scenario
- Because the generated JSON spec is not tracked in the repository, `open-api/**`-only changes cannot rely on an existing checked-in spec when `backend-build` is skipped

### Scenario: Both backend and frontend changes

- ✅ Create OpenAPI spec runs
- ✅ Backend test jobs run
- ✅ Frontend job runs
- Normal full validation

### Scenario: Workflow file changes

- ✅ Backend jobs run
- ✅ Frontend job runs
- Conservative approach to ensure workflow changes don't break validation

## Frontend Job Dependencies

The frontend job has special dependency handling:

```yaml
needs: [changes, backend-build]
if: |
  !cancelled() &&
   (
      github.event_name != 'pull_request' ||
      needs.changes.outputs.openapi == 'true' ||
      needs.changes.outputs.frontend == 'true'
   ) &&
  (needs.backend-build.result == 'success' || needs.backend-build.result == 'skipped')
```

This allows the frontend to run after Create OpenAPI spec for backend-only and frontend-only changes, while still supporting `open-api/**`-only changes when `backend-build` is skipped.

### OpenAPI Artifact Handling

The "Download OpenAPI document" step in the frontend job is conditional:

```yaml
- name: Download OpenAPI document
  if: needs.backend-build.result == 'success'
  uses: actions/download-artifact@v4
```

- If backend-build runs and succeeds, the new OpenAPI spec is downloaded
- If backend-build is skipped, the existing OpenAPI spec from the repository is used

## Performance Benefits

Path filters improve CI throughput by:

1. **Reducing backend test time**: Backend test jobs are skipped for frontend-only and `open-api/**`-only PRs
2. **Preserving fresh API validation**: Create OpenAPI spec still runs for backend and frontend app changes so frontend validation uses a current artifact
3. **Reducing runner usage**: Documentation-only and `open-api/**`-only PRs still skip unaffected jobs

### Example Savings

- **Frontend-only PR**: Saves ~3-5 minutes (skips backend test jobs while still regenerating OpenAPI)
- **OpenAPI-only PR**: Saves ~5-7 minutes (skips backend jobs and runs only frontend validation)
- **Documentation-only PR**: Saves ~7-10 minutes (skips all build/test jobs, runs only `changes` and `dependency-review`)

## Guardrails

The implementation includes several guardrails to prevent missing validation:

1. **Conservative structure**: Backend projects and backend-only config files live under `backend/`, so one path captures solution, project, and analyzer changes together
2. **Workflow file triggers both**: Changes to the workflow file trigger all jobs
3. **Always run on push**: All jobs run on pushes to main/master branches
4. **Dependency review still runs**: The dependency-review job always runs on PRs regardless of changed files

## Testing Path Filters

To test the path filters work correctly:

1. Create a PR with only backend changes (e.g., modify a `.cs` file)
   - Verify Create OpenAPI spec, backend test jobs, and frontend job all run

2. Create a PR with only frontend changes (e.g., modify a `.vue` file)
   - Verify Create OpenAPI spec and frontend job run, and backend test jobs are skipped

3. Create a PR with both backend and frontend changes
   - Verify all jobs run

4. Create a PR with only `open-api/` changes
   - Verify Create OpenAPI spec and backend test jobs are skipped, and frontend job runs

5. Create a PR with only documentation changes (e.g., modify a `.md` file)
   - Verify the `changes` job runs, `dependency-review` runs, and build/test jobs are skipped

## Troubleshooting

### Jobs unexpectedly skipped

- Check if the changed files match the path patterns
- Verify the PR is against the correct base branch
- Check if this is a push event (push events always run all jobs)

### Jobs not being skipped

- Ensure the event is a pull_request (not push)
- Check if the workflow file was changed (triggers all jobs)
- Verify the paths-filter action is running correctly in the changes job

### Frontend fails because OpenAPI spec is missing

This can happen if:

- The backend changed but wasn't included in the PR
- The OpenAPI spec in the repo is out of sync

**Solution**: Include both backend and frontend changes in the same PR when the OpenAPI contract changes.
