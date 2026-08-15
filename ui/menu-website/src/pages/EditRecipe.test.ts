import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { flushPromises, mount, type VueWrapper } from '@vue/test-utils';
import { QLayout, QPageContainer, Quasar } from 'quasar';
import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query';
import { createMemoryHistory, createRouter, type Router } from 'vue-router';
import { defineComponent } from 'vue';
import EditRecipe from './EditRecipe.vue';
import { ApiError } from '@/services/api-error';
import type { RecipeDetail } from '@/services/recipe-api';

const getRecipe = vi.fn();

vi.mock('@/services/recipe-api', () => ({
  useRecipeApi: () => ({
    getRecipe,
    getRecipes: vi.fn(),
    deleteRecipe: vi.fn(),
    postRecipe: vi.fn(),
    putRecipe: vi.fn(),
    getIngredientUnits: vi.fn(),
  }),
}));

const recipe = (overrides: Partial<RecipeDetail> = {}) =>
  ({
    id: 1,
    title: 'Lasagne',
    accessScope: 'Private',
    summary: null,
    servings: null,
    yieldText: null,
    prepTimeMinutes: null,
    cookTimeMinutes: null,
    totalTimeMinutes: null,
    effectiveTotalTimeMinutes: null,
    canEdit: true,
    canDelete: true,
    ingredients: [],
    steps: [],
    ...overrides,
  }) as unknown as RecipeDetail;

const StubPage = defineComponent({ template: '<div />' });

let router: Router;
const mounted: VueWrapper[] = [];

const mountPage = async () => {
  router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: StubPage },
      { path: '/recipes', component: StubPage },
      { path: '/recipe/:recipeId', component: StubPage },
      { path: '/recipe/:recipeId/edit', component: EditRecipe, props: true },
    ],
  });
  await router.push('/recipe/1/edit');
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

describe('EditRecipe', () => {
  beforeEach(() => {
    getRecipe.mockReset();
    getRecipe.mockResolvedValue(recipe());
  });

  afterEach(() => {
    while (mounted.length) mounted.pop()?.unmount();
  });

  it('renders the form populated from the recipe', async () => {
    getRecipe.mockResolvedValue(recipe({ title: 'Lasagne' }));
    const wrapper = await mountPage();

    expect(wrapper.find('input').element.value).toBe('Lasagne');
    expect(wrapper.find('button[type="submit"]').text()).toBe('Save changes');
  });

  it('refuses to show the form when the server says the caller may not edit', async () => {
    // Better to say so here than to let the user fill in a form that would only ever 403.
    getRecipe.mockResolvedValue(recipe({ canEdit: false }));
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('You do not have permission to edit this recipe.');
    expect(wrapper.find('form').exists()).toBe(false);
  });

  it('shows a not-found message for a 404', async () => {
    getRecipe.mockRejectedValue(new ApiError('nope', 404, { status: 404 }));
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('Recipe not found.');
  });

  it('distinguishes a load failure from a missing recipe', async () => {
    getRecipe.mockRejectedValue(new ApiError('boom', 500, { title: 'Internal Server Error' }));
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('Something went wrong loading this recipe.');
    expect(wrapper.text()).not.toContain('Recipe not found.');
  });
});
