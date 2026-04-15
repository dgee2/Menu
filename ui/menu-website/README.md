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

The frontend OpenAPI client under `src/generated/open-api/` is a generated build artifact and is not committed. On a clean checkout, first generate the backend OpenAPI document from the repository root:

```sh
dotnet build MenuApi/MenuApi.csproj
```

Then, from `ui/menu-website`, regenerate the frontend client if you need it explicitly:

```sh
pnpm generate-openapi
```

`pnpm dev`, `pnpm aspire`, `pnpm type-check`, `pnpm build`, `pnpm test`, `pnpm test:e2e`, `pnpm test:storybook`, `pnpm storybook`, and `pnpm build-storybook` regenerate `src/generated/open-api/menu-api.ts` for you, but they still require `../../open-api/menu-api.json` from the backend build to exist first.

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

```sh
# Install browsers for the first run
npx playwright install

# When testing on CI, must build the project first
pnpm build

# Runs the end-to-end tests
pnpm test:e2e
# Runs the tests only on Chromium
pnpm test:e2e --project=chromium
# Runs the tests of a specific file
pnpm test:e2e tests/example.spec.ts
# Runs the tests in debug mode
pnpm test:e2e --debug
```

### Lint with [ESLint](https://eslint.org/)

```sh
pnpm lint
```

`pnpm lint` does not regenerate the OpenAPI client because it does not depend on `src/generated/open-api/menu-api.ts`.
