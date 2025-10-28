<script setup lang="ts">
import { useAuth } from '@/services/auth';
import { useAuth0 } from '@auth0/auth0-vue';
import UserInfoView from '@/views/UserInfoView.vue';

const { isAuthenticated, user } = useAuth();

const auth0 = useAuth0();
</script>

<template>
  <UserInfoView :is-authenticated="isAuthenticated" :user="user" :auth0="auth0" />
</template>
