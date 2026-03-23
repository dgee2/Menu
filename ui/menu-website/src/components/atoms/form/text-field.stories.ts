import preview from '../../../../.storybook/preview';
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
