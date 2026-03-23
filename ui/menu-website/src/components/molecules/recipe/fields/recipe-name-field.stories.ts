import RecipeNameField from './recipe-name-field.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, fn, userEvent, within } from 'storybook/test';

const meta = {
  title: 'Molecules/Recipe/Fields/RecipeNameField',
  component: RecipeNameField,
  tags: ['autodocs'],
  args: {
    'onUpdate:modelValue': fn(),
  },
} satisfies Meta<typeof RecipeNameField>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);
    const input = canvas.getByRole('textbox', { name: 'Name' });

    await userEvent.type(input, 'Lasagne');
    await expect(args['onUpdate:modelValue']).toHaveBeenCalled();
  },
};

export const PreFilledValue: Story = {
  args: {
    modelValue: 'Chocolate Cake',
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByDisplayValue('Chocolate Cake')).toBeInTheDocument();
  },
};
