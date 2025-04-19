<script setup lang="ts">
import { useAuth } from '../../services/auth';

const { logout } = useAuth();
</script>

<template>
  <q-btn flat dense @click="logout">Logout</q-btn>
</template>
