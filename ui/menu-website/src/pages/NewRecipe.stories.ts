import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, within } from 'storybook/test';
import { withPageLayout } from '../../.storybook/preview';
import NewRecipe from './NewRecipe.vue';

const meta = {
  title: 'Pages/NewRecipe',
  component: NewRecipe,
  tags: ['autodocs'],
  decorators: [withPageLayout],
} satisfies Meta<typeof NewRecipe>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByLabelText('Name')).toBeInTheDocument();
  },
};

