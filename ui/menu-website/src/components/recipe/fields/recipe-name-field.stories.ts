import RecipeNameField from './recipe-name-field.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { fn } from 'storybook/test';

const meta = {
  title: 'Recipe/Fields/RecipeNameField',
  component: RecipeNameField,
  tags: ['autodocs'],
  args: {
    'onUpdate:modelValue': fn(),
  },
} satisfies Meta<typeof RecipeNameField>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {};

export const PreFilledValue: Story = {
  args: {
    modelValue: 'Chocolate Cake',
  },
};
