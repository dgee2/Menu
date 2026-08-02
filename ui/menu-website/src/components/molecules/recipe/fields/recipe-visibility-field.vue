<script setup lang="ts">
import { computed } from 'vue';
import SelectField from '@/components/atoms/form/select-field.vue';

const accessScope = defineModel<string>({ default: 'Private' });

const options = [
  { label: 'Private', value: 'Private' },
  { label: 'Visible to all Menu users', value: 'AuthenticatedUsers' },
] as const;

const selectedOption = computed({
  get: () => options.find((option) => option.value === accessScope.value) ?? options[0],
  set: (option) => {
    accessScope.value = option?.value ?? options[0].value;
  },
});
</script>

<template>
  <select-field v-model="selectedOption" :options="[...options]" label="Visibility" />
</template>

<style scoped></style>
