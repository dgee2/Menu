<script setup lang="ts">
import type { ValidationRule } from 'quasar';

const value = defineModel<string | null>();
withDefaults(
  defineProps<{
    label: string;
    hint?: string;
    rules?: ValidationRule[];
    type?: 'text' | 'textarea';
    /**
     * A message from outside the client-side rules — typically the server rejecting a value the
     * client had no way to check, such as a title that is already taken. Shown as a field error
     * rather than run as a rule, because it arrives after validation has already passed.
     */
    errorMessage?: string | null;
  }>(),
  { type: 'text', hint: undefined, rules: undefined, errorMessage: undefined },
);
</script>

<template>
  <q-input
    v-model="value"
    :type="type"
    :label="label"
    :hint="hint"
    :rules="rules"
    :error="!!errorMessage"
    :error-message="errorMessage ?? undefined"
  />
</template>

<style scoped></style>
