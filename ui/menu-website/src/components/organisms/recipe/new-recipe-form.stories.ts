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

// A fully populated form wired to a successful create. In the interactive
// Storybook dev server (where the MSW service worker is active) submitting this
// story navigates to the new recipe's detail page. The story therefore asserts
// only that a fully populated form clears validation and reaches submit — see
// the NOTE below for why the response itself is not asserted here.
export const FullyPopulated = meta.story({
  parameters: {
    msw: {
      handlers: {
        recipe: recipeCreateSuccessHandler,
      },
    },
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

// NOTE: this story exercises the failed-submit UI, but not specifically a 500.
// Under `vitest --project=storybook`, MSW does not serve the `*/api/recipe`
// handler registered below — the POST fails at the network layer instead, which
// drives the component down the same failure path. (Verified by probing: a POST
// to a sub-path such as `*/api/recipe/probe-ok` is served correctly with its
// body, while `*/api/recipe` is not, so this is a handler-registration gap in the
// Storybook/Vitest MSW setup rather than anything specific to POST. It equally
// affects the RecipeList and RecipeDetail stories.) The story keeps passing
// unchanged once that gap is closed, since the handler below also returns 500.
// The response-specific behaviour — the exact request payload, the 500 → banner
// mapping, and the success path navigating to the new recipe — is covered
// deterministically in new-recipe-form.test.ts, which mocks the API layer.
export const SubmitFailureShowsError = meta.story({
  parameters: {
    msw: {
      handlers: {
        recipe: recipeCreateErrorHandler,
      },
    },
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
