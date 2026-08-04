<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import RecipeNameField from '@/components/molecules/recipe/fields/recipe-name-field.vue';
import TextField from '@/components/atoms/form/text-field.vue';
import NumberField from '@/components/atoms/form/number-field.vue';
import { useRecipeService } from '@/services/recipe-service';
import type { UpsertRecipe } from '@/services/recipe-api';

const router = useRouter();
const { useCreateRecipe } = useRecipeService();
const { mutateAsync: createRecipe, isPending, isError } = useCreateRecipe();

const title = ref<string | null>(null);
const summary = ref<string | null>(null);
const yieldText = ref<string | null>(null);
const servings = ref<number | null>(null);
const prepTimeMinutes = ref<number | null>(null);
const cookTimeMinutes = ref<number | null>(null);
const totalTimeMinutes = ref<number | null>(null);

const nonNegativeIntegerRules = [
  (val: number | null) => val == null || Number.isInteger(val) || 'Must be a whole number',
  (val: number | null) => val == null || val >= 0 || 'Must be 0 or greater',
];

const onSubmit = async () => {
  const recipe: UpsertRecipe = {
    title: title.value ?? '',
    summary: summary.value,
    yieldText: yieldText.value,
    servings: servings.value,
    prepTimeMinutes: prepTimeMinutes.value,
    cookTimeMinutes: cookTimeMinutes.value,
    totalTimeMinutes: totalTimeMinutes.value,
    accessScope: 'Private',
    ingredients: [],
    steps: [],
  };

  try {
    const created = await createRecipe(recipe);
    await router.push(`/recipe/${created.id}`);
  } catch {
    // isError from useMutation already reflects this; swallow to avoid an unhandled rejection.
  }
};
</script>

<template>
  <q-form class="q-gutter-md" @submit="onSubmit">
    <q-banner v-if="isError" class="bg-negative text-white">
      Failed to save recipe. Please try again.
    </q-banner>
    <recipe-name-field v-model="title" />
    <text-field v-model="summary" type="textarea" label="Summary" hint="A short description of the recipe" />
    <text-field v-model="yieldText" label="Yield" hint="e.g. One 9-inch cake" />
    <number-field v-model="servings" label="Servings" :min="0" :step="1" :rules="nonNegativeIntegerRules" />
    <number-field
      v-model="prepTimeMinutes"
      label="Prep time (minutes)"
      :min="0"
      :step="1"
      :rules="nonNegativeIntegerRules"
    />
    <number-field
      v-model="cookTimeMinutes"
      label="Cook time (minutes)"
      :min="0"
      :step="1"
      :rules="nonNegativeIntegerRules"
    />
    <number-field
      v-model="totalTimeMinutes"
      label="Total time (minutes)"
      :min="0"
      :step="1"
      :rules="nonNegativeIntegerRules"
    />
    <q-btn label="Save recipe" type="submit" color="primary" :loading="isPending" />
  </q-form>
</template>

<style scoped></style>
