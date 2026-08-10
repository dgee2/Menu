<script setup lang="ts">
import TextField from '@/components/atoms/form/text-field.vue';
import { requiredText } from '@/services/form-rules';

const name = defineModel<string | null>();

defineProps<{
  /** A server-side rejection of this name, such as a title the owner has already used. */
  serverError?: string | null;
}>();

const rules = [requiredText('Name is required')];
</script>

<template>
  <text-field
    v-model="name"
    label="Name"
    hint="Enter the name of the recipe"
    :rules="rules"
    :error-message="serverError"
  />
</template>

<style scoped></style>
