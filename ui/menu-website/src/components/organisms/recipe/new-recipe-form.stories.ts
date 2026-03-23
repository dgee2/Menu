import NewRecipeForm from './new-recipe-form.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, within } from 'storybook/test';

const meta = {
  title: 'Organisms/Recipe/NewRecipeForm',
  component: NewRecipeForm,
  tags: ['autodocs'],
  args: {},
} satisfies Meta<typeof NewRecipeForm>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {},
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByLabelText('Name')).toBeInTheDocument();
  },
};
