<script setup lang="ts">
import TextField from '@/components/generic/form/text-field.vue';

const name = defineModel<string>();
</script>

<template>
  <text-field v-model="name" label="Name" hint="Enter the name of the recipe" />
</template>

<style scoped></style>
