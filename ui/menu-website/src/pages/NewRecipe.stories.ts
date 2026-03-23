import { expect, within } from 'storybook/test';
import preview, { withPageLayout } from '@storybook-config/preview';
import NewRecipe from './NewRecipe.vue';

const meta = preview.meta({
  title: 'Pages/NewRecipe',
  component: NewRecipe,
  tags: ['autodocs'],
  decorators: [withPageLayout],
});

export const Default = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByLabelText('Name')).toBeInTheDocument();
  },
});
