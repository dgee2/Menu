import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { flushPromises, mount, type VueWrapper } from '@vue/test-utils';
import { QInput, QLayout, QPageContainer, Quasar } from 'quasar';
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
    restoreRecipe: vi.fn(),
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
    vi.useRealTimers();
  });

  it('requests the caller-owned scope by default', async () => {
    await mountPage();

    expect(getRecipes).toHaveBeenCalledWith('mine', undefined);
  });

  it('renders the summary and timings the list endpoint already returns', async () => {
    // These have been on RecipeListItem since the API gained them; the page rendered only the
    // title, so every recipe looked identical.
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('Lasagne');
    expect(wrapper.text()).toContain('Layers of pasta.');
    expect(wrapper.text()).toContain('50 min');
    expect(wrapper.text()).toContain('Serves 4');
    expect(wrapper.text()).toContain('Private');
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
      .find((button) => button.text() === 'All Menu users');
    await sharedButton?.trigger('click');
    await flushPromises();

    expect(getRecipes).toHaveBeenLastCalledWith('authenticated', undefined);
  });

  it('debounces search input before requesting matching recipes', async () => {
    vi.useFakeTimers();
    const wrapper = await mountPage();
    getRecipes.mockClear();

    await wrapper.find('input[aria-label="Search recipes"]').setValue(' lemon cake ');
    await flushPromises();
    expect(getRecipes).not.toHaveBeenCalled();

    await vi.advanceTimersByTimeAsync(299);
    expect(getRecipes).not.toHaveBeenCalled();

    await vi.advanceTimersByTimeAsync(1);
    await flushPromises();
    expect(getRecipes).toHaveBeenCalledWith('mine', 'lemon cake');
    vi.useRealTimers();
  });

  it('restores the unfiltered request when Quasar clears the search to null', async () => {
    vi.useFakeTimers();
    const wrapper = await mountPage();
    await wrapper.find('input[aria-label="Search recipes"]').setValue('lemon');
    await vi.advanceTimersByTimeAsync(300);
    await flushPromises();
    expect(getRecipes).toHaveBeenLastCalledWith('mine', 'lemon');
    getRecipes.mockClear();

    wrapper.findComponent(QInput).vm.$emit('update:modelValue', null);
    await flushPromises();
    await vi.advanceTimersByTimeAsync(300);
    await flushPromises();

    expect(getRecipes).toHaveBeenLastCalledWith('mine', undefined);
  });

  it('shows an empty-search message when no recipe matches a nonempty query', async () => {
    getRecipes.mockResolvedValue([]);
    const wrapper = await mountPage();

    await wrapper.find('input[aria-label="Search recipes"]').setValue('missing dish');
    await new Promise((resolve) => setTimeout(resolve, 310));
    await flushPromises();

    expect(wrapper.text()).toContain('No recipes match your search.');
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
