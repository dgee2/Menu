<template>
  <div class="q-pa-md">
    <div class="q-gutter-md row">
      <q-select
        v-model="model"
        filled
        use-input
        hide-selected
        fill-input
        input-debounce="0"
        :options="options"
        placeholder="Select Ingredient..."
        autocomplete=""
        @filter="filterFn"
      >
        <!-- Slot to display when no results match -->
        <template #no-option>
          <q-item>
            <q-item-section class="text-grey">No results</q-item-section>
          </q-item>
        </template>
      </q-select>
    </div>
  </div>
</template>

<script setup lang="ts">
// The model will hold the currently selected value(s)
import { ref } from 'vue';
import type { QSelect } from 'quasar';

type FilterFn = InstanceType<typeof QSelect>['$props']['onFilter'];

const model = ref<string[]>([]);

// An array of string options, you could also fetch these from an API
const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle'];

// The options displayed in the QSelect match the original list by default
const options = ref([...stringOptions]);

// The filter function is invoked whenever the user types in the input
const filterFn: FilterFn = (val, update) => {
  update(() => {
    const needle = val.toLocaleLowerCase();
    options.value = stringOptions.filter((v) => v.toLocaleLowerCase().includes(needle));
  });
};
</script>
