<template>
  <q-page padding>
    <h1>All Recipes</h1>
    <RecipeListView :recipes="recipes" :is-loading="isLoading" />
  </q-page>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRecipeService } from '@/services/recipe-service';
import RecipeListView from '../views/RecipeListView.vue';

const { useRecipes } = useRecipeService();

const { data, isLoading } = useRecipes();
const recipes = computed(() => data.value ?? []);
</script>
