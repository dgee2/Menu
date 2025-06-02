import SearchableSelectField from './select-field.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';

const meta = {
  title: 'Generic/Form/SelectField',
  tags: ['autodocs'],
  args: {
    modelValue: undefined,
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

export const Default: Story = {};

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
};

export const Hint: Story = {
  args: {
    hint: 'Select an ingredient for your recipe',
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
