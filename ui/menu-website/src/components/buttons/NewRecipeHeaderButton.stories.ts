import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, userEvent, within, waitFor } from 'storybook/test';
import NewRecipeHeaderButton from './NewRecipeHeaderButton.vue';
import { router } from '../../../.storybook/preview';

const meta = {
  title: 'Buttons/NewRecipeHeaderButton',
  component: NewRecipeHeaderButton,
  tags: ['autodocs'],
} satisfies Meta<typeof NewRecipeHeaderButton>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const button = canvas.getByRole('button');

    await userEvent.click(button);

    await waitFor(() => {
      void expect(router.currentRoute.value.path).toBe('/new-recipe');
    });
  },
};


