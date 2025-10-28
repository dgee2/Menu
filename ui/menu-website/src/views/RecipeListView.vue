<template>
  <q-list v-if="recipes && recipes.length" bordered separator>
    <q-item v-for="recipe in recipes" :key="recipe.id">
      <q-item-section>
        <div class="text-h6">{{ recipe.name }}</div>
        <div v-if="recipe.description" class="text-subtitle2">{{ recipe.description }}</div>
      </q-item-section>
    </q-item>
  </q-list>

  <div v-else-if="isLoading">Loading recipes...</div>
  <div v-else>No recipes found.</div>
</template>

<script setup lang="ts">
interface Recipe {
  id: string | number;
  name: string;
  description?: string | null;
  [key: string]: unknown;
}

// Use a type-only signature for defineProps to avoid creating an unused local variable
defineProps<{
  recipes: Recipe[];
  isLoading: boolean;
}>();
</script>
