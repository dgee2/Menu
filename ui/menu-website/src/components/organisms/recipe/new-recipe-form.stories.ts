import preview from '@storybook-config/preview';
import NewRecipeForm from './new-recipe-form.vue';
import { recipeCreateErrorHandler } from '@storybook-config/msw-handlers';
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

// NOTE: MSW isn't intercepting POST requests to `/api/recipe` under
// `vitest --project=storybook` today, for the same underlying reason
// documented in RecipeDetail.stories.ts's Success story (a pre-existing gap
// between `msw-storybook-addon` and this project's Storybook 10 CSF-factory
// setup, confirmed by inspecting the raw MSW "unhandled request" warnings —
// the request is dispatched with the exact registered method/URL but no
// handler matches). Because `onUnhandledRequest` is configured to bypass to
// the real network, the request always fails here regardless of which
// handler is registered, which happens to make this specific assertion
// (error state shown on failure) reliably true today. It will keep being
// true once the underlying MSW gap is fixed, since the registered handler
// below also responds with a 500. Manual verification in the real
// interactive Storybook/dev server confirms the success path (navigating to
// the new recipe's detail page) works correctly.
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

    await expect(await canvas.findByText('Failed to save recipe. Please try again.')).toBeInTheDocument();
  },
});
