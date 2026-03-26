import { expect, userEvent, within, waitFor } from 'storybook/test';
import NewRecipeHeaderButton from './NewRecipeHeaderButton.vue';
import preview from '@storybook-config/preview';
import { router } from '@storybook-config/router';

const meta = preview.meta({
  title: 'Molecules/Navigation/NewRecipeHeaderButton',
  component: NewRecipeHeaderButton,
  tags: ['autodocs'],
});

export const Default = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const button = canvas.getByRole('button');

    await userEvent.click(button);
    await router.isReady();
    await waitFor(() => expect(router.currentRoute.value.path).toBe('/new-recipe'));
  },
});

