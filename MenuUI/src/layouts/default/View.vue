<template>
  <v-app-bar flat>
    <v-container class="fill-height d-flex align-center">
      <v-btn to="/">
      <v-icon icon="mdi-home"/>
    </v-btn>
      <v-btn variant="text" to="/recipes">Recipes</v-btn>

      <v-spacer></v-spacer>

      <v-responsive max-width="260">
        <v-text-field
          density="compact"
          hide-details
          variant="solo"
        ></v-text-field>
      </v-responsive>
    </v-container>
  </v-app-bar>
  <v-main>
    <v-container>
      <router-view />
    </v-container>
  </v-main>
</template>

<script lang="ts" setup>
const links = ["Dashboard", "Messages", "Profile", "Updates"];
</script>
