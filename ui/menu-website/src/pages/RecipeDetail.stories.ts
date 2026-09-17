import { expect, userEvent, waitFor, within } from 'storybook/test';
import preview, { router, withPageLayout } from '@storybook-config/preview';
import RecipeDetail from './RecipeDetail.vue';
import {
  recipeDetailSuccessHandler,
  recipeDetailNotFoundHandler,
  recipeDetailEditableHandler,
  recipeDetailErrorHandler,
  recipeDetailLoadingHandler,
  recipeDeleteSuccessHandler,
  recipeRestoreSuccessHandler,
  recipeDeleteErrorHandler,
} from '@storybook-config/msw-handlers';

const meta = preview.meta({
  title: 'Pages/RecipeDetail',
  component: RecipeDetail,
  tags: ['autodocs'],
  decorators: [withPageLayout],
  args: {
    recipeId: '1',
  },
});

export const Success = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeDetailSuccessHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(await canvas.findByText('Chocolate Cake')).toBeInTheDocument();
    await expect(await canvas.findByText('A rich, moist chocolate cake.')).toBeInTheDocument();
    await expect(await canvas.findByText('2 cups Flour')).toBeInTheDocument();
    await expect(await canvas.findByText('Preheat the oven to 180C.')).toBeInTheDocument();
    await expect(canvas.queryByRole('link', { name: 'Edit' })).not.toBeInTheDocument();
    await expect(canvas.queryByRole('button', { name: 'Delete' })).not.toBeInTheDocument();
  },
});

export const EditableActions = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeDetailEditableHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await expect(await canvas.findByRole('link', { name: 'Edit' })).toBeInTheDocument();
    await expect(await canvas.findByRole('button', { name: 'Delete' })).toBeInTheDocument();

    await userEvent.click(canvas.getByRole('link', { name: 'Edit' }));
    await waitFor(() => expect(router.currentRoute.value.path).toBe('/recipe/1/edit'));
  },
});

export const NotFound = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeDetailNotFoundHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(await canvas.findByText('Recipe not found.')).toBeInTheDocument();
  },
});

// The page used to render "Recipe not found." for any failure at all. A 404 and a 500 are now
// distinct states, so this asserts the one the previous story could not distinguish.
export const LoadFailure = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeDetailErrorHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(
      await canvas.findByText('Something went wrong loading this recipe.'),
    ).toBeInTheDocument();
    await expect(canvas.queryByText('Recipe not found.')).not.toBeInTheDocument();
  },
});

export const Loading = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeDetailLoadingHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('Loading recipe...')).toBeInTheDocument();
  },
});

export const DeleteSuccess = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeDetailEditableHandler, recipeDeleteSuccessHandler, recipeRestoreSuccessHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const body = within(document.body);

    await userEvent.click(await canvas.findByRole('button', { name: 'Delete' }));
    const dialog = await body.findByRole('dialog');
    await expect(dialog).toHaveTextContent('Delete this recipe?');

    await userEvent.click(within(dialog).getByRole('button', { name: 'Delete' }));
    await waitFor(() => expect(router.currentRoute.value.path).toBe('/recipes'));

    await userEvent.click(await body.findByRole('button', { name: 'Undo' }));
    await expect(await body.findByText('Recipe restored.')).toBeInTheDocument();
  },
});

export const DeleteFailureShowsError = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeDetailEditableHandler, recipeDeleteErrorHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const body = within(document.body);

    await userEvent.click(await canvas.findByRole('button', { name: 'Delete' }));
    const dialog = await body.findByRole('dialog');
    await userEvent.click(within(dialog).getByRole('button', { name: 'Delete' }));

    await expect(
      await canvas.findByText('Failed to delete recipe. Please try again.'),
    ).toBeInTheDocument();
    await waitFor(() => expect(body.queryByRole('dialog')).not.toBeInTheDocument());
    await expect(router.currentRoute.value.path).toBe('/');
  },
});
