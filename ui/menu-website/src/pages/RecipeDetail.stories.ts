import { expect, within } from 'storybook/test';
import preview, { withPageLayout } from '@storybook-config/preview';
import RecipeDetail from './RecipeDetail.vue';
import {
  recipeDetailSuccessHandler,
  recipeDetailNotFoundHandler,
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

export const Loading = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeDetailLoadingHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('Loading recipe...')).toBeInTheDocument();
  },
});
