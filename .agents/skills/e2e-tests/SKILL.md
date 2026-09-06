---
name: e2e-tests
description: Run and maintain Menu's full-stack Playwright E2E smoke suite; use for Auth0, Aspire, recipe CRUD, route smoke, failure triage, or deciding whether an E2E assertion may change.
---

# End-to-end tests

Use this skill for the browser suite in `ui/menu-website/e2e/`. Read [`write-tests`](../write-tests/SKILL.md) for repository test conventions and [`playwright-cli`](../playwright-cli/SKILL.md) for interactive browser and trace tooling. This skill owns full-stack execution, test-safety, and triage rules; it does not replace those references.

## Run the suite

From `ui/menu-website/`, run:

```powershell
pnpm test:e2e
```

Playwright's `webServer` starts the Aspire AppHost (`backend/Menu.AppHost`) when the configured UI URL is not already serving. Aspire brings up the SQL Server container, migration service, API, and UI. The UI is `http://localhost:65276`; the API is `http://localhost:65273`. Allow up to five minutes for a cold start (Docker image pull, SQL initialization, migrations, and frontend startup); a warm run is shorter. Docker Desktop must be running.

If a healthy stack is already running on the configured UI URL, Playwright reuses it because `reuseExistingServer: true`. Inspect the existing Aspire dashboard and logs before stopping anything. A reused stack must be the same checkout/configuration under test; otherwise stop it through the normal Aspire workflow and start a clean stack.

On Windows, use the `Start-Job` timeout pattern in the root [`AGENTS.md`](../../../AGENTS.md) for this long-running command. Keep the timeout longer than the five-minute cold-start allowance and preserve the job's output and exit status.

## Prerequisites and authentication

Before authenticated tests, confirm:

- Docker is available and the AppHost can run.
- AppHost user secrets contain `Parameters:Auth0Domain` and `Parameters:Auth0Audience` (environment form: `Parameters__Auth0Domain` and `Parameters__Auth0Audience`); the audience normally matches `http://localhost:65273`.
- `ui/menu-website/.env.e2e` contains `Parameters__Auth0Domain` and `Parameters__Auth0Audience`, and `E2E_AUTH0_USERNAME` plus `E2E_AUTH0_PASSWORD` are set in the Windows environment of the shell running Playwright. Obtain missing credentials from the repository owner; never substitute mocks or an injected token.

The setup project performs the real Auth0 login and saves temporary state at `.playwright/.auth/user.json`. Auth0 consent may appear on a fresh browser profile: complete it deliberately and treat `consent_required`, missing bearer authorization, or a failed authenticated API response as a product/authentication failure. Never print credentials, cookies, tokens, or storage state; never commit `.env.e2e.local` or any `storageState` file. CI uses a separate user and secrets.

## Failure triage

Treat a red test as an application or environment regression until evidence says otherwise. First inspect Aspire dashboard logs for migration/API/UI startup errors, then inspect the Playwright HTML report and trace. The report is `ui/menu-website/playwright-report/`; screenshots, videos, and traces are under `ui/menu-website/test-results/`. Open a trace with `pnpm exec playwright show-trace <path-to-trace.zip>`, then examine the failing action, console, network, and response details. Check Docker/container health, ports, Auth0 configuration, and whether a reused stack is stale before editing a spec.

## Assertion and data-safety rules

A spec may change only when:

- Deliberate product behavior changed (route, label, field, or contract), and the change is traceable to an issue or deliberate commit.
- A selector is brittle; strengthen it with a role-, label-, or other semantic locator.
- Timing is genuinely asynchronous; replace the race with an `expect`-based wait or response wait. Fixed sleeps are not synchronization.

Diagnose an unexplained failure first. Preserve status-code checks, expected network requests, and meaningful UI assertions. A failure in Auth0 or token acquisition is a regression signal, not permission to bypass authentication. A fix must not remove an assertion, loosen a status check, add `test.skip`/`test.fixme`/`test.only`, or mock the real Auth0/API path. CI's `forbidOnly` is a backstop, not a development workflow.

Every mutating test owns uniquely identifiable data and cleans it up in `finally`; use worker/shard/time/UUID components in titles and look up by unique title, never by list length, order, or a shared fixture. Tests must be safe under Playwright workers and future shards. Network assertions prove the intended request and authenticated bearer header happened; do not route/mock the API to make a CRUD or auth smoke test green. Keep public tests public and authenticated tests on the real Auth0 flow.

## Completion gate

Before handing off an E2E-related change, run the relevant Playwright project/spec and repository checks required by the changed area. Record the exact command and whether it passed, failed, timed out, or was blocked by Docker/Auth0 credentials. A completed run includes the report/trace location when failures occur, and no secrets or storage state in the diff.
