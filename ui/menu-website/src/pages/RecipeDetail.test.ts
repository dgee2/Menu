import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { flushPromises, mount, type VueWrapper } from '@vue/test-utils';
import { QLayout, QPageContainer, Quasar } from 'quasar';
import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query';
import { createMemoryHistory, createRouter, type Router } from 'vue-router';
import { defineComponent } from 'vue';
import RecipeDetail from './RecipeDetail.vue';
import { ApiError } from '@/services/api-error';
import type { RecipeDetail as RecipeDetailModel } from '@/services/recipe-api';

const getRecipe = vi.fn();
const deleteRecipe = vi.fn();

vi.mock('@/services/recipe-api', () => ({
  useRecipeApi: () => ({
    getRecipe,
    deleteRecipe,
    postRecipe: vi.fn(),
    putRecipe: vi.fn(),
    getRecipes: vi.fn(),
    getIngredientUnits: vi.fn(),
  }),
}));

const recipe = (overrides: Partial<RecipeDetailModel> = {}) =>
  ({
    id: 1,
    title: 'Lasagne',
    accessScope: 'Private',
    summary: 'Layers.',
    servings: 4,
    yieldText: 'One tray',
    prepTimeMinutes: 20,
    cookTimeMinutes: 30,
    totalTimeMinutes: null,
    effectiveTotalTimeMinutes: 50,
    canEdit: true,
    canDelete: true,
    ingredients: [],
    steps: [],
    ...overrides,
  }) as unknown as RecipeDetailModel;

const ingredient = (sortOrder: number, ingredientText: string, sectionTitle: string | null) => ({
  sortOrder,
  ingredientText,
  measureText: '1',
  sectionTitle,
  preparationText: null,
  isOptional: false,
});

const StubPage = defineComponent({ template: '<div />' });

let router: Router;

// Tracked so every mount is torn down between tests: QDialog portals its content to document.body,
// and a wrapper left mounted leaves a stale dialog there for the next test to find and click.
const mounted: VueWrapper[] = [];

const mountPage = async () => {
  router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: StubPage },
      { path: '/recipes', component: StubPage },
      { path: '/recipe/:recipeId', component: RecipeDetail, props: true },
      { path: '/recipe/:recipeId/edit', component: StubPage },
    ],
  });
  await router.push('/recipe/1');
  await router.isReady();

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  // QPage refuses to render without a QLayout/QPageContainer ancestor, so the page is mounted
  // inside the same shell the app gives it. Registered explicitly because Quasar's components are
  // resolved at compile time by the Vite plugin, not globally at runtime.
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

const clickButton = async (wrapper: VueWrapper, name: string) => {
  const button = wrapper.findAll('button').find((candidate) => candidate.text().endsWith(name));
  if (!button) throw new Error(`No button labelled "${name}"`);
  await button.trigger('click');
};

