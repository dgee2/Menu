import HeaderButton from '@/components/generic/header/HeaderButton.vue';
import type { Meta, StoryObj } from '@storybook/vue3';
import { fn } from '@storybook/test';

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
