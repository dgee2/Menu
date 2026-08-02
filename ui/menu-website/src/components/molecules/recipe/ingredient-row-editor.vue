<script setup lang="ts">
import TextField from '@/components/atoms/form/text-field.vue';

const ingredientText = defineModel<string | null>('ingredientText');
const measureText = defineModel<string | null>('measureText');
const sectionTitle = defineModel<string | null>('sectionTitle');
const preparationText = defineModel<string | null>('preparationText');
const isOptional = defineModel<boolean>('isOptional', { default: false });

defineProps<{
  canMoveUp: boolean;
  canMoveDown: boolean;
}>();

defineEmits<{
  remove: [];
  moveUp: [];
  moveDown: [];
}>();

const ingredientTextRules = [(val: string | null) => !!val?.trim() || 'Ingredient is required'];
const measureTextRules = [(val: string | null) => !!val?.trim() || 'Measure is required'];
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
      <text-field v-model="sectionTitle" label="Section" hint="e.g. For the sauce" />
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
