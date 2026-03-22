import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, userEvent, within, waitFor } from 'storybook/test';
import RecipeListButton from './RecipeListButton.vue';
import { router } from '../../../.storybook/preview';

const meta = {
  title: 'Buttons/RecipeListButton',
  component: RecipeListButton,
  tags: ['autodocs'],
} satisfies Meta<typeof RecipeListButton>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const button = canvas.getByRole('button', { name: 'Recipes' });

    await userEvent.click(button);

    await waitFor(() => {
      void expect(router.currentRoute.value.path).toBe('/recipes');
    });
  },
};