describe('RecipeDetail', () => {
  beforeEach(() => {
    getRecipe.mockReset();
    deleteRecipe.mockReset();
    deleteRecipe.mockResolvedValue(undefined);
  });

  afterEach(() => {
    while (mounted.length) mounted.pop()?.unmount();
  });

  it('renders the recipe once loaded', async () => {
    getRecipe.mockResolvedValue(recipe());
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('Lasagne');
    expect(wrapper.text()).toContain('Layers.');
    expect(wrapper.text()).toContain('Servings: 4');
  });

  it('renders the server-computed total time rather than deriving one', async () => {
    getRecipe.mockResolvedValue(recipe({ totalTimeMinutes: null, effectiveTotalTimeMinutes: 50 }));
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('Total: 50 min');
  });

  it('shows a not-found message for a 404', async () => {
    getRecipe.mockRejectedValue(new ApiError('nope', 404, { status: 404 }));
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('Recipe not found.');
  });

  it('distinguishes a load failure from a missing recipe', async () => {
    // Rendering "Recipe not found." for a dropped connection sends the user looking for a recipe
    // that is almost certainly still there.
    getRecipe.mockRejectedValue(new ApiError('boom', 500, { title: 'Internal Server Error' }));
    const wrapper = await mountPage();

    expect(wrapper.text()).not.toContain('Recipe not found.');
    // A 5xx explains the server, not the request - "Internal Server Error" tells the user nothing.
    expect(wrapper.text()).toContain('Something went wrong loading this recipe.');
    expect(wrapper.text()).not.toContain('Internal Server Error');
  });

  it('shows the server explanation for a client error', async () => {
    getRecipe.mockRejectedValue(new ApiError('nope', 403, { detail: 'This recipe was unshared.' }));
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('This recipe was unshared.');
  });

  it('falls back to a generic message when a failure carries no detail', async () => {
    getRecipe.mockRejectedValue(new TypeError('Failed to fetch'));
    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('Failed to fetch');
    expect(wrapper.text()).not.toContain('Recipe not found.');
  });

  describe('ingredient sections', () => {
    it('groups contiguous runs of the same section', async () => {
      getRecipe.mockResolvedValue(
        recipe({
          ingredients: [
            ingredient(0, 'Butter', 'For the sauce'),
            ingredient(1, 'Flour', 'For the sauce'),
            ingredient(2, 'Pasta', null),
          ],
        }),
      );
      const wrapper = await mountPage();

      const sections = wrapper.findAll('h3').map((heading) => heading.text());
      expect(sections).toEqual(['For the sauce']);
    });

    it('renders a repeated section twice instead of teleporting rows together', async () => {
      // Looking the section up by title would move "Cheese" up next to "Butter", silently
      // reordering a recipe the author had deliberately arranged.
      getRecipe.mockResolvedValue(
        recipe({
          ingredients: [
            ingredient(0, 'Butter', 'For the sauce'),
            ingredient(1, 'Pasta', 'For the layers'),
            ingredient(2, 'Cheese', 'For the sauce'),
          ],
        }),
      );
      const wrapper = await mountPage();

      expect(wrapper.findAll('h3').map((heading) => heading.text())).toEqual([
        'For the sauce',
        'For the layers',
        'For the sauce',
      ]);

      const rows = wrapper.findAll('.q-item').map((row) => row.text());
      expect(rows.some((row) => row.includes('Butter'))).toBe(true);
      expect(rows[2]).toContain('Cheese');
    });

    it('reports an empty ingredient list', async () => {
      getRecipe.mockResolvedValue(recipe({ ingredients: [] }));
      const wrapper = await mountPage();

      expect(wrapper.text()).toContain('No ingredients.');
    });
  });

  describe('permissions', () => {
    it('offers edit and delete when the server says the caller may', async () => {
      getRecipe.mockResolvedValue(recipe({ canEdit: true, canDelete: true }));
      const wrapper = await mountPage();

      expect(wrapper.text()).toContain('Edit');
      expect(wrapper.text()).toContain('Delete');
    });

    it('hides edit and delete when the server says the caller may not', async () => {
      // Driven by the server's flags, not by comparing the owner id here: ownership stops meaning
      // editability once recipes can be shared.
      getRecipe.mockResolvedValue(recipe({ canEdit: false, canDelete: false }));
      const wrapper = await mountPage();

      expect(wrapper.text()).not.toContain('Edit');
      expect(wrapper.text()).not.toContain('Delete');
    });
  });

  describe('delete', () => {
    it('asks for confirmation before deleting', async () => {
      getRecipe.mockResolvedValue(recipe());
      const wrapper = await mountPage();

      await clickButton(wrapper, 'Delete');
      await flushPromises();

      expect(deleteRecipe).not.toHaveBeenCalled();
      expect(document.body.textContent).toContain('Delete this recipe?');
    });

    it('deletes and returns to the list once confirmed', async () => {
      getRecipe.mockResolvedValue(recipe());
      const wrapper = await mountPage();

      await clickButton(wrapper, 'Delete');
      await flushPromises();

      const dialogDelete = Array.from(document.body.querySelectorAll('button')).find(
        (button) => button.textContent?.trim() === 'Delete' && button.closest('.q-dialog'),
      );
      dialogDelete?.click();
      await flushPromises();
      // A second flush: the mutation resolving and the redirect it triggers are separate ticks.
      await flushPromises();

      expect(deleteRecipe.mock.calls[0]?.[0]).toBe('1');
      // The redirect waits on the mutation's cache invalidation, so poll rather than assume a tick.
      await vi.waitFor(() => expect(router.currentRoute.value.path).toBe('/recipes'));
    });
  });
});
