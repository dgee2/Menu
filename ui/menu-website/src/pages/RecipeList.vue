<template>
  <q-page class="q-pa-md">
    <h1>All Recipes</h1>
    <q-list v-if="recipes && recipes.length" bordered separator>
      <q-item v-for="recipe in recipes" :key="recipe.id">
        <q-item-section>
          <div class="text-h6">{{ recipe.name }}</div>
          <!--          <div class="text-subtitle2">{{ recipe.description }}</div>-->
        </q-item-section>
      </q-item>
    </q-list>
    <div v-else-if="isLoading">Loading recipes...</div>
    <div v-else>No recipes found.</div>
  </q-page>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRecipeService } from '@/services/recipe-service';

const { useRecipes } = useRecipeService();

const { data, isLoading } = useRecipes();
const recipes = computed(() => data.value ?? []);
</script>

<style scoped>
.q-list {
  max-width: 600px;
  margin: 0 auto;
}
</style>
