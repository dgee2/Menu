<script setup lang="ts">
import { useRouter } from 'vue-router';

const router = useRouter();

const goToRecipesPage = async () => {
  await router.push('/recipes');
};
</script>

<template>
  <q-btn flat @click="goToRecipesPage">Recipes</q-btn>
</template>

<style scoped></style>
