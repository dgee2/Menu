<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { onBeforeRouteLeave, useRouter } from 'vue-router';
import RecipeNameField from '@/components/molecules/recipe/fields/recipe-name-field.vue';
import TextField from '@/components/atoms/form/text-field.vue';
import NumberField from '@/components/atoms/form/number-field.vue';
import IngredientRowEditor from '@/components/molecules/recipe/ingredient-row-editor.vue';
import StepRowEditor from '@/components/molecules/recipe/step-row-editor.vue';
import RecipeVisibilityField from '@/components/molecules/recipe/fields/recipe-visibility-field.vue';
import { useRecipeService } from '@/services/recipe-service';
import { ApiError } from '@/services/api-error';
import { nonNegativeIntegerRules } from '@/services/form-rules';
import { isBlankIngredientRow, isBlankStepRow } from '@/services/recipe-rows';
import { effectiveTotalTimeMinutes } from '@/services/recipe-timing';
import type {
  RecipeAccessScope,
  RecipeDetail,
  RecipeIngredientItem,
  RecipeStepItem,
  UpsertRecipe,
} from '@/services/recipe-api';

/**
 * The one recipe editor, for both create and edit.
 *
 * `initialRecipe` is what distinguishes them: absent means create. There is no second component and
 * no wrapper, so a change to the payload, the validation or the dirty check cannot land in one mode
 * and be forgotten in the other.
 */
const props = defineProps<{
  initialRecipe?: RecipeDetail;
}>();

const router = useRouter();
const { useCreateRecipe, useUpdateRecipe } = useRecipeService();
const { mutateAsync: createRecipe, isPending: isCreating } = useCreateRecipe();
const { mutateAsync: updateRecipe, isPending: isUpdating } = useUpdateRecipe();

const isEditing = computed(() => props.initialRecipe !== undefined);
const isPending = computed(() => isCreating.value || isUpdating.value);
const submitLabel = computed(() => (isEditing.value ? 'Save changes' : 'Save recipe'));

const title = ref<string | null>(props.initialRecipe?.title ?? null);
const accessScope = ref<RecipeAccessScope>(props.initialRecipe?.accessScope ?? 'Private');
const summary = ref<string | null>(props.initialRecipe?.summary ?? null);
const yieldText = ref<string | null>(props.initialRecipe?.yieldText ?? null);
const servings = ref<number | null>(props.initialRecipe?.servings ?? null);
const prepTimeMinutes = ref<number | null>(props.initialRecipe?.prepTimeMinutes ?? null);
const cookTimeMinutes = ref<number | null>(props.initialRecipe?.cookTimeMinutes ?? null);
const totalTimeMinutes = ref<number | null>(props.initialRecipe?.totalTimeMinutes ?? null);

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

const blankIngredient = (): IngredientRow => ({
  ingredientText: '',
  measureText: '',
  sectionTitle: null,
  preparationText: null,
  isOptional: false,
  sortOrder: 0,
  rowId: crypto.randomUUID(),
});

const blankStep = (): StepRow => ({
  instructionText: '',
  title: null,
  durationMinutes: null,
  sortOrder: 0,
  rowId: crypto.randomUUID(),
});

// Create mode opens with one blank row of each, so there is somewhere to type without hunting for
// an "add" button first. Edit mode seeds nothing — the recipe's own rows are the starting point,
// and a recipe genuinely saved with no steps should not sprout one on every visit.
const ingredients = ref<IngredientRow[]>(
  props.initialRecipe
    ? props.initialRecipe.ingredients.map((ingredient) => ({
        ...ingredient,
        rowId: crypto.randomUUID(),
      }))
    : [blankIngredient()],
);

const steps = ref<StepRow[]>(
  props.initialRecipe
    ? props.initialRecipe.steps.map((step) => ({ ...step, rowId: crypto.randomUUID() }))
    : [blankStep()],
);

const addIngredient = () => ingredients.value.push(blankIngredient());
const removeIngredient = (index: number) => ingredients.value.splice(index, 1);
const moveIngredient = (index: number, offset: -1 | 1) => moveItem(ingredients.value, index, offset);

const addStep = () => steps.value.push(blankStep());
const removeStep = (index: number) => steps.value.splice(index, 1);
const moveStep = (index: number, offset: -1 | 1) => moveItem(steps.value, index, offset);

/** Section titles already used in this recipe, so the next row can reuse one instead of retyping it. */
const sectionSuggestions = computed(() => [
  ...new Set(
    ingredients.value
      .map((ingredient) => ingredient.sectionTitle?.trim())
      .filter((section): section is string => !!section),
  ),
]);

// Shown as a placeholder, never written into the field. Populating the input would make a derived
// total indistinguishable from an explicit one, and clearing it would look like data loss.
const derivedTotalTime = computed(() =>
  effectiveTotalTimeMinutes(null, prepTimeMinutes.value, cookTimeMinutes.value),
);

