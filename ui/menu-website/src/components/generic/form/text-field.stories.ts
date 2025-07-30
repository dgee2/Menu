import TextField from './text-field.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { fn } from 'storybook/test';

const meta = {
  title: 'Generic/Form/TextField',
  component: TextField,
  tags: ['autodocs'],
  args: {
    label: 'Text Field',
    'onUpdate:modelValue': fn(),
  },
} satisfies Meta<typeof TextField>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {},
};

export const Hint: Story = {
  args: {
    hint: 'This is a hint for the text field.',
  },
};
