<script setup lang="ts">
import { useAuth } from '@/services/auth';
import LoginButton from '@/components/molecules/navigation/LoginButton.vue';
import LogoutButton from '@/components/molecules/navigation/LogoutButton.vue';
import NewRecipeHeaderButton from '@/components/molecules/navigation/NewRecipeHeaderButton.vue';
import ProfileButton from '@/components/molecules/navigation/ProfileButton.vue';
import RecipeListButton from '@/components/molecules/navigation/RecipeListButton.vue';

const { isAuthenticated } = useAuth();
</script>

<template>
  <q-header elevated>
    <q-toolbar>
      <q-toolbar-title> Menu</q-toolbar-title>
      <recipe-list-button />
      <new-recipe-header-button v-if="isAuthenticated" />
      <profile-button v-if="isAuthenticated" />
      <logout-button v-if="isAuthenticated" />
      <login-button v-else />
    </q-toolbar>
  </q-header>
</template>
