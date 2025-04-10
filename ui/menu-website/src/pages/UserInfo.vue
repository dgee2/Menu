<script setup lang="ts">
import { useAuth0 } from '@auth0/auth0-vue';

const { isAuthenticated, user } = useAuth0();
</script>

<template>
  <code v-if="isAuthenticated">
    {{ user }}
  </code>
</template>

<style scoped></style>
