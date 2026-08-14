import preview, { router } from '@storybook-config/preview';
import NewRecipeForm from './new-recipe-form.vue';
import {
  recipeCreateErrorHandler,
  recipeCreateSuccessHandler,
} from '@storybook-config/msw-handlers';
import { expect, userEvent, within } from 'storybook/test';

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
    await expect(canvas.getByLabelText('Summary')).toBeInTheDocument();
    await expect(canvas.getByLabelText('Yield')).toBeInTheDocument();
    await expect(canvas.getByLabelText('Servings')).toBeInTheDocument();
    await expect(canvas.getByLabelText('Prep time (minutes)')).toBeInTheDocument();
    await expect(canvas.getByLabelText('Cook time (minutes)')).toBeInTheDocument();
    await expect(canvas.getByLabelText('Total time (minutes)')).toBeInTheDocument();
    await expect(canvas.getByRole('combobox', { name: 'Visibility' })).toHaveValue('Private');
  },
});

export const SelectingVisibilityUpdatesForm = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const combobox = canvas.getByRole('combobox', { name: 'Visibility' });

    await userEvent.click(combobox);
    const body = within(document.body);
    await userEvent.click(await body.findByRole('option', { name: 'Visible to all Menu users' }));

    await expect(combobox).toHaveValue('Visible to all Menu users');
  },
});

export const EmptyTitleBlocksSubmit = meta.story({
  args: {},
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const submitButton = canvas.getByRole('button', { name: 'Save recipe' });

    await userEvent.click(submitButton);

    await expect(await canvas.findByText('Name is required')).toBeInTheDocument();
  },
});

export const ValidTitlePassesValidation = meta.story({
  args: {},
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const nameInput = canvas.getByLabelText('Name');
    const submitButton = canvas.getByRole('button', { name: 'Save recipe' });

    await userEvent.type(nameInput, 'Lasagne');
    await userEvent.click(submitButton);

    await expect(canvas.queryByText('Name is required')).not.toBeInTheDocument();
  },
});

export const NegativeServingsBlocksSubmit = meta.story({
  args: {},
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const nameInput = canvas.getByLabelText('Name');
    const servingsInput = canvas.getByLabelText('Servings');
    const submitButton = canvas.getByRole('button', { name: 'Save recipe' });

    await userEvent.type(nameInput, 'Lasagne');
    await userEvent.type(servingsInput, '-5');
    await userEvent.click(submitButton);

    await expect(await canvas.findByText('Must be 0 or greater')).toBeInTheDocument();
  },
});

export const NonIntegerServingsBlocksSubmit = meta.story({
  args: {},
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const nameInput = canvas.getByLabelText('Name');
    const servingsInput = canvas.getByLabelText('Servings');
    const submitButton = canvas.getByRole('button', { name: 'Save recipe' });

    await userEvent.type(nameInput, 'Lasagne');
    await userEvent.type(servingsInput, '3.5');
    await userEvent.click(submitButton);

    await expect(await canvas.findByText('Must be a whole number')).toBeInTheDocument();
  },
});

export const FullyPopulated = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeCreateSuccessHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await userEvent.type(canvas.getByLabelText('Name'), 'Chocolate Cake');
    await userEvent.type(canvas.getByLabelText('Summary'), 'A rich, moist chocolate cake.');
    await userEvent.type(canvas.getByLabelText('Yield'), 'One 9-inch cake');
    await userEvent.type(canvas.getByLabelText('Servings'), '8');
    await userEvent.type(canvas.getByLabelText('Prep time (minutes)'), '20');
    await userEvent.type(canvas.getByLabelText('Cook time (minutes)'), '35');
    await userEvent.type(canvas.getByLabelText('Total time (minutes)'), '55');

    await userEvent.click(canvas.getByRole('button', { name: 'Save recipe' }));

    await expect(canvas.queryByText('Name is required')).not.toBeInTheDocument();
    await expect(canvas.queryByText('Must be 0 or greater')).not.toBeInTheDocument();
    await expect(canvas.queryByText('Must be a whole number')).not.toBeInTheDocument();
  },
});

export const AddEditRemoveIngredientRows = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const addButton = canvas.getByRole('button', { name: 'Add ingredient' });

    await userEvent.click(addButton);
    await userEvent.click(addButton);

    const ingredientInputs = canvas.getAllByLabelText('Ingredient');
    await expect(ingredientInputs).toHaveLength(2);

    await userEvent.type(ingredientInputs[0], 'Flour');
    await userEvent.type(canvas.getAllByLabelText('Measure')[0], '2 cups');
    await userEvent.type(ingredientInputs[1], 'Sugar');
    await userEvent.type(canvas.getAllByLabelText('Measure')[1], '1 cup');

    await expect(ingredientInputs[0]).toHaveValue('Flour');
    await expect(ingredientInputs[1]).toHaveValue('Sugar');

    const removeButtons = canvas.getAllByRole('button', { name: 'Remove ingredient' });
    await userEvent.click(removeButtons[0]);

    const remainingIngredientInputs = canvas.getAllByLabelText('Ingredient');
    await expect(remainingIngredientInputs).toHaveLength(1);
    await expect(remainingIngredientInputs[0]).toHaveValue('Sugar');
  },
});

