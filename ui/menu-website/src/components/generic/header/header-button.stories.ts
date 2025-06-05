import HeaderButton from '@/components/generic/header/header-button.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { fn } from 'storybook/test';

const meta = {
  title: 'Generic/HeaderButton',
  component: HeaderButton,
  tags: ['autodocs'],
  args: {
    label: 'Button',
    onClick: fn(),
  },
} satisfies Meta<typeof HeaderButton>;

export default meta;
type Story = StoryObj<typeof meta>;

export const ChangeLabel: Story = {
  args: {
    label: 'Click Me',
  },
};
