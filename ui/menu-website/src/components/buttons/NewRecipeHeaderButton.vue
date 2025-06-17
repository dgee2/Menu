<script setup lang="ts">
import { useRouter } from 'vue-router';

const router = useRouter();

const goToNewRecipePage = async () => {
  await router.push('/new-recipe');
};
</script>

<template>
  <q-btn flat round icon="add" @click="goToNewRecipePage" />
</template>
