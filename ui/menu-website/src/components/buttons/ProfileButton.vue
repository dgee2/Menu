<script setup lang="ts">
import { useRouter } from 'vue-router';
import { computed } from 'vue';
import { useAuth } from '@/services/auth';

const { user } = useAuth();
const router = useRouter();
const goToProfile = async () => {
  await router.push('/profile');
};

const userInitials = computed(() => {
  if (user.value?.nickname) {
    const initials = user.value.nickname
      .split(' ')
      .map((name) => name.charAt(0))
      .join('')
      .slice(0, 2);
    return initials.toUpperCase();
  }
  return '';
});
</script>

<template>
  <q-btn flat round @click="goToProfile">
    <q-avatar>
      <q-img v-if="user?.picture" :src="user.picture!" />
      <span v-else> {{ userInitials }} </span>
    </q-avatar>
  </q-btn>
</template>
