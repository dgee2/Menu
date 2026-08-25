<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useRecipeService } from '@/services/recipe-service';
import { ApiError, userFacingMessage } from '@/services/api-error';
import { recipeAccessScopeBadgeLabels } from '@/services/recipe-labels';
import type { RecipeIngredientItem } from '@/services/recipe-api';

const props = defineProps<{
  recipeId: string;
}>();

const router = useRouter();
const { useRecipe, useDeleteRecipe } = useRecipeService();

const recipeId = computed(() => props.recipeId);
const { data: recipe, isLoading, isError, error } = useRecipe(recipeId);
const { mutateAsync: deleteRecipe, isPending: isDeleting } = useDeleteRecipe();

// A 404 means this recipe is not there (or not ours to see). Anything else — a dropped connection,
// a 500 — is a failure to load, and telling the user "not found" would send them looking for a
// recipe that is probably fine.
const isNotFound = computed(() => error.value instanceof ApiError && error.value.status === 404);
const loadErrorMessage = computed(() =>
  userFacingMessage(error.value, 'Something went wrong loading this recipe.'),
);

const accessScopeLabel = computed(() =>
  recipe.value ? recipeAccessScopeBadgeLabels[recipe.value.accessScope] : '',
);

/**
 * Ingredients grouped by contiguous runs of the same section title.
 *
 * Deliberately not a lookup by title: matching an earlier section would teleport a later row up
 * next to the first occurrence, silently reordering the recipe. A section that genuinely appears
 * twice renders twice, which is at least what the author saved.
 */
const ingredientSections = computed(() => {
  const sections: { key: string; title: string | null; ingredients: RecipeIngredientItem[] }[] = [];

  for (const ingredient of recipe.value?.ingredients ?? []) {
    const title = ingredient.sectionTitle ?? null;
    const current = sections.at(-1);

    if (!current || current.title !== title) {
      sections.push({ key: `${sections.length}-${title ?? ''}`, title, ingredients: [ingredient] });
      continue;
    }

    current.ingredients.push(ingredient);
  }

  return sections;
});

const confirmingDelete = ref(false);
const deleteError = ref<string | null>(null);

const onDelete = async () => {
  deleteError.value = null;
  try {
    await deleteRecipe(props.recipeId);
    confirmingDelete.value = false;
    await router.push('/recipes');
  } catch (deleteFailure) {
    // Closed here too: the banner renders on the page behind the dialog, so leaving the dialog open
    // would hide the only feedback the user gets.
    confirmingDelete.value = false;
    deleteError.value = userFacingMessage(
      deleteFailure,
      'Failed to delete recipe. Please try again.',
    );
  }
};
</script>

<template>
  <q-page class="q-pa-md">
    <div v-if="recipe">
      <div class="row items-center q-gutter-sm">
        <h1 class="q-my-none">{{ recipe.title }}</h1>
        <q-badge :color="recipe.accessScope === 'Private' ? 'grey' : 'primary'">
          {{ accessScopeLabel }}
        </q-badge>
        <q-space />
        <!-- Driven by the server's own access evaluation, not by comparing ids here: ownership
             stops meaning editability as soon as recipes can be shared. -->
        <q-btn
          v-if="recipe.canEdit"
          label="Edit"
          icon="edit"
          flat
          :to="`/recipe/${recipe.id}/edit`"
        />
        <q-btn
          v-if="recipe.canDelete"
          label="Delete"
          icon="delete"
          flat
          color="negative"
          @click="confirmingDelete = true"
        />
      </div>

      <q-banner v-if="deleteError" class="bg-negative text-white q-mt-sm">
        {{ deleteError }}
      </q-banner>

      <p v-if="recipe.summary">{{ recipe.summary }}</p>

      <div class="row q-gutter-md text-caption text-grey-8">
        <div v-if="recipe.servings != null">Servings: {{ recipe.servings }}</div>
        <div v-if="recipe.yieldText">Yield: {{ recipe.yieldText }}</div>
        <div v-if="recipe.prepTimeMinutes != null">Prep: {{ recipe.prepTimeMinutes }} min</div>
        <div v-if="recipe.cookTimeMinutes != null">Cook: {{ recipe.cookTimeMinutes }} min</div>
        <!-- The server-computed total, so an explicit override and a derived figure render the
             same way and neither page has to know the derivation. -->
        <div v-if="recipe.effectiveTotalTimeMinutes != null">
          Total: {{ recipe.effectiveTotalTimeMinutes }} min
        </div>
      </div>

      <h2>Ingredients</h2>
      <div v-if="ingredientSections.length === 0">No ingredients.</div>
      <div v-for="section in ingredientSections" :key="section.key">
        <h3 v-if="section.title">{{ section.title }}</h3>
        <q-list bordered separator>
          <q-item v-for="ingredient in section.ingredients" :key="ingredient.sortOrder">
            <q-item-section>
              {{ ingredient.measureText }} {{ ingredient.ingredientText }}
              <span v-if="ingredient.isOptional" class="text-grey-6">(optional)</span>
            </q-item-section>
          </q-item>
        </q-list>
      </div>

      <h2>Steps</h2>
      <div v-if="recipe.steps.length === 0">No steps.</div>
      <q-list v-else bordered separator>
        <q-item v-for="(step, index) in recipe.steps" :key="step.sortOrder">
          <q-item-section avatar>{{ index + 1 }}</q-item-section>
          <q-item-section>
            <div v-if="step.title" class="text-weight-bold">{{ step.title }}</div>
            <div>{{ step.instructionText }}</div>
            <div v-if="step.durationMinutes != null" class="text-caption text-grey-8">
              {{ step.durationMinutes }} min
            </div>
          </q-item-section>
        </q-item>
      </q-list>

      <q-dialog v-model="confirmingDelete">
        <q-card>
          <q-card-section class="text-h6">Delete this recipe?</q-card-section>
          <q-card-section>
            "{{ recipe.title }}" will be removed, along with its ingredients and steps.
          </q-card-section>
          <q-card-actions align="right">
            <q-btn v-close-popup flat label="Cancel" />
            <q-btn flat color="negative" label="Delete" :loading="isDeleting" @click="onDelete" />
          </q-card-actions>
        </q-card>
      </q-dialog>
    </div>
    <div v-else-if="isLoading">Loading recipe...</div>
    <div v-else-if="isError && !isNotFound">
      <q-banner class="bg-negative text-white">{{ loadErrorMessage }}</q-banner>
    </div>
    <div v-else>Recipe not found.</div>
  </q-page>
</template>

<style scoped></style>
