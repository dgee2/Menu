<script setup lang="ts">
import { useAuth } from '../../services/auth';

const { login } = useAuth();
</script>

<template>
  <q-btn flat dense label="Login" @click="login" />
</template>
