<script setup lang="ts">
import { computed, ref } from 'vue';
import type { QSelect } from 'quasar';

/**
 * A free-text field that suggests values already in use, rather than restricting to them.
 *
 * Distinct from `select-field`: there the options are the permitted values, here they are a
 * shortcut. Anything typed is accepted, which is what section titles need — the first row to use a
 * new section has to be able to invent it.
 */
const value = defineModel<string | null>();

const props = withDefaults(
  defineProps<{
    label: string;
    hint?: string;
    /** Values to suggest. Typically the ones already used elsewhere in the same form. */
    suggestions?: string[];
  }>(),
  { hint: undefined, suggestions: () => [] },
);

const filtered = ref<string[]>([]);

type FilterFn = InstanceType<typeof QSelect>['$props']['onFilter'];

const onFilter: FilterFn = (needle, update) => {
  update(() => {
    const search = needle.toLocaleLowerCase();
    filtered.value = props.suggestions.filter((s) => s.toLocaleLowerCase().includes(search));
  });
};

// QSelect emits null when cleared and a string otherwise; normalise so the model is never
// undefined, which would read as "unchanged" to the dirty check.
const selected = computed<string | null>({
  get: () => value.value ?? null,
  set: (next) => {
    value.value = next?.trim() ? next : null;
  },
});
</script>

<template>
  <q-select
    v-model="selected"
    use-input
    fill-input
    hide-selected
    hide-dropdown-icon
    clearable
    new-value-mode="add-unique"
    :input-debounce="0"
    :options="filtered"
    :label="label"
    :hint="hint"
    @filter="onFilter"
  >
    <template #no-option>
      <q-item>
        <q-item-section class="text-grey">Type to add a new one</q-item-section>
      </q-item>
    </template>
  </q-select>
</template>

<style scoped></style>
