import preview from '@storybook-config/preview';
import TextField from './text-field.vue';
import { expect, fn, userEvent, within } from 'storybook/test';

const meta = preview.meta({
  title: 'Atoms/Form/TextField',
  component: TextField,
  tags: ['autodocs'],
  args: {
    label: 'Text Field',
    'onUpdate:modelValue': fn(),
  },
});

export const Default = meta.story({
  args: {},
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);
    const input = canvas.getByRole('textbox', { name: 'Text Field' });

    await userEvent.type(input, 'hello');
    await expect(args['onUpdate:modelValue']).toHaveBeenCalled();
  },
});

export const Hint = meta.story({
  args: {
    hint: 'This is a hint for the text field.',
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('This is a hint for the text field.')).toBeInTheDocument();
  },
});

export const Textarea = meta.story({
  args: {
    type: 'textarea',
    hint: 'A short description of the recipe',
  },
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);
    const input = canvas.getByRole('textbox', { name: 'Text Field' });

    await expect(input.tagName).toBe('TEXTAREA');

    await userEvent.type(input, 'A rich, moist chocolate cake.');
    await expect(args['onUpdate:modelValue']).toHaveBeenCalled();
  },
});

const requiredRule = [(val: string | null) => !!val?.trim() || 'This field is required'];

export const RuleRejectsTheValue = meta.story({
  args: {
    rules: requiredRule,
    modelValue: '',
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    // Quasar validates on blur (and on model change), so focus then leave the field.
    await userEvent.click(canvas.getByRole('textbox', { name: 'Text Field' }));
    await userEvent.tab();

    await expect(await canvas.findByText('This field is required')).toBeInTheDocument();
  },
});

export const RuleAcceptsTheValue = meta.story({
  args: {
    rules: requiredRule,
    modelValue: 'Lasagne',
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await userEvent.click(canvas.getByRole('textbox', { name: 'Text Field' }));
    await userEvent.tab();

    await expect(canvas.queryByText('This field is required')).not.toBeInTheDocument();
  },
});
