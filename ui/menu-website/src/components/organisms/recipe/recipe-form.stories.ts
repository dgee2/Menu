import preview, { router } from '@storybook-config/preview';
import RecipeForm from './recipe-form.vue';
import {
  recipeCreateErrorHandler,
  recipeCreateSuccessHandler,
} from '@storybook-config/msw-handlers';
import { expect, userEvent, within } from 'storybook/test';
import type { RecipeDetail } from '@/services/recipe-api';

const existingRecipe = {
  id: 7,
  title: 'Existing Lasagne',
  accessScope: 'AuthenticatedUsers',
  summary: 'Layers.',
  yieldText: 'One tray',
  servings: 6,
  prepTimeMinutes: 20,
  cookTimeMinutes: 40,
  totalTimeMinutes: null,
  effectiveTotalTimeMinutes: 60,
  canEdit: true,
  canDelete: true,
  ingredients: [
    {
      sortOrder: 0,
      ingredientText: 'Pasta',
      measureText: '250g',
      sectionTitle: 'For the layers',
      preparationText: null,
      isOptional: false,
    },
  ],
  steps: [{ sortOrder: 0, instructionText: 'Assemble', title: null, durationMinutes: null }],
} as unknown as RecipeDetail;

const meta = preview.meta({
  title: 'Organisms/Recipe/RecipeForm',
  component: RecipeForm,
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

// Create mode opens with one blank ingredient row and one blank step row already present, so
// these stories start from a row count of 1 rather than 0.
export const SeedsOneBlankRowOfEach = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await expect(canvas.getAllByLabelText('Ingredient')).toHaveLength(1);
    await expect(canvas.getAllByLabelText('Instructions')).toHaveLength(1);
  },
});

export const BlankSeededRowsDoNotBlockSubmit = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await userEvent.type(canvas.getByLabelText('Name'), 'Lasagne');
    await userEvent.click(canvas.getByRole('button', { name: 'Save recipe' }));

    // "A recipe with zero ingredients and zero steps is valid" has to survive the seeding.
    await expect(canvas.queryByText('Ingredient is required')).not.toBeInTheDocument();
    await expect(canvas.queryByText('Instructions are required')).not.toBeInTheDocument();
  },
});

export const TotalTimeShowsTheDerivedValueAsAPlaceholder = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await userEvent.type(canvas.getByLabelText('Prep time (minutes)'), '20');
    await userEvent.type(canvas.getByLabelText('Cook time (minutes)'), '30');

    // A placeholder, never a value: filling the input in would make a derived total look identical
    // to an explicitly chosen one.
    const totalTime = canvas.getByLabelText('Total time (minutes)');
    await expect(totalTime).toHaveValue(null);
    await expect(totalTime).toHaveAttribute('placeholder', '50');
    await expect(
      canvas.getByText('Calculated: 50 min — enter a value to override'),
    ).toBeInTheDocument();
  },
});

export const AddEditRemoveIngredientRows = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const addButton = canvas.getByRole('button', { name: 'Add ingredient' });

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

    await userEvent.click(canvas.getByRole('button', { name: 'Add ingredient' }));

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

export const PartlyFilledIngredientRowBlocksSubmit = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await userEvent.type(canvas.getByLabelText('Name'), 'Lasagne');
    // A row the user has started is no longer blank, so its required fields apply again.
    await userEvent.type(canvas.getAllByLabelText('Ingredient')[0], 'Flour');
    await userEvent.click(canvas.getByRole('button', { name: 'Save recipe' }));

    await expect(await canvas.findByText('Measure is required')).toBeInTheDocument();
  },
});

// Section is free text with suggestions, not a closed list — the first row to use a new section
// has to be able to invent it. This story covers that entry path; the suggestion list itself
// (sections already used in this recipe being offered to later rows) is asserted in
// recipe-form.test.ts and ingredient-row-editor.test.ts, where the props can be inspected directly
// rather than through a portalled QSelect menu.
export const SectionAcceptsANewTitle = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    const firstSection = canvas.getAllByLabelText('Section')[0];
    await userEvent.type(firstSection, 'For the sauce');
    await userEvent.keyboard('{Enter}');

    await expect(firstSection).toHaveValue('For the sauce');
  },
});

export const AddEditRemoveStepRows = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await userEvent.click(canvas.getByRole('button', { name: 'Add step' }));

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

    await userEvent.click(canvas.getByRole('button', { name: 'Add step' }));

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

export const PartlyFilledStepRowBlocksSubmit = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await userEvent.type(canvas.getByLabelText('Name'), 'Lasagne');
    await userEvent.type(canvas.getAllByLabelText('Title')[0], 'Preheat');
    await userEvent.click(canvas.getByRole('button', { name: 'Save recipe' }));

    await expect(await canvas.findByText('Instructions are required')).toBeInTheDocument();
  },
});

export const ZeroStepDurationBlocksSubmit = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await userEvent.type(canvas.getByLabelText('Name'), 'Lasagne');
    await userEvent.type(canvas.getByLabelText('Instructions'), 'Preheat the oven');
    await userEvent.type(canvas.getByLabelText('Duration (minutes)'), '0');
    await userEvent.click(canvas.getByRole('button', { name: 'Save recipe' }));

    await expect(await canvas.findByText('Must be greater than 0')).toBeInTheDocument();
  },
});

// Edit mode is the same component with `initialRecipe` supplied - there is no second form.
export const EditingAnExistingRecipe = meta.story({
  args: { initialRecipe: existingRecipe },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await expect(canvas.getByLabelText('Name')).toHaveValue('Existing Lasagne');
    await expect(canvas.getByRole('combobox', { name: 'Visibility' })).toHaveValue(
      'Visible to all Menu users',
    );
    await expect(canvas.getByRole('button', { name: 'Save changes' })).toBeInTheDocument();
    // Edit mode seeds nothing: a recipe saved with one ingredient shows exactly one.
    await expect(canvas.getAllByLabelText('Ingredient')).toHaveLength(1);
    await expect(canvas.getAllByLabelText('Ingredient')[0]).toHaveValue('Pasta');
  },
});

// A 500 is not a 4xx, so it gets the generic banner rather than a server-supplied message. The
// request payload and the 409 -> title-field path are covered in recipe-form.test.ts, which mocks
// the API layer and can produce an exact problem-details body.
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
