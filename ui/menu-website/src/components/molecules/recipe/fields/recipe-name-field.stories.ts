import preview from '../../../../../.storybook/preview';
import RecipeNameField from './recipe-name-field.vue';
import { expect, fn, userEvent, within } from 'storybook/test';

const meta = preview.meta({
  title: 'Molecules/Recipe/Fields/RecipeNameField',
  component: RecipeNameField,
  tags: ['autodocs'],
  args: {
    'onUpdate:modelValue': fn(),
  },
});

export const Default = meta.story({
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);
    const input = canvas.getByRole('textbox', { name: 'Name' });

    await userEvent.type(input, 'Lasagne');
    await expect(args['onUpdate:modelValue']).toHaveBeenCalled();
  },
});

export const PreFilledValue = meta.story({
  args: {
    modelValue: 'Chocolate Cake',
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByDisplayValue('Chocolate Cake')).toBeInTheDocument();
  },
});