const totalTimeHint = computed(() =>
  derivedTotalTime.value == null
    ? 'Leave blank to use prep + cook time'
    : `Calculated: ${derivedTotalTime.value} min — enter a value to override`,
);

const buildPayload = (): UpsertRecipe => ({
  title: title.value ?? '',
  summary: summary.value,
  yieldText: yieldText.value,
  servings: servings.value,
  prepTimeMinutes: prepTimeMinutes.value,
  cookTimeMinutes: cookTimeMinutes.value,
  totalTimeMinutes: totalTimeMinutes.value,
  accessScope: accessScope.value,
  ingredients: ingredients.value
    .filter((ingredient) => !isBlankIngredientRow(ingredient))
    .map((ingredient, index) => ({
      ingredientText: ingredient.ingredientText,
      measureText: ingredient.measureText,
      sectionTitle: ingredient.sectionTitle,
      preparationText: ingredient.preparationText,
      isOptional: ingredient.isOptional,
      sortOrder: index,
    })),
  steps: steps.value
    .filter((step) => !isBlankStepRow(step))
    .map((step, index) => ({
      instructionText: step.instructionText,
      title: step.title,
      durationMinutes: step.durationMinutes,
      sortOrder: index,
    })),
});

// The dirty check compares snapshots of the *payload*, reusing the builder above deliberately: a
// guard with its own idea of what the form contains drifts from what actually gets saved.
// The baseline is taken after seeding, so the seeded blank rows do not count as an edit.
const baseline = ref(JSON.stringify(buildPayload()));
const isArmed = ref(true);
const isDirty = () => isArmed.value && JSON.stringify(buildPayload()) !== baseline.value;

const warnOnUnload = (event: BeforeUnloadEvent) => {
  if (!isDirty()) return;
  event.preventDefault();
};

// onBeforeRouteLeave never fires for a tab close or a reload, so the browser-level guard is needed
// as well as the router one.
onMounted(() => window.addEventListener('beforeunload', warnOnUnload));
onBeforeUnmount(() => window.removeEventListener('beforeunload', warnOnUnload));

onBeforeRouteLeave(() => {
  if (!isDirty()) return true;

  return window.confirm('You have unsaved changes. Leave without saving?');
});

const bannerError = ref<string | null>(null);
const titleConflictError = ref<string | null>(null);

/**
 * The agreed split: the client validates, the server backstops. Most failures are a banner, but a
 * 409 is a duplicate title and belongs on the title field — as a banner it reads as "try again",
 * and retrying an unchanged title can never succeed.
 */
const reportFailure = (error: unknown) => {
  if (error instanceof ApiError && error.isConflict) {
    titleConflictError.value = error.detail ?? 'You already have a recipe with this name.';
    return;
  }

  bannerError.value =
    error instanceof ApiError
      ? error.userFacingMessage('Failed to save recipe. Please try again.')
      : 'Failed to save recipe. Please try again.';
};

const onSubmit = async () => {
  bannerError.value = null;
  titleConflictError.value = null;

  const recipe = buildPayload();

  try {
    const saved = props.initialRecipe
      ? await updateRecipe({ recipeId: String(props.initialRecipe.id), recipe })
      : await createRecipe(recipe);

    // Disarmed before navigating: leaving it armed prompts the user to discard the changes they
    // have just successfully saved.
    isArmed.value = false;
    await router.push(`/recipe/${saved.id}`);
  } catch (error) {
    reportFailure(error);
  }
};

const onCancel = async () => {
  await router.push(props.initialRecipe ? `/recipe/${props.initialRecipe.id}` : '/recipes');
};

// The title field clears its server-side conflict as soon as the name changes, so the message never
// outlives the value it was about.
const onTitleInput = () => {
  titleConflictError.value = null;
};
</script>

<template>
  <q-form greedy class="q-gutter-md" @submit="onSubmit">
    <q-banner v-if="bannerError" class="bg-negative text-white">
      {{ bannerError }}
    </q-banner>
    <recipe-name-field
      v-model="title"
      :server-error="titleConflictError"
      @update:model-value="onTitleInput"
    />
    <recipe-visibility-field v-model="accessScope" />
    <text-field
      v-model="summary"
      type="textarea"
      label="Summary"
      hint="A short description of the recipe"
    />
    <text-field v-model="yieldText" label="Yield" hint="e.g. One 9-inch cake" />
    <number-field
      v-model="servings"
      label="Servings"
      :min="0"
      :step="1"
      :rules="nonNegativeIntegerRules"
    />
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
      :hint="totalTimeHint"
      :placeholder="derivedTotalTime == null ? undefined : String(derivedTotalTime)"
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
      :section-suggestions="sectionSuggestions"
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

    <div class="row q-gutter-sm items-center">
      <q-btn :label="submitLabel" type="submit" color="primary" :loading="isPending" />
      <q-btn label="Cancel" flat type="button" @click="onCancel" />
    </div>
  </q-form>
</template>

<style scoped></style>
