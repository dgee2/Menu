import preview from '@storybook-config/preview';
import IngredientRowEditor from './ingredient-row-editor.vue';
import { expect, fn, userEvent, within } from 'storybook/test';

const meta = preview.meta({
  title: 'Molecules/Recipe/IngredientRowEditor',
  component: IngredientRowEditor,
  tags: ['autodocs'],
  args: {
    canMoveUp: true,
    canMoveDown: true,
    'onUpdate:ingredientText': fn(),
    'onUpdate:measureText': fn(),
    'onUpdate:sectionTitle': fn(),
    'onUpdate:preparationText': fn(),
    'onUpdate:isOptional': fn(),
    onRemove: fn(),
    onMoveUp: fn(),
    onMoveDown: fn(),
  },
});

export const Default = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByLabelText('Measure')).toBeInTheDocument();
    await expect(canvas.getByLabelText('Ingredient')).toBeInTheDocument();
    await expect(canvas.getByLabelText('Preparation')).toBeInTheDocument();
    await expect(canvas.getByLabelText('Section')).toBeInTheDocument();
    await expect(canvas.getByText('Optional')).toBeInTheDocument();
  },
});

export const EditingFieldsUpdatesModel = meta.story({
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);

    await userEvent.type(canvas.getByLabelText('Ingredient'), 'Flour');
    await expect(args['onUpdate:ingredientText']).toHaveBeenCalled();

    await userEvent.type(canvas.getByLabelText('Measure'), '2 cups');
    await expect(args['onUpdate:measureText']).toHaveBeenCalled();

    await userEvent.click(canvas.getByText('Optional'));
    await expect(args['onUpdate:isOptional']).toHaveBeenCalledWith(true);
  },
});

export const RemoveAndReorderEmitEvents = meta.story({
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);

    await userEvent.click(canvas.getByRole('button', { name: 'Move ingredient up' }));
    await expect(args.onMoveUp).toHaveBeenCalled();

    await userEvent.click(canvas.getByRole('button', { name: 'Move ingredient down' }));
    await expect(args.onMoveDown).toHaveBeenCalled();

    await userEvent.click(canvas.getByRole('button', { name: 'Remove ingredient' }));
    await expect(args.onRemove).toHaveBeenCalled();
  },
});

export const FirstRowCannotMoveUp = meta.story({
  args: {
    canMoveUp: false,
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('button', { name: 'Move ingredient up' })).toBeDisabled();
    await expect(canvas.getByRole('button', { name: 'Move ingredient down' })).toBeEnabled();
  },
});

export const LastRowCannotMoveDown = meta.story({
  args: {
    canMoveDown: false,
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('button', { name: 'Move ingredient down' })).toBeDisabled();
    await expect(canvas.getByRole('button', { name: 'Move ingredient up' })).toBeEnabled();
  },
});
