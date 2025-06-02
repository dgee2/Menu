<script setup lang="ts">
import IngredientField from '../generic/form/ingredient-field.vue';
import RecipeNameField from './recipe-fields/recipe-name-field.vue';
</script>

<template>
  <q-form>
    <recipe-name-field />
    <ingredient-field />
  </q-form>
</template>

<style scoped></style>
