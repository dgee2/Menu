---
name: frontend-component
description: Conventions for adding a new Vue 3 + Quasar component in the Menu frontend — correct folder, TypeScript Composition API style, co-located Storybook story, and the two-layer API service pattern.
user-invokable: true
context: inline
---

# Adding a New Frontend Component

## Purpose

Use this skill when creating a new Vue 3 component in `ui/menu-website/`. It covers folder placement, coding style, Storybook stories, and the mandatory two-layer API pattern.

## Repository facts

- Frontend root: `ui/menu-website/`
- Source: `ui/menu-website/src/`
- Path alias: `@/` maps to `src/`

## Component folder placement

Place components in the correct subfolder of `src/components/`:

| Type | Location | Example |
|---|---|---|
| Reusable form fields | `src/components/generic/form/` | `text-field`, `select-field` |
| Reusable header buttons | `src/components/generic/header/` | `header-button` |
| Recipe-specific components | `src/components/recipe/` | `new-recipe-form` |
| Recipe field sub-components | `src/components/recipe/fields/` | field components |
| Auth/nav buttons | `src/components/buttons/` | `LoginButton`, `LogoutButton` |

Pages live in `src/pages/` and are route-level components only.

## Component file conventions

Use Vue 3 Composition API with `<script setup lang="ts">`. Example skeleton:

```vue
<script setup lang="ts">
import type { PropType } from 'vue'

const props = defineProps({
  label: {
    type: String as PropType<string>,
    required: true,
  },
})
</script>

<template>
  <q-btn :label="props.label" />
</template>
```

- Always use TypeScript (`lang="ts"`).
- Use `import type` for type-only imports (`@typescript-eslint/consistent-type-imports` is enforced).
- Use Quasar components (`q-*`) for UI elements; do not add additional UI libraries.
- Reference other source files with the `@/` alias, not relative `../` paths where avoidable.

## Co-located Storybook story

Every component must have a co-located story file named `<ComponentName>.stories.ts` in the same folder. Example:

```ts
import type { Meta, StoryObj } from '@storybook/vue3'
import MyComponent from './MyComponent.vue'

const meta: Meta<typeof MyComponent> = {
  component: MyComponent,
}
export default meta

type Story = StoryObj<typeof MyComponent>

export const Default: Story = {
  args: {
    label: 'Example',
  },
}
```

## Two-layer API service pattern

Pages and components must **never** call the API layer (`recipe-api.ts`) directly. Always use the service layer:

| Layer | File | What it does |
|---|---|---|
| API layer | `src/services/recipe-api.ts` | Low-level `openapi-fetch` calls with typed functions |
| Service layer | `src/services/recipe-service.ts` | TanStack Query composables (`useRecipes`, `useCreateRecipe`); handles cache invalidation |

In a component, import and use a composable from the service layer:

```ts
import { useRecipes } from '@/services/recipe-service'

const { data: recipes, isLoading } = useRecipes()
```

## Routing

- Unauthenticated routes: add to `src/router/public.routes.ts`
- Authenticated routes: add to `src/router/authenticated.routes.ts` (protected by `authGuard`)
- Both groups use `MainLayout.vue` as the parent layout.

## Validation

Run from `ui/menu-website/`:

```bash
pnpm run lint
pnpm run build
pnpm run test
```
