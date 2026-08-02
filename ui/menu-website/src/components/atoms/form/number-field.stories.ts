import preview from '@storybook-config/preview';
import NumberField from './number-field.vue';
import { expect, fn, userEvent, within } from 'storybook/test';

const meta = preview.meta({
  title: 'Atoms/Form/NumberField',
  component: NumberField,
  tags: ['autodocs'],
  args: {
    label: 'Number Field',
    'onUpdate:modelValue': fn(),
  },
});

export const Default = meta.story({
  args: {},
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);
    const input = canvas.getByRole('spinbutton', { name: 'Number Field' });

    await userEvent.type(input, '42');
    await expect(args['onUpdate:modelValue']).toHaveBeenCalledWith(42);
  },
});

export const Hint = meta.story({
  args: {
    hint: 'Enter a whole number.',
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('Enter a whole number.')).toBeInTheDocument();
  },
});

export const PreFilledValue = meta.story({
  args: {
    modelValue: 8,
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByDisplayValue('8')).toBeInTheDocument();
  },
});
