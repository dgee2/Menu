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
import { useAuth0 } from '@auth0/auth0-vue';
import LoginButton from 'components/LoginButton.vue';

const { isAuthenticated } = useAuth0();
</script>
