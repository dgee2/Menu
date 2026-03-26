import preview from '@storybook-config/preview';
import { expect, within } from 'storybook/test';
import ErrorNotFound from './ErrorNotFound.vue';

const meta = preview.meta({
  title: 'Pages/ErrorNotFound',
  component: ErrorNotFound,
  tags: ['autodocs'],
});

export const Default = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('404')).toBeInTheDocument();
    await expect(canvas.getByRole('link', { name: 'Go Home' })).toBeInTheDocument();
  },
});
