<script setup lang="ts">
import TextField from '@/components/atoms/form/text-field.vue';

const name = defineModel<string | null>();

const rules = [(val: string | null) => !!val?.trim() || 'Name is required'];
</script>

<template>
  <text-field v-model="name" label="Name" hint="Enter the name of the recipe" :rules="rules" />
</template>

<style scoped></style>
