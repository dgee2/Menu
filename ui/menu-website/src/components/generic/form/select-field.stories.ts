import SearchableSelectField from './select-field.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, fn, within } from 'storybook/test';

const meta = {
  title: 'Generic/Form/SelectField',
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
} satisfies Meta<typeof SearchableSelectField>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('combobox', { name: 'Recipe Ingredient' })).toBeInTheDocument();
  },
};

export const DefaultNoResultsText: Story = {
  args: {
    options: [],
  },
};

export const CustomNoResultsText: Story = {
  args: {
    noResultsText: 'No ingredients found',
    options: [],
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('combobox', { name: 'Recipe Ingredient' })).toBeInTheDocument();
  },
};

export const Hint: Story = {
  args: {
    hint: 'Select an ingredient for your recipe',
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('Select an ingredient for your recipe')).toBeInTheDocument();
  },
};

export const Searchable: Story = {
  args: {
    searchable: true,
  },
};

export const Clearable: Story = {
  args: {
    clearable: true,
    modelValue: { label: 'Eggs', value: 3 },
  },
};
