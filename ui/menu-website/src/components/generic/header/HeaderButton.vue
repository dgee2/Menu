<script setup lang="ts">
const emit = defineEmits<{
  click: []
}>()
defineProps<{
  label: string
}>()
</script>

<template>
  <q-btn flat dense @click="emit('click')" :label="label" />
</template>

<style scoped></style>
