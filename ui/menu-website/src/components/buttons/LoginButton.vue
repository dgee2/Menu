<script setup lang="ts">
import { useAuth } from '@/services/auth';
import HeaderButton from '@/components/generic/header/HeaderButton.vue';

const { login } = useAuth();
</script>

<template>
  <header-button label="Login" @click="login" />
</template>
