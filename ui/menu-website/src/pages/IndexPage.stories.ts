import { expect, within } from 'storybook/test';
import preview, { withPageLayout } from '../../.storybook/preview';
import IndexPage from './IndexPage.vue';

const meta = preview.meta({
  title: 'Pages/IndexPage',
  component: IndexPage,
  tags: ['autodocs'],
  decorators: [withPageLayout],
});

export const Blank = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('main')).toBeInTheDocument();
  },
});
