import preview from '../../../../.storybook/preview';
import NewRecipeForm from './new-recipe-form.vue';
import { expect, within } from 'storybook/test';

const meta = preview.meta({
  title: 'Organisms/Recipe/NewRecipeForm',
  component: NewRecipeForm,
  tags: ['autodocs'],
  args: {},
});

export const Default = meta.story({
  args: {},
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByLabelText('Name')).toBeInTheDocument();
  },
});
