<script setup lang="ts">
import { useAuth } from '../services/auth';
import { useAuth0 } from '@auth0/auth0-vue';

const { isAuthenticated, user } = useAuth();

const auth0 = useAuth0();
</script>

<template>
  <code v-if="isAuthenticated">
    {{ user }}
  </code>
  <q-img v-if="user?.picture" :src="user?.picture.toString()" />
  <code>
    {{ auth0 }}
  </code>
</template>

<style scoped></style>
