<template>
  <q-page padding>
    <div v-if="isAuthenticated">
      <div class="user-name">{{ user?.name ?? 'Unknown user' }}</div>
      <q-img v-if="user?.picture" :src="user.picture" />
    </div>
    <div v-else>
      Not authenticated
    </div>
  </q-page>
</template>

<script setup lang="ts">
interface User {
  picture?: string | null;
  name?: string | null;
  [key: string]: unknown;
}

defineProps<{
  isAuthenticated: boolean;
  user: User | null;
  auth0: unknown;
}>();
</script>
