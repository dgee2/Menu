<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import RecipeNameField from '@/components/molecules/recipe/fields/recipe-name-field.vue';
import TextField from '@/components/atoms/form/text-field.vue';
import NumberField from '@/components/atoms/form/number-field.vue';
import IngredientRowEditor from '@/components/molecules/recipe/ingredient-row-editor.vue';
import StepRowEditor from '@/components/molecules/recipe/step-row-editor.vue';
import RecipeVisibilityField from '@/components/molecules/recipe/fields/recipe-visibility-field.vue';
import { useRecipeService } from '@/services/recipe-service';
import type {
  RecipeAccessScope,
  RecipeIngredientItem,
  RecipeStepItem,
  UpsertRecipe,
} from '@/services/recipe-api';

const router = useRouter();
const { useCreateRecipe } = useRecipeService();
const { mutateAsync: createRecipe, isPending, isError } = useCreateRecipe();

const title = ref<string | null>(null);
const accessScope = ref<RecipeAccessScope>('Private');
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

interface IngredientRow extends RecipeIngredientItem {
  rowId: string;
}

interface StepRow extends RecipeStepItem {
  rowId: string;
}

const moveItem = <T,>(array: T[], index: number, offset: -1 | 1) => {
  const targetIndex = index + offset;
  if (targetIndex < 0 || targetIndex >= array.length) return;
  const [moved] = array.splice(index, 1);
  if (!moved) return;
  array.splice(targetIndex, 0, moved);
};

const ingredients = ref<IngredientRow[]>([]);

const addIngredient = () => {
  ingredients.value.push({
    ingredientText: '',
    measureText: '',
    sectionTitle: null,
    preparationText: null,
    isOptional: false,
    rowId: crypto.randomUUID(),
  });
};

const removeIngredient = (index: number) => {
  ingredients.value.splice(index, 1);
};

const moveIngredient = (index: number, offset: -1 | 1) => moveItem(ingredients.value, index, offset);

const steps = ref<StepRow[]>([]);

const addStep = () => {
  steps.value.push({
    instructionText: '',
    title: null,
    durationMinutes: null,
    rowId: crypto.randomUUID(),
  });
};

const removeStep = (index: number) => {
  steps.value.splice(index, 1);
};

const moveStep = (index: number, offset: -1 | 1) => moveItem(steps.value, index, offset);

const onSubmit = async () => {
  const recipe: UpsertRecipe = {
    title: title.value ?? '',
    summary: summary.value,
    yieldText: yieldText.value,
    servings: servings.value,
    prepTimeMinutes: prepTimeMinutes.value,
    cookTimeMinutes: cookTimeMinutes.value,
    totalTimeMinutes: totalTimeMinutes.value,
    accessScope: accessScope.value,
    ingredients: ingredients.value.map((ingredient, index) => ({
      ingredientText: ingredient.ingredientText,
      measureText: ingredient.measureText,
      sectionTitle: ingredient.sectionTitle,
      preparationText: ingredient.preparationText,
      isOptional: ingredient.isOptional,
      sortOrder: index,
    })),
    steps: steps.value.map((step, index) => ({
      instructionText: step.instructionText,
      title: step.title,
      durationMinutes: step.durationMinutes,
      sortOrder: index,
    })),
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
  <q-form greedy class="q-gutter-md" @submit="onSubmit">
    <q-banner v-if="isError" class="bg-negative text-white">
      Failed to save recipe. Please try again.
    </q-banner>
    <recipe-name-field v-model="title" />
    <recipe-visibility-field v-model="accessScope" />
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
    <div class="text-h6">Ingredients</div>
    <ingredient-row-editor
      v-for="(ingredient, index) in ingredients"
      :key="ingredient.rowId"
      v-model:ingredient-text="ingredient.ingredientText"
      v-model:measure-text="ingredient.measureText"
      v-model:section-title="ingredient.sectionTitle"
      v-model:preparation-text="ingredient.preparationText"
      v-model:is-optional="ingredient.isOptional"
      :can-move-up="index > 0"
      :can-move-down="index < ingredients.length - 1"
      @remove="removeIngredient(index)"
      @move-up="moveIngredient(index, -1)"
      @move-down="moveIngredient(index, 1)"
    />
    <q-btn label="Add ingredient" icon="add" flat @click="addIngredient" />

    <div class="text-h6">Steps</div>
    <step-row-editor
      v-for="(step, index) in steps"
      :key="step.rowId"
      v-model:instruction-text="step.instructionText"
      v-model:title="step.title"
      v-model:duration-minutes="step.durationMinutes"
      :can-move-up="index > 0"
      :can-move-down="index < steps.length - 1"
      @remove="removeStep(index)"
      @move-up="moveStep(index, -1)"
      @move-down="moveStep(index, 1)"
    />
    <q-btn label="Add step" icon="add" flat @click="addStep" />

    <q-btn label="Save recipe" type="submit" color="primary" :loading="isPending" />
  </q-form>
</template>

<style scoped></style>
