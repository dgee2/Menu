<template>
  <VDataTable :headers="headers" :items="recipes" :loading="loading"></VDataTable>
</template>

<script setup lang="ts">
import { Recipe, getRecipes } from "@/services/menuApi";
import { computed, onMounted } from "vue";
import { ref } from "vue";
import { VDataTable } from "vuetify/labs/VDataTable";

type UnwrapReadonlyArrayType<A> = A extends Readonly<Array<infer I>> ? UnwrapReadonlyArrayType<I> : A
type ReadonlyDataTableHeader = UnwrapReadonlyArrayType<InstanceType<typeof VDataTable>['headers']>;

const loading = ref<boolean>(true);
const recipes = ref<Recipe[]>();
const data = computed(()=>JSON.stringify(recipes.value, null, 4));

const headers: ReadonlyDataTableHeader[] = [
  {
    title: "Id",
    align: "end",
    key: "id",
  },
  {
    title: "Name",
    key: "name",
  },
];

onMounted(async () => {
  const response = await getRecipes({});
  recipes.value = response.data;
  loading.value = false;
});
</script>

<style scoped></style>
