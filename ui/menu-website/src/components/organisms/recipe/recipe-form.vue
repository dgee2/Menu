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

interface IngredientRow extends Omit<RecipeIngredientItem, 'sectionTitle'> {
  rowId: string;
}

interface IngredientSection {
  sectionId: string;
  title: string | null;
  rows: IngredientRow[];
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
const seedSections = (): IngredientSection[] => {
  const result: IngredientSection[] = [{ sectionId: crypto.randomUUID(), title: null, rows: [] }];
  for (const ingredient of props.initialRecipe?.ingredients ?? []) {
    const { sectionTitle, ...row } = ingredient;
    const title = sectionTitle?.trim() || null;
    let section = result.at(-1);
    // Keep contiguous runs separate: a repeated heading later in the recipe is a new container.
    if (!section || section.title !== title) {
      section = { sectionId: crypto.randomUUID(), title, rows: [] };
      result.push(section);
    }
    section.rows.push({ ...row, rowId: crypto.randomUUID() });
  }
  if (!props.initialRecipe) result[0].rows.push(blankIngredient());
  return result;
};

const sections = ref<IngredientSection[]>(seedSections());

const steps = ref<StepRow[]>(
  props.initialRecipe
    ? props.initialRecipe.steps.map((step) => ({ ...step, rowId: crypto.randomUUID() }))
    : [blankStep()],
);

const addIngredient = (section: IngredientSection) => section.rows.push(blankIngredient());
const removeIngredient = (section: IngredientSection, index: number) =>
  section.rows.splice(index, 1);
const moveIngredient = (section: IngredientSection, index: number, offset: -1 | 1) =>
  moveItem(section.rows, index, offset);
const addSection = () =>
  sections.value.push({ sectionId: crypto.randomUUID(), title: '', rows: [] });
const removeSection = (index: number) => {
  if (index < 1) return;
  const [removed] = sections.value.splice(index, 1);
  if (removed) sections.value[index - 1].rows.push(...removed.rows);
};
const moveSection = (index: number, offset: -1 | 1) => {
  if (index + offset < 1) return;
  moveItem(sections.value, index, offset);
};
const sectionOptions = computed(() =>
  sections.value.map((section, index) => ({
    label: index === 0 ? 'Unsectioned' : section.title?.trim() || `Section ${index}`,
    value: section.sectionId,
  })),
);
const moveIngredientToSection = (source: IngredientSection, rowIndex: number, targetId: string) => {
  const target = sections.value.find((section) => section.sectionId === targetId);
  if (!target || target === source) return;
  const [row] = source.rows.splice(rowIndex, 1);
  if (row) target.rows.push(row);
};

const dragged = ref<
  { kind: 'ingredient'; rowId: string } | { kind: 'section'; sectionId: string } | null
>(null);
const startIngredientDrag = (rowId: string, event: DragEvent) => {
  dragged.value = { kind: 'ingredient', rowId };
  if (event.dataTransfer) event.dataTransfer.effectAllowed = 'move';
};
const startSectionDrag = (sectionId: string, event: DragEvent) => {
  dragged.value = { kind: 'section', sectionId };
  if (event.dataTransfer) event.dataTransfer.effectAllowed = 'move';
};
const dropIngredient = (targetSection: IngredientSection, targetIndex: number) => {
  const draggedItem = dragged.value;
  if (draggedItem?.kind !== 'ingredient') return;
  const source = sections.value.find((section) =>
    section.rows.some((row) => row.rowId === draggedItem.rowId),
  );
  if (!source) return;
  const sourceIndex = source.rows.findIndex((row) => row.rowId === draggedItem.rowId);
  const [row] = source.rows.splice(sourceIndex, 1);
  if (!row) return;
  const insertIndex =
    source === targetSection && sourceIndex < targetIndex ? targetIndex - 1 : targetIndex;
  targetSection.rows.splice(insertIndex, 0, row);
  dragged.value = null;
};
const dropOnRow = (targetSection: IngredientSection, targetIndex: number, event: DragEvent) => {
  if (dragged.value?.kind !== 'ingredient') return;
  event.stopPropagation();
  dropIngredient(targetSection, targetIndex);
};
const dropOnSection = (target: IngredientSection) => {
  const draggedItem = dragged.value;
  if (draggedItem?.kind === 'ingredient') {
    dropIngredient(target, target.rows.length);
  } else if (draggedItem?.kind === 'section') {
    const sourceIndex = sections.value.findIndex(
      (section) => section.sectionId === draggedItem.sectionId,
    );
    const targetIndex = sections.value.indexOf(target);
    if (sourceIndex > 0 && targetIndex > 0 && sourceIndex !== targetIndex) {
      const [section] = sections.value.splice(sourceIndex, 1);
      if (section) sections.value.splice(targetIndex, 0, section);
    }
    dragged.value = null;
  }
};

const addStep = () => steps.value.push(blankStep());
const removeStep = (index: number) => steps.value.splice(index, 1);
const moveStep = (index: number, offset: -1 | 1) => moveItem(steps.value, index, offset);

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
  ingredients: sections.value
    .flatMap((section) =>
      section.rows
        .filter((ingredient) => !isBlankIngredientRow(ingredient))
        .map((ingredient) => ({ section, ingredient })),
    )
    .map(({ section, ingredient }, index) => ({
      ingredientText: ingredient.ingredientText,
      measureText: ingredient.measureText,
      sectionTitle: section.title?.trim() || null,
      preparationText: ingredient.preparationText,
      isOptional: ingredient.isOptional,
      // Carried through untouched: the form does not expose these, but the API accepts them and
      // UpsertRecipeIngredientsAsync replaces the whole collection, so omitting them would delete
      // any structured amounts an existing recipe already has.
      amount: ingredient.amount,
      unitText: ingredient.unitText,
      canonicalIngredientId: ingredient.canonicalIngredientId,
      canonicalUnitId: ingredient.canonicalUnitId,
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
  // preventDefault() is what the spec asks for, but Chromium and WebKit still gate the prompt on a
  // non-empty returnValue, so both are set. No current browser displays the value itself.
  event.returnValue = true;
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
    <div
      v-for="(section, sectionIndex) in sections"
      :key="section.sectionId"
      class="ingredient-section"
      :class="{ 'ingredient-section--visible q-pa-md': sections.length > 1 }"
      :data-section-id="section.sectionId"
      @dragover.prevent
      @drop.stop.prevent="dropOnSection(section)"
    >
      <div
        v-if="sectionIndex > 0"
        class="row items-center q-gutter-sm q-mb-sm"
        draggable="true"
        @dragstart.stop="startSectionDrag(section.sectionId, $event)"
        @dragend="dragged = null"
      >
        <span class="material-icons cursor-grab" aria-hidden="true">drag_indicator</span>
        <text-field v-model="section.title" class="col" label="Section heading" />
        <q-btn
          flat
          dense
          round
          icon="arrow_upward"
          aria-label="Move section up"
          :disable="sectionIndex === 1"
          @click="moveSection(sectionIndex, -1)"
        />
        <q-btn
          flat
          dense
          round
          icon="arrow_downward"
          aria-label="Move section down"
          :disable="sectionIndex === sections.length - 1"
          @click="moveSection(sectionIndex, 1)"
        />
        <q-btn
          flat
          dense
          round
          icon="delete"
          color="negative"
          aria-label="Remove section"
          @click="removeSection(sectionIndex)"
        />
      </div>
      <div v-else-if="sections.length > 1" class="text-subtitle2 q-mb-sm">
        Unsectioned ingredients
      </div>
      <div v-if="sectionIndex === 0 && sections.length > 1" class="text-caption q-mb-sm">
        Drag rows between sections or use Move ingredient to section.
      </div>
      <div
        v-for="(ingredient, index) in section.rows"
        :key="ingredient.rowId"
        class="ingredient-row"
        draggable="true"
        @dragstart.stop="startIngredientDrag(ingredient.rowId, $event)"
        @dragend="dragged = null"
        @dragover.prevent
        @drop.prevent="dropOnRow(section, index, $event)"
      >
        <span class="material-icons cursor-grab" aria-hidden="true">drag_indicator</span>
        <ingredient-row-editor
          v-model:ingredient-text="ingredient.ingredientText"
          v-model:measure-text="ingredient.measureText"
          v-model:preparation-text="ingredient.preparationText"
          v-model:is-optional="ingredient.isOptional"
          :can-move-up="index > 0"
          :can-move-down="index < section.rows.length - 1"
          @remove="removeIngredient(section, index)"
          @move-up="moveIngredient(section, index, -1)"
          @move-down="moveIngredient(section, index, 1)"
        />
        <q-select
          v-if="sections.length > 1"
          :model-value="section.sectionId"
          :options="sectionOptions"
          emit-value
          map-options
          dense
          outlined
          label="Move ingredient to section"
          class="q-mb-sm"
          @update:model-value="moveIngredientToSection(section, index, $event)"
        />
      </div>
      <q-btn label="Add ingredient" icon="add" flat @click="addIngredient(section)" />
    </div>
    <q-btn label="Add section" icon="add" flat @click="addSection" />

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

<style scoped>
.ingredient-section--visible {
  border: 1px solid var(--q-primary);
  border-radius: 8px;
}
.ingredient-row {
  cursor: grab;
}
</style>
