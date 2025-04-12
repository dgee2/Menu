<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated>
      <q-toolbar>
        <q-toolbar-title> Menu</q-toolbar-title>
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
import LogoutButton from 'components/LogoutButton.vue';
import { useAuth } from '../services/auth';
import LoginButton from 'components/LoginButton.vue';

const { isAuthenticated } = useAuth();
</script>
