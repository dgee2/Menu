<script setup lang="ts">
const value = defineModel<string>();
defineProps<{
  label: string;
  hint?: string;
}>();
</script>

<template>
  <q-input v-model="value" :label="label" :hint="hint" />
</template>

<style scoped></style>
