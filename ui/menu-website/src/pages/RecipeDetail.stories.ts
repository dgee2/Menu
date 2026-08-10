import { expect, within } from 'storybook/test';
import preview, { withPageLayout } from '@storybook-config/preview';
import RecipeDetail from './RecipeDetail.vue';
import {
  recipeDetailSuccessHandler,
  recipeDetailNotFoundHandler,
  recipeDetailErrorHandler,
  recipeDetailLoadingHandler,
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
