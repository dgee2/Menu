<script setup lang="ts">
import { useAuth0 } from '@auth0/auth0-vue';

const auth0 = useAuth0();

const login = () => auth0.loginWithRedirect();
</script>

<template>
  <q-btn flat dense @click="login" label="Login" />
</template>

<style scoped></style>
