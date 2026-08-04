<script setup lang="ts">
import { computed } from 'vue';
import SelectField from '@/components/atoms/form/select-field.vue';
import type { RecipeAccessScope } from '@/services/recipe-api';

// The default lives with the owning form, not here, so the field can never display a
// value the parent has not actually been given.
const accessScope = defineModel<RecipeAccessScope>();

type VisibilityOption = { label: string; value: RecipeAccessScope };

const options: VisibilityOption[] = [
  { label: 'Private', value: 'Private' },
  { label: 'Visible to all Menu users', value: 'AuthenticatedUsers' },
];

const selectedOption = computed<VisibilityOption | undefined>({
  // An unrecognised scope renders as empty rather than silently reading as Private.
  get: () => options.find((option) => option.value === accessScope.value),
  set: (option) => {
    accessScope.value = option?.value;
  },
});
</script>

<template>
  <select-field
    v-model="selectedOption"
    :options="options"
    label="Visibility"
    hint="Who can see this recipe"
  />
</template>

<style scoped></style>
