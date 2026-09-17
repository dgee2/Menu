<script setup lang="ts">
import { useAuth } from '@/services/auth';
import { computed } from 'vue';

const { isAuthenticated, user } = useAuth();
const displayName = computed(
  () => user.value?.name || user.value?.nickname || 'Authenticated user',
);
const identity = computed(() => user.value?.email || displayName.value);
</script>

<template>
  <q-page class="items-center justify-evenly">
    <section v-if="isAuthenticated && user" aria-labelledby="profile-heading">
      <h1 id="profile-heading">Profile</h1>
      <q-img v-if="user.picture" :src="user.picture" :alt="`${displayName} profile picture`" />
      <p data-testid="profile-identity">{{ identity }}</p>
    </section>
    <p v-else>Profile unavailable.</p>
  </q-page>
</template>
