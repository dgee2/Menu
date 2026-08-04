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

export const MinAndStep = meta.story({
  args: {
    min: 0,
    step: 1,
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const input = canvas.getByRole('spinbutton', { name: 'Number Field' });

    await expect(input).toHaveAttribute('min', '0');
    await expect(input).toHaveAttribute('step', '1');
  },
});

export const ClearedValueEmitsNull = meta.story({
  args: {
    modelValue: 8,
  },
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);
    const input = canvas.getByRole('spinbutton', { name: 'Number Field' });

    await userEvent.clear(input);

    await expect(args['onUpdate:modelValue']).toHaveBeenCalledWith(null);
  },
});

const nonNegativeIntegerRules = [
  (val: number | null) => val == null || Number.isInteger(val) || 'Must be a whole number',
  (val: number | null) => val == null || val >= 0 || 'Must be 0 or greater',
];

export const RuleRejectsANegativeValue = meta.story({
  args: {
    rules: nonNegativeIntegerRules,
    modelValue: -5,
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    // Quasar validates on blur (and on model change), so focus then leave the field.
    await userEvent.click(canvas.getByRole('spinbutton', { name: 'Number Field' }));
    await userEvent.tab();

    await expect(await canvas.findByText('Must be 0 or greater')).toBeInTheDocument();
  },
});

export const RuleRejectsAFractionalValue = meta.story({
  args: {
    rules: nonNegativeIntegerRules,
    modelValue: 3.5,
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await userEvent.click(canvas.getByRole('spinbutton', { name: 'Number Field' }));
    await userEvent.tab();

    await expect(await canvas.findByText('Must be a whole number')).toBeInTheDocument();
  },
});

export const RuleAcceptsAValidValue = meta.story({
  args: {
    rules: nonNegativeIntegerRules,
    modelValue: 8,
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await userEvent.click(canvas.getByRole('spinbutton', { name: 'Number Field' }));
    await userEvent.tab();

    await expect(canvas.queryByText('Must be 0 or greater')).not.toBeInTheDocument();
    await expect(canvas.queryByText('Must be a whole number')).not.toBeInTheDocument();
  },
});
