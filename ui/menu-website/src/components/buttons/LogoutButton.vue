<script setup lang="ts">
import { useAuth } from '@/services/auth';
import HeaderButton from '@/components/generic/header/HeaderButton.vue';

const { logout } = useAuth();
</script>

<template>
  <header-button label="Logout" @click="logout" />
</template>
