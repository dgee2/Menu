<script setup lang="ts">
import { computed } from 'vue';
import SelectField from '@/components/atoms/form/select-field.vue';
import type { RecipeAccessScope } from '@/services/recipe-api';
import { recipeAccessScopeLabels } from '@/services/recipe-labels';

// The default lives with the owning form, not here, so the field can never display a
// value the parent has not actually been given.
const accessScope = defineModel<RecipeAccessScope>();

type VisibilityOption = { label: string; value: RecipeAccessScope };

// Built from the label map, so a scope added to the API's enum shows up here as a missing key at
// compile time rather than as an option that silently does not exist.
const options: VisibilityOption[] = (
  Object.keys(recipeAccessScopeLabels) as RecipeAccessScope[]
).map((value) => ({ label: recipeAccessScopeLabels[value], value }));

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
