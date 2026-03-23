import preview from '@storybook-config/preview';
import SearchableSelectField from './select-field.vue';
import { expect, fn, within } from 'storybook/test';

const meta = preview.meta({
  title: 'Atoms/Form/SelectField',
  tags: ['autodocs'],
  args: {
    'onUpdate:modelValue': fn(),
    options: [
      { label: 'Flour', value: 1 },
      { label: 'Sugar', value: 2 },
      { label: 'Eggs', value: 3 },
      { label: 'Butter', value: 4 },
      { label: 'Milk', value: 5 },
    ],
    label: 'Recipe Ingredient',
  },

  component: SearchableSelectField,
});

export const Default = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('combobox', { name: 'Recipe Ingredient' })).toBeInTheDocument();
  },
});

export const DefaultNoResultsText = meta.story({
  args: {
    options: [],
  },
});

export const CustomNoResultsText = meta.story({
  args: {
    noResultsText: 'No ingredients found',
    options: [],
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('combobox', { name: 'Recipe Ingredient' })).toBeInTheDocument();
  },
});

export const Hint = meta.story({
  args: {
    hint: 'Select an ingredient for your recipe',
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('Select an ingredient for your recipe')).toBeInTheDocument();
  },
});

export const Searchable = meta.story({
  args: {
    searchable: true,
  },
});

export const Clearable = meta.story({
  args: {
    clearable: true,
    modelValue: { label: 'Eggs', value: 3 },
  },
});
