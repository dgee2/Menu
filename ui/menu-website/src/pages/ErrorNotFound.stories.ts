import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, within } from 'storybook/test';
import ErrorNotFound from './ErrorNotFound.vue';

const meta = {
  title: 'Pages/ErrorNotFound',
  component: ErrorNotFound,
  tags: ['autodocs'],
} satisfies Meta<typeof ErrorNotFound>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('404')).toBeInTheDocument();
    await expect(canvas.getByRole('link', { name: 'Go Home' })).toBeInTheDocument();
  },
};

