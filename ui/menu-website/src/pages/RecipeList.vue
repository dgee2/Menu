<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { useRecipeService } from '@/services/recipe-service';
import { userFacingMessage } from '@/services/api-error';
import { recipeAccessScopeBadgeLabels } from '@/services/recipe-labels';
import type { RecipeListScope } from '@/services/recipe-api';

const { useRecipeSearch } = useRecipeService();

// The API has always supported both scopes; only `mine` was ever reachable from the app.
const scopeOptions: { label: string; value: RecipeListScope }[] = [
  { label: 'Mine', value: 'mine' },
  { label: 'All Menu users', value: 'authenticated' },
];

const scope = ref<RecipeListScope>('mine');
const searchText = ref('');
const debouncedSearch = ref('');
let debounceTimer: ReturnType<typeof setTimeout> | undefined;

watch(searchText, (value) => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    debouncedSearch.value = value.trim();
  }, 300);
});

onBeforeUnmount(() => clearTimeout(debounceTimer));

const { data, isLoading, isError, error } = useRecipeSearch(scope, debouncedSearch);
const recipes = computed(() => data.value ?? []);

const emptyMessage = computed(() => {
  if (debouncedSearch.value) return 'No recipes match your search.';
  return scope.value === 'mine'
    ? 'You have not created any recipes yet.'
    : 'Nobody has shared a recipe yet.';
});

const errorMessage = computed(() =>
  userFacingMessage(error.value, 'Something went wrong loading recipes.'),
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
    <h1>Recipe search</h1>
    <q-input
      v-model="searchText"
      class="search-input q-mb-md"
      outlined
      clearable
      label="Search recipes"
      aria-label="Search recipes"
    >
      <template #prepend>
        <q-icon name="search" />
      </template>
    </q-input>
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
          <div v-if="recipe.summary" class="recipe-summary text-body2 text-grey-8">
            {{ recipe.summary }}
          </div>
          <div v-if="timings(recipe)" class="text-caption text-grey-7">{{ timings(recipe) }}</div>
          <q-badge :color="recipe.accessScope === 'Private' ? 'grey' : 'primary'">
            {{ recipeAccessScopeBadgeLabels[recipe.accessScope] }}
          </q-badge>
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

.search-input {
  max-width: 600px;
  margin-right: auto;
  margin-left: auto;
}

.recipe-summary {
  display: -webkit-box;
  overflow: hidden;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}
</style>
