import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { flushPromises, mount, type VueWrapper } from '@vue/test-utils';
import { QLayout, QPageContainer, Quasar } from 'quasar';
import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query';
import { createMemoryHistory, createRouter, type Router } from 'vue-router';
import { defineComponent } from 'vue';
import RecipeList from './RecipeList.vue';
import { ApiError } from '@/services/api-error';
import type { RecipeListItem } from '@/services/recipe-api';

const getRecipes = vi.fn();

vi.mock('@/services/recipe-api', () => ({
  useRecipeApi: () => ({
    getRecipes,
    getRecipe: vi.fn(),
    deleteRecipe: vi.fn(),
    postRecipe: vi.fn(),
    putRecipe: vi.fn(),
    getIngredientUnits: vi.fn(),
  }),
}));

const listItem = (overrides: Partial<RecipeListItem> = {}) =>
  ({
    id: 1,
    title: 'Lasagne',
    accessScope: 'Private',
    summary: 'Layers of pasta.',
    servings: 4,
    prepTimeMinutes: 20,
    cookTimeMinutes: 30,
    totalTimeMinutes: null,
    effectiveTotalTimeMinutes: 50,
    ...overrides,
  }) as unknown as RecipeListItem;

const StubPage = defineComponent({ template: '<div />' });

let router: Router;
const mounted: VueWrapper[] = [];

const mountPage = async () => {
  router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: StubPage },
      { path: '/recipes', component: RecipeList },
      { path: '/recipe/:recipeId', component: StubPage },
    ],
  });
  await router.push('/recipes');
  await router.isReady();

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  // QPage refuses to render without a QLayout/QPageContainer ancestor.
  const LayoutHost = defineComponent({
    components: { QLayout, QPageContainer },
    template: '<q-layout><q-page-container><router-view /></q-page-container></q-layout>',
  });

  const wrapper = mount(LayoutHost, {
    global: { plugins: [Quasar, [VueQueryPlugin, { queryClient }], router] },
    attachTo: document.body,
  });

  mounted.push(wrapper);
  await flushPromises();

  return wrapper;
};

describe('RecipeList', () => {
  beforeEach(() => {
    getRecipes.mockReset();
    getRecipes.mockResolvedValue([listItem()]);
  });

  afterEach(() => {
    while (mounted.length) mounted.pop()?.unmount();
  });

  it('requests the caller-owned scope by default', async () => {
    await mountPage();

    expect(getRecipes).toHaveBeenCalledWith('mine');
  });

  it('renders the summary and timings the list endpoint already returns', async () => {
    // These have been on RecipeListItem since the API gained them; the page rendered only the
    // title, so every recipe looked identical.
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('Lasagne');
    expect(wrapper.text()).toContain('Layers of pasta.');
    expect(wrapper.text()).toContain('50 min');
    expect(wrapper.text()).toContain('Serves 4');
  });

  it('renders the server-computed total rather than deriving one', async () => {
    getRecipes.mockResolvedValue([
      listItem({ totalTimeMinutes: 90, effectiveTotalTimeMinutes: 90, servings: null }),
    ]);
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('90 min');
  });

  it('omits the timings line when the recipe has none', async () => {
    getRecipes.mockResolvedValue([
      listItem({ effectiveTotalTimeMinutes: null, servings: null, summary: null }),
    ]);
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('Lasagne');
    expect(wrapper.text()).not.toContain('min');
  });

  it('switches scope when the toggle is used', async () => {
    const wrapper = await mountPage();

    const sharedButton = wrapper
      .findAll('button')
      .find((button) => button.text() === 'Shared with everyone');
    await sharedButton?.trigger('click');
    await flushPromises();

    expect(getRecipes).toHaveBeenLastCalledWith('authenticated');
    expect(wrapper.find('h1').text()).toBe('Shared with everyone');
  });

  it('explains an empty list in terms of the current scope', async () => {
    getRecipes.mockResolvedValue([]);
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('You have not created any recipes yet.');
  });

  it('distinguishes a load failure from an empty list', async () => {
    getRecipes.mockRejectedValue(new ApiError('boom', 500, { title: 'Internal Server Error' }));
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('Something went wrong loading recipes.');
    expect(wrapper.text()).not.toContain('You have not created any recipes yet.');
  });

  it('falls back to a generic message when a failure carries no detail', async () => {
    getRecipes.mockRejectedValue(new TypeError('Failed to fetch'));
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('Failed to fetch');
  });
});
