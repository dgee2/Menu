<script setup lang="ts">
import type { ValidationRule } from 'quasar';

const value = defineModel<number | null>();
defineProps<{
  label: string;
  hint?: string;
  rules?: ValidationRule[];
  min?: number;
  step?: number;
  /**
   * Greyed-out text shown while the field is empty. Used where a value would be derived if none is
   * given — a placeholder shows the figure without claiming the user entered it.
   */
  placeholder?: string;
}>();

const onUpdate = (val: string | number | null) => {
  if (val === null || val === '') {
    value.value = null;
    return;
  }
  const num = Number(val);
  value.value = Number.isNaN(num) ? null : num;
};
</script>

<template>
  <q-input
    :model-value="value"
    type="number"
    :label="label"
    :hint="hint"
    :rules="rules"
    :min="min"
    :step="step"
    :placeholder="placeholder"
    @update:model-value="onUpdate"
  />
</template>

<style scoped></style>
