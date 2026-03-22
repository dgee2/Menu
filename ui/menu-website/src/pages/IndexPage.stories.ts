import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, within } from 'storybook/test';
import { withPageLayout } from '../../.storybook/preview';
import IndexPage from './IndexPage.vue';

const meta = {
  title: 'Pages/IndexPage',
  component: IndexPage,
  tags: ['autodocs'],
  decorators: [withPageLayout],
} satisfies Meta<typeof IndexPage>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Blank: Story = {
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('main')).toBeInTheDocument();
  },
};

