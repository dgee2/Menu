<script setup lang="ts">
const emit = defineEmits<{
  click: [];
}>();
defineProps<{
  label: string;
}>();
</script>

<template>
  <q-btn flat dense :label="label" @click="emit('click')" />
</template>

<style scoped></style>
