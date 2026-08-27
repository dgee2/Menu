<script setup lang="ts">
import { computed } from 'vue';
import RecipeForm from '@/components/organisms/recipe/recipe-form.vue';
import { useRecipeService } from '@/services/recipe-service';
import { ApiError, userFacingMessage } from '@/services/api-error';

/**
 * Editing lives on its own route rather than toggling the detail page into an edit mode, so the
 * unsaved-changes guard has a route transition to hook, and so an in-progress edit survives a
 * refresh as a URL the user can return to.
 */
const props = defineProps<{
  recipeId: string;
}>();

const { useRecipe } = useRecipeService();

const recipeId = computed(() => props.recipeId);
const { data: recipe, isLoading, isError, error } = useRecipe(recipeId);

const isNotFound = computed(() => error.value instanceof ApiError && error.value.status === 404);
const loadErrorMessage = computed(() =>
  userFacingMessage(error.value, 'Something went wrong loading this recipe.'),
);
</script>

<template>
  <q-page class="q-pa-md">
    <div v-if="isLoading">Loading recipe...</div>
    <div v-else-if="isError && !isNotFound">
      <q-banner class="bg-negative text-white">{{ loadErrorMessage }}</q-banner>
    </div>
    <div v-else-if="!recipe">Recipe not found.</div>
    <!-- The server decides who may edit. Reaching this page for someone else's recipe shows the
         refusal here rather than letting the user fill in a form that would only ever 403. -->
    <div v-else-if="!recipe.canEdit">
      <q-banner class="bg-warning">You do not have permission to edit this recipe.</q-banner>
      <q-btn class="q-mt-md" flat label="Back to recipe" :to="`/recipe/${recipe.id}`" />
    </div>
    <div v-else>
      <h1 class="q-mt-none">Edit recipe</h1>
      <!-- Keyed on the id so moving between two edit URLs builds a fresh form: recipe-form reads
           initialRecipe once, in its setup, and the route record is the same for both. -->
      <recipe-form :key="recipe.id" :initial-recipe="recipe" />
    </div>
  </q-page>
</template>

<style scoped></style>
