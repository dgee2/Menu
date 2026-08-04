<script setup lang="ts">
import type { ValidationRule } from 'quasar';

const value = defineModel<string | null>();
withDefaults(
  defineProps<{
    label: string;
    hint?: string;
    rules?: ValidationRule[];
    type?: 'text' | 'textarea';
  }>(),
  { type: 'text', hint: undefined, rules: undefined },
);
</script>

<template>
  <q-input v-model="value" :type="type" :label="label" :hint="hint" :rules="rules" />
</template>

<style scoped></style>
