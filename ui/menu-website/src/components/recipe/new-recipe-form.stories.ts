import NewRecipeForm from './new-recipe-form.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';

const meta = {
  title: 'Recipe/NewRecipeForm',
  component: NewRecipeForm,
  tags: ['autodocs'],
  args: {},
} satisfies Meta<typeof NewRecipeForm>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {},
};
