<script setup lang="ts">
import { useAuth0 } from '@auth0/auth0-vue';

const auth0 = useAuth0();

const logout = async () => {
  await auth0.logout({ logoutParams: { returnTo: window.location.origin } });
};
</script>

<template>
  <q-btn flat dense @click="logout">Logout</q-btn>
</template>

<style scoped></style>
