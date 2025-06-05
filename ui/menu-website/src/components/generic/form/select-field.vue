<template>
  <q-select
    v-model="model"
    :use-input="searchable"
    :hide-selected="searchable"
    :fill-input="searchable"
    :input-debounce="searchable ? 0 : undefined"
    :options="searchable ? internalOptions : options"
    :autocomplete="searchable ? '' : undefined"
    :label="label"
    :hint="hint"
    :clearable="clearable"
    @filter="filterFn"
  >
    <!-- Slot to display when no results match -->
    <template #no-option>
      <q-item>
        <q-item-section class="text-grey">{{ noResultsText }}</q-item-section>
      </q-item>
    </template>
  </q-select>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { QSelect } from 'quasar';

type Option = {
  label: string;
  value: string | number;
};
const model = defineModel<Option>();
const props = withDefaults(
  defineProps<{
    options: Option[];
    noResultsText?: string;
    label: string;
    hint?: string;
    searchable?: boolean;
    clearable?: boolean;
  }>(),
  {
    noResultsText: 'No results',
    hint: undefined,
    searchable: false,
    clearable: false,
  },
);

type FilterFn = InstanceType<typeof QSelect>['$props']['onFilter'];

// The options displayed in the QSelect match the original list by default
const internalOptions = ref<Option[]>([]);

// The filter function is invoked whenever the user types in the input
const filterFn: FilterFn = (val, update) => {
  update(() => {
    const needle = val.toLocaleLowerCase();
    internalOptions.value = props.options.filter((v) =>
      v.label.toLocaleLowerCase().includes(needle),
    );
  });
};
</script>
