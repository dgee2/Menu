<template>
  <q-page class="q-pa-md">
    <div v-if="recipe">
      <div class="row items-center q-gutter-sm">
        <h1 class="q-my-none">{{ recipe.title }}</h1>
        <q-badge :color="recipe.accessScope === 'Private' ? 'grey' : 'primary'">
          {{ recipe.accessScope }}
        </q-badge>
      </div>

      <p v-if="recipe.summary">{{ recipe.summary }}</p>

      <div class="row q-gutter-md text-caption text-grey-8">
        <div v-if="recipe.servings != null">Servings: {{ recipe.servings }}</div>
        <div v-if="recipe.yieldText">Yield: {{ recipe.yieldText }}</div>
        <div v-if="recipe.prepTimeMinutes != null">Prep: {{ recipe.prepTimeMinutes }} min</div>
        <div v-if="recipe.cookTimeMinutes != null">Cook: {{ recipe.cookTimeMinutes }} min</div>
        <div v-if="recipe.totalTimeMinutes != null">Total: {{ recipe.totalTimeMinutes }} min</div>
      </div>

      <h2>Ingredients</h2>
      <div v-if="ingredientSections.length === 0">No ingredients.</div>
      <div v-for="section in ingredientSections" :key="section.title ?? ''">
        <h3 v-if="section.title">{{ section.title }}</h3>
        <q-list bordered separator>
          <q-item
            v-for="ingredient in section.ingredients"
            :key="ingredient.sortOrder"
          >
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
    </div>
    <div v-else-if="isLoading">Loading recipe...</div>
    <div v-else>Recipe not found.</div>
  </q-page>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRecipeService } from '@/services/recipe-service';
import type { RecipeIngredientItem } from '@/services/recipe-api';

const props = defineProps<{
  recipeId: string;
}>();

const { useRecipe } = useRecipeService();

const recipeId = computed(() => props.recipeId);
const { data: recipe, isLoading } = useRecipe(recipeId);

const ingredientSections = computed(() => {
  const ingredients = recipe.value?.ingredients ?? [];
  const sections: { title: string | null; ingredients: RecipeIngredientItem[] }[] = [];

  for (const ingredient of ingredients) {
    const title = ingredient.sectionTitle ?? null;
    let section = sections.find((s) => s.title === title);
    if (!section) {
      section = { title, ingredients: [] };
      sections.push(section);
    }
    section.ingredients.push(ingredient);
  }

  return sections;
});
</script>

<style scoped></style>
