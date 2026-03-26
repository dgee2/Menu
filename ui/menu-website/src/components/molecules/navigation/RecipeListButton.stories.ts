import { expect, userEvent, within, waitFor } from 'storybook/test';
import RecipeListButton from './RecipeListButton.vue';
import preview from '@storybook-config/preview';
import { router } from '@storybook-config/router';

const meta = preview.meta({
  title: 'Molecules/Navigation/RecipeListButton',
  component: RecipeListButton,
  tags: ['autodocs'],
});

export const Default = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const button = canvas.getByRole('button', { name: 'Recipes' });

    await userEvent.click(button);
    await router.isReady();
    await waitFor(() => expect(router.currentRoute.value.path).toBe('/recipes'));
  },
});

