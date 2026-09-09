# menu-website

This template should help get you started developing with Vue 3 in Vite.

## Recommended IDE Setup

[VSCode](https://code.visualstudio.com/) + [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (and disable Vetur).

## Type Support for `.vue` Imports in TS

TypeScript cannot handle type information for `.vue` imports by default, so we replace the `tsc` CLI with `vue-tsc` for type checking. In editors, we need [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) to make the TypeScript language service aware of `.vue` types.

## Customize configuration

See [Vite Configuration Reference](https://vite.dev/config/).

## Project Setup

```sh
pnpm install
```

### Compile and Hot-Reload for Development

```sh
pnpm dev
```

### Type-Check, Compile and Minify for Production

```sh
pnpm build
```

### Run Unit Tests with [Vitest](https://vitest.dev/)

```sh
pnpm test
```

### Run End-to-End Tests with [Playwright](https://playwright.dev)

The end-to-end command starts the complete Aspire stack: the SQL Server container,
database migrations, API, and UI. Docker must be running. Playwright waits for the UI at
`http://localhost:65276` and reuses an already-running stack on that address.

```sh
# Install browsers for the first run
npx playwright install

# Starts the Aspire stack and runs the end-to-end tests
pnpm test:e2e
# Runs the tests only on Chromium
pnpm test:e2e --project=chromium
# Runs the tests of a specific file
pnpm test:e2e e2e/public.spec.ts
# Runs the tests in debug mode
pnpm test:e2e --debug
```

The API is available at `http://localhost:65273`. Playwright writes its HTML report to
`playwright-report/` and failure artifacts, including traces captured on retry, to
`test-results/`.

Authenticated end-to-end tests require a dedicated local Auth0 Database user. Set
`E2E_AUTH0_USERNAME` and `E2E_AUTH0_PASSWORD` as Windows environment variables in the
shell that runs Playwright. Copy `.env.e2e.example` to `.env.e2e` and set the
`Parameters__Auth0Domain` and `Parameters__Auth0Audience` values there. Credentials
must never be committed or printed:

```sh
Copy-Item .env.e2e.example .env.e2e
```

The `.env.e2e` file contains non-secret connection parameters and may be committed;
keep credentials exclusively in the Windows environment.

The Auth0 setup project must handle the localhost API consent screen both when it is
shown on first login and when consent has already been granted. This account is for
local testing only; CI requires a separate user and GitHub Actions secrets.

### Lint with [ESLint](https://eslint.org/)

```sh
pnpm lint
```