export const ReorderIngredientRows = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const addButton = canvas.getByRole('button', { name: 'Add ingredient' });

    await userEvent.click(addButton);
    await userEvent.click(addButton);

    const ingredientInputs = () => canvas.getAllByLabelText('Ingredient');
    await userEvent.type(ingredientInputs()[0], 'Flour');
    await userEvent.type(ingredientInputs()[1], 'Sugar');

    const moveDownButtons = canvas.getAllByRole('button', { name: 'Move ingredient down' });
    await userEvent.click(moveDownButtons[0]);

    const reordered = ingredientInputs();
    await expect(reordered[0]).toHaveValue('Sugar');
    await expect(reordered[1]).toHaveValue('Flour');
  },
});

export const EmptyIngredientRowBlocksSubmit = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const nameInput = canvas.getByLabelText('Name');
    const addButton = canvas.getByRole('button', { name: 'Add ingredient' });
    const submitButton = canvas.getByRole('button', { name: 'Save recipe' });

    await userEvent.type(nameInput, 'Lasagne');
    await userEvent.click(addButton);
    await userEvent.click(submitButton);

    await expect(await canvas.findByText('Ingredient is required')).toBeInTheDocument();
    await expect(canvas.getByText('Measure is required')).toBeInTheDocument();
  },
});

export const AddEditRemoveStepRows = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const addButton = canvas.getByRole('button', { name: 'Add step' });

    await userEvent.click(addButton);
    await userEvent.click(addButton);

    const instructionInputs = canvas.getAllByLabelText('Instructions');
    await expect(instructionInputs).toHaveLength(2);

    await userEvent.type(instructionInputs[0], 'Preheat the oven');
    await userEvent.type(instructionInputs[1], 'Mix the batter');

    await expect(instructionInputs[0]).toHaveValue('Preheat the oven');
    await expect(instructionInputs[1]).toHaveValue('Mix the batter');

    const removeButtons = canvas.getAllByRole('button', { name: 'Remove step' });
    await userEvent.click(removeButtons[0]);

    const remainingInstructionInputs = canvas.getAllByLabelText('Instructions');
    await expect(remainingInstructionInputs).toHaveLength(1);
    await expect(remainingInstructionInputs[0]).toHaveValue('Mix the batter');
  },
});

export const ReorderStepRows = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const addButton = canvas.getByRole('button', { name: 'Add step' });

    await userEvent.click(addButton);
    await userEvent.click(addButton);

    const instructionInputs = () => canvas.getAllByLabelText('Instructions');
    await userEvent.type(instructionInputs()[0], 'Preheat the oven');
    await userEvent.type(instructionInputs()[1], 'Mix the batter');

    const moveDownButtons = canvas.getAllByRole('button', { name: 'Move step down' });
    await userEvent.click(moveDownButtons[0]);

    const reordered = instructionInputs();
    await expect(reordered[0]).toHaveValue('Mix the batter');
    await expect(reordered[1]).toHaveValue('Preheat the oven');
  },
});

export const EmptyStepRowBlocksSubmit = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const nameInput = canvas.getByLabelText('Name');
    const addButton = canvas.getByRole('button', { name: 'Add step' });
    const submitButton = canvas.getByRole('button', { name: 'Save recipe' });

    await userEvent.type(nameInput, 'Lasagne');
    await userEvent.click(addButton);
    await userEvent.click(submitButton);

    await expect(await canvas.findByText('Instructions are required')).toBeInTheDocument();
  },
});

export const ZeroStepDurationBlocksSubmit = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const nameInput = canvas.getByLabelText('Name');
    const addButton = canvas.getByRole('button', { name: 'Add step' });
    const submitButton = canvas.getByRole('button', { name: 'Save recipe' });

    await userEvent.type(nameInput, 'Lasagne');
    await userEvent.click(addButton);
    await userEvent.type(canvas.getByLabelText('Instructions'), 'Preheat the oven');
    await userEvent.type(canvas.getByLabelText('Duration (minutes)'), '0');
    await userEvent.click(submitButton);

    await expect(await canvas.findByText('Must be greater than 0')).toBeInTheDocument();
  },
});

export const SubmitFailureShowsError = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeCreateErrorHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const nameInput = canvas.getByLabelText('Name');
    const submitButton = canvas.getByRole('button', { name: 'Save recipe' });

    await userEvent.type(nameInput, 'Lasagne');
    await userEvent.click(submitButton);

    await expect(
      await canvas.findByText('Failed to save recipe. Please try again.'),
    ).toBeInTheDocument();
    // The form stays put rather than navigating to a recipe that was never created.
    await expect(router.currentRoute.value.path).toBe('/');
  },
});
