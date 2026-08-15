import { expect, within } from 'storybook/test';
import preview, { withPageLayout } from '@storybook-config/preview';
import EditRecipe from './EditRecipe.vue';
import {
  recipeDetailEditableHandler,
  recipeDetailErrorHandler,
  recipeDetailLoadingHandler,
  recipeDetailNotFoundHandler,
  recipeDetailSuccessHandler,
} from '@storybook-config/msw-handlers';

const meta = preview.meta({
  title: 'Pages/EditRecipe',
  component: EditRecipe,
  tags: ['autodocs'],
  decorators: [withPageLayout],
  args: {
    recipeId: '1',
  },
});

export const Editing = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeDetailEditableHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(await canvas.findByDisplayValue('Chocolate Cake')).toBeInTheDocument();
    await expect(canvas.getByRole('button', { name: 'Save changes' })).toBeInTheDocument();
  },
});

export const NotEditable = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeDetailSuccessHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    // The server decides who may edit. Without canEdit, the page refuses rather than offering a
    // form that could only ever 403.
    await expect(
      await canvas.findByText('You do not have permission to edit this recipe.'),
    ).toBeInTheDocument();
    await expect(canvas.queryByRole('button', { name: 'Save changes' })).not.toBeInTheDocument();
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

export const NotFound = meta.story({
  beforeEach({ msw }) {
    msw.use(recipeDetailNotFoundHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(await canvas.findByText('Recipe not found.')).toBeInTheDocument();
  },
});

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
