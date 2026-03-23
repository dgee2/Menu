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
  parameters: {
    msw: {
      handlers: {
        recipes: recipesSuccessHandler,
      },
    },
  },
});

export const Success = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('All Recipes')).toBeInTheDocument();
  },
});

export const Empty = meta.story({
  parameters: {
    msw: {
      handlers: {
        recipes: recipesEmptyHandler,
      },
    },
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(await canvas.findByText('No recipes found.')).toBeInTheDocument();
  },
});

export const ErrorStory = meta.story({
  name: 'Error',
  parameters: {
    msw: {
      handlers: {
        recipes: recipesErrorHandler,
      },
    },
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(await canvas.findByText('No recipes found.')).toBeInTheDocument();
  },
});

export const Loading = meta.story({
  parameters: {
    msw: {
      handlers: {
        recipes: recipesLoadingHandler,
      },
    },
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('Loading recipes...')).toBeInTheDocument();
  },
});
