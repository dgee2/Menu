<script setup lang="ts">
import AppHeader from '@/components/organisms/navigation/app-header.vue';
</script>

<template>
  <q-layout view="lHh Lpr lFf">
    <app-header />

    <q-page-container>
      <slot />
    </q-page-container>
  </q-layout>
</template>
