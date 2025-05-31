<script setup lang="ts">
const name = defineModel<string>();
</script>

<template>
  <q-input v-model="name" label="Name" hint="Recipe name" />
</template>

<style scoped></style>
