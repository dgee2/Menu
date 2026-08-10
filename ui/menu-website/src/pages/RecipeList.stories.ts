import { expect, within } from 'storybook/test';
import preview, { withPageLayout } from '@storybook-config/preview';
import RecipeList from './RecipeList.vue';
import {
  recipesEmptyHandler,
  recipesErrorHandler,
  recipesLoadingHandler,
  recipesSuccessHandler,
} from '@storybook-config/msw-handlers';

const meta = preview.meta({
  title: 'Pages/RecipeList',
  component: RecipeList,
  tags: ['autodocs'],
  decorators: [withPageLayout],
});

export const Success = meta.story({
  beforeEach({ msw }) {
    msw.use(recipesSuccessHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('My Recipes')).toBeInTheDocument();
    await expect(await canvas.findByText('Chocolate Cake')).toBeInTheDocument();
  },
});

export const Empty = meta.story({
  beforeEach({ msw }) {
    msw.use(recipesEmptyHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(await canvas.findByText('No recipes found.')).toBeInTheDocument();
  },
});

export const ErrorStory = meta.story({
  name: 'Error',
  beforeEach({ msw }) {
    msw.use(recipesErrorHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(await canvas.findByText('No recipes found.')).toBeInTheDocument();
  },
});

export const Loading = meta.story({
  beforeEach({ msw }) {
    msw.use(recipesLoadingHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('Loading recipes...')).toBeInTheDocument();
  },
});
