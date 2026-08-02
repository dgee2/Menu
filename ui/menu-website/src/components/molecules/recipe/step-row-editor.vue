<script setup lang="ts">
import TextField from '@/components/atoms/form/text-field.vue';
import NumberField from '@/components/atoms/form/number-field.vue';

const instructionText = defineModel<string | null>('instructionText');
const title = defineModel<string | null>('title');
const durationMinutes = defineModel<number | null>('durationMinutes');

defineProps<{
  canMoveUp: boolean;
  canMoveDown: boolean;
}>();

defineEmits<{
  remove: [];
  moveUp: [];
  moveDown: [];
}>();

const instructionTextRules = [(val: string | null) => !!val?.trim() || 'Instructions are required'];
const positiveIntegerRules = [
  (val: number | null) => val == null || Number.isInteger(val) || 'Must be a whole number',
  (val: number | null) => val == null || val > 0 || 'Must be greater than 0',
];
</script>

<template>
  <div class="row q-col-gutter-sm items-start">
    <div class="col-12 col-sm-3">
      <text-field v-model="title" label="Title" hint="e.g. Preheat the oven" />
    </div>
    <div class="col-12 col-sm-6">
      <text-field
        v-model="instructionText"
        type="textarea"
        label="Instructions"
        :rules="instructionTextRules"
      />
    </div>
    <div class="col-12 col-sm-3">
      <number-field
        v-model="durationMinutes"
        label="Duration (minutes)"
        :min="1"
        :step="1"
        :rules="positiveIntegerRules"
      />
    </div>
    <div class="col-12 row items-center q-gutter-sm">
      <q-btn
        flat
        dense
        round
        icon="arrow_upward"
        :disable="!canMoveUp"
        aria-label="Move step up"
        @click="$emit('moveUp')"
      />
      <q-btn
        flat
        dense
        round
        icon="arrow_downward"
        :disable="!canMoveDown"
        aria-label="Move step down"
        @click="$emit('moveDown')"
      />
      <q-btn flat dense round icon="delete" color="negative" aria-label="Remove step" @click="$emit('remove')" />
    </div>
  </div>
</template>

<style scoped></style>
