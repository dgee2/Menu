import TextField from './text-field.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, fn, userEvent, within } from 'storybook/test';

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
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);
    const input = canvas.getByRole('textbox', { name: 'Text Field' });

    await userEvent.type(input, 'hello');
    await expect(args['onUpdate:modelValue']).toHaveBeenCalled();
  },
};

export const Hint: Story = {
  args: {
    hint: 'This is a hint for the text field.',
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('This is a hint for the text field.')).toBeInTheDocument();
  },
};
