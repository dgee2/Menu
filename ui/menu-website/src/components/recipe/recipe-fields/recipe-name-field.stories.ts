import RecipeNameField from './recipe-name-field.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';

const meta = {
  title: 'Recipe/Fields/RecipeNameField',
  component: RecipeNameField,
  tags: ['autodocs'],
  args: {
    name: 'Chocolate Cake',
  },
} satisfies Meta<typeof RecipeNameField>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {
    name: 'Chocolate Cake',
  },
};
