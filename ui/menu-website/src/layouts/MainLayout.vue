<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated>
      <q-toolbar>
        <q-toolbar-title> Menu</q-toolbar-title>
        <profile-button v-if="isAuthenticated" />
        <logout-button v-if="isAuthenticated" />
        <login-button v-else />
      </q-toolbar>
    </q-header>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import LogoutButton from '@/components/buttons/LogoutButton.vue';
import { useAuth } from '../services/auth';
import LoginButton from '@/components/buttons/LoginButton.vue';
import ProfileButton from '@/components/buttons/ProfileButton.vue';

const { isAuthenticated } = useAuth();
</script>
