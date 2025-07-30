<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated>
      <q-toolbar>
        <q-toolbar-title> Menu</q-toolbar-title>
        <new-recipe-header-button v-if="isAuthenticated" />
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
import { useAuth } from '@/services/auth';
import LoginButton from '@/components/buttons/LoginButton.vue';
import ProfileButton from '@/components/buttons/ProfileButton.vue';
import NewRecipeHeaderButton from '@/components/buttons/NewRecipeHeaderButton.vue';

const { isAuthenticated } = useAuth();
</script>
