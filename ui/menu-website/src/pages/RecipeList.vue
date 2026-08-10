<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRecipeService } from '@/services/recipe-service';
import { ApiError } from '@/services/api-error';
import type { RecipeListScope } from '@/services/recipe-api';

const { useRecipes } = useRecipeService();

// The API has always supported both scopes; only `mine` was ever reachable from the app.
const scopeOptions: { label: string; value: RecipeListScope }[] = [
  { label: 'My recipes', value: 'mine' },
  { label: 'Shared with everyone', value: 'authenticated' },
];

const scope = ref<RecipeListScope>('mine');

const { data, isLoading, isError, error } = useRecipes(scope);
const recipes = computed(() => data.value ?? []);

const heading = computed(
  () => scopeOptions.find((option) => option.value === scope.value)?.label ?? 'Recipes',
);

const emptyMessage = computed(() =>
  scope.value === 'mine'
    ? 'You have not created any recipes yet.'
    : 'Nobody has shared a recipe yet.',
);

const errorMessage = computed(() =>
  error.value instanceof ApiError
    ? error.value.userFacingMessage('Something went wrong loading recipes.')
    : 'Something went wrong loading recipes.',
);

const timings = (recipe: { effectiveTotalTimeMinutes?: number | null; servings?: number | null }) =>
  [
    recipe.effectiveTotalTimeMinutes == null ? null : `${recipe.effectiveTotalTimeMinutes} min`,
    recipe.servings == null ? null : `Serves ${recipe.servings}`,
  ]
    .filter((part): part is string => part !== null)
    .join(' · ');
</script>

<template>
  <q-page class="q-pa-md">
    <h1>{{ heading }}</h1>
    <q-btn-toggle
      v-model="scope"
      class="q-mb-md"
      no-caps
      unelevated
      toggle-color="primary"
      :options="scopeOptions"
    />
    <q-list v-if="recipes.length" bordered separator>
      <q-item v-for="recipe in recipes" :key="recipe.id" clickable :to="`/recipe/${recipe.id}`">
        <q-item-section>
          <div class="text-h6">{{ recipe.title }}</div>
          <div v-if="recipe.summary" class="text-body2 text-grey-8">{{ recipe.summary }}</div>
          <!-- The list carries summary and timings from the API; rendering only the title threw
               them away and made every recipe look identical. -->
          <div v-if="timings(recipe)" class="text-caption text-grey-7">{{ timings(recipe) }}</div>
        </q-item-section>
      </q-item>
    </q-list>
    <div v-else-if="isLoading">Loading recipes...</div>
    <div v-else-if="isError">
      <q-banner class="bg-negative text-white">{{ errorMessage }}</q-banner>
    </div>
    <div v-else>{{ emptyMessage }}</div>
  </q-page>
</template>

<style scoped>
.q-list {
  max-width: 600px;
  margin: 0 auto;
}
</style>
