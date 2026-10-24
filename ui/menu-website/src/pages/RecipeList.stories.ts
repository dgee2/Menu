import { expect, userEvent, within } from 'storybook/test';
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
    await expect(canvas.getByRole('heading', { name: 'Recipe search' })).toBeInTheDocument();
    await expect(await canvas.findByText('Chocolate Cake')).toBeInTheDocument();
    await expect(await canvas.findByText('Tomato Soup')).toBeInTheDocument();
    await expect(canvas.getByText('Shared')).toBeInTheDocument();
    await expect(canvas.getByText('Private')).toBeInTheDocument();
  },
});

export const ScopeToggle = meta.story({
  beforeEach({ msw }) {
    msw.use(recipesSuccessHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    // `scope=authenticated` has been supported by the API all along; only `mine` was reachable.
    await expect(canvas.getByRole('button', { name: 'Mine' })).toBeInTheDocument();
    await expect(canvas.getByRole('button', { name: 'All Menu users' })).toBeInTheDocument();
    await userEvent.click(canvas.getByRole('button', { name: 'All Menu users' }));
    await expect(canvas.getByRole('textbox', { name: 'Search recipes' })).toBeInTheDocument();
  },
});

export const Search = meta.story({
  beforeEach({ msw }) {
    msw.use(recipesSuccessHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const input = canvas.getByRole('textbox', { name: 'Search recipes' });
    await userEvent.type(input, 'cake');
    await expect(await canvas.findByText('Chocolate Cake')).toBeInTheDocument();
    await expect(canvas.getByText('Shared')).toBeInTheDocument();
  },
});

export const Empty = meta.story({
  beforeEach({ msw }) {
    msw.use(recipesEmptyHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(
      await canvas.findByText('You have not created any recipes yet.'),
    ).toBeInTheDocument();
  },
});

export const ErrorStory = meta.story({
  name: 'Error',
  beforeEach({ msw }) {
    msw.use(recipesErrorHandler);
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    // An empty list and a failed load are now distinct states, so this asserts the banner rather
    // than the empty message the page used to show for both.
    await expect(
      await canvas.findByText('Something went wrong loading recipes.'),
    ).toBeInTheDocument();
    await expect(
      canvas.queryByText('You have not created any recipes yet.'),
    ).not.toBeInTheDocument();
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
