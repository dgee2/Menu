<script setup lang="ts">
import { computed } from 'vue';
import TextField from '@/components/atoms/form/text-field.vue';
import ComboboxField from '@/components/atoms/form/combobox-field.vue';
import { requiredTextUnlessRowIsBlank } from '@/services/form-rules';
import { isBlankIngredientRow } from '@/services/recipe-rows';

const ingredientText = defineModel<string | null>('ingredientText');
const measureText = defineModel<string | null>('measureText');
const sectionTitle = defineModel<string | null>('sectionTitle');
const preparationText = defineModel<string | null>('preparationText');
const isOptional = defineModel<boolean>('isOptional', { default: false });

withDefaults(
  defineProps<{
    canMoveUp: boolean;
    canMoveDown: boolean;
    /** Section titles already used in this recipe, offered as suggestions. */
    sectionSuggestions?: string[];
  }>(),
  { sectionSuggestions: () => [] },
);

defineEmits<{
  remove: [];
  moveUp: [];
  moveDown: [];
}>();

// An untouched row is valid and gets dropped from the payload, so a seeded blank row neither
// blocks the form nor saves an empty ingredient.
const rowIsBlank = computed(() =>
  isBlankIngredientRow({
    ingredientText: ingredientText.value ?? undefined,
    measureText: measureText.value ?? undefined,
    preparationText: preparationText.value,
    sectionTitle: sectionTitle.value,
    isOptional: isOptional.value,
  }),
);

const isBlank = () => rowIsBlank.value;

const ingredientTextRules = [requiredTextUnlessRowIsBlank('Ingredient is required', isBlank)];
const measureTextRules = [requiredTextUnlessRowIsBlank('Measure is required', isBlank)];
</script>

<template>
  <div class="row q-col-gutter-sm items-start">
    <div class="col-12 col-sm-3">
      <text-field v-model="measureText" label="Measure" :rules="measureTextRules" />
    </div>
    <div class="col-12 col-sm-4">
      <text-field v-model="ingredientText" label="Ingredient" :rules="ingredientTextRules" />
    </div>
    <div class="col-12 col-sm-3">
      <text-field v-model="preparationText" label="Preparation" hint="e.g. diced, sifted" />
    </div>
    <div class="col-12 col-sm-2">
      <combobox-field
        v-model="sectionTitle"
        label="Section"
        hint="e.g. For the sauce"
        :suggestions="sectionSuggestions"
      />
    </div>
    <div class="col-12 row items-center q-gutter-sm">
      <q-toggle v-model="isOptional" label="Optional" />
      <q-btn
        flat
        dense
        round
        icon="arrow_upward"
        :disable="!canMoveUp"
        aria-label="Move ingredient up"
        @click="$emit('moveUp')"
      />
      <q-btn
        flat
        dense
        round
        icon="arrow_downward"
        :disable="!canMoveDown"
        aria-label="Move ingredient down"
        @click="$emit('moveDown')"
      />
      <q-btn
        flat
        dense
        round
        icon="delete"
        color="negative"
        aria-label="Remove ingredient"
        @click="$emit('remove')"
      />
    </div>
  </div>
</template>

<style scoped></style>
