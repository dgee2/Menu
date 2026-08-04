import { beforeEach, describe, expect, it, vi } from 'vitest';
import { flushPromises, mount, type VueWrapper } from '@vue/test-utils';
import { Quasar } from 'quasar';
import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query';
import { createMemoryHistory, createRouter, type Router } from 'vue-router';
import { defineComponent } from 'vue';
import NewRecipeForm from './new-recipe-form.vue';
import type { RecipeDetail } from '@/services/recipe-api';

const postRecipe = vi.fn();

// Mocked at the API layer rather than the service layer, so the real
// useCreateRecipe() mutation (and therefore the real isPending/isError wiring
// the form renders from) is exercised by these tests.
vi.mock('@/services/recipe-api', () => ({
  useRecipeApi: () => ({
    postRecipe,
    putRecipe: vi.fn(),
    getRecipes: vi.fn(),
    getRecipe: vi.fn(),
    getIngredientUnits: vi.fn(),
  }),
}));

const createdRecipe = { id: 1, title: 'Lasagne' } as unknown as RecipeDetail;

const StubPage = defineComponent({ template: '<div />' });

const ERROR_MESSAGE = 'Failed to save recipe. Please try again.';

let router: Router;

const mountForm = async () => {
  router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: StubPage },
      { path: '/new-recipe', component: StubPage },
      { path: '/recipe/:recipeId', component: StubPage },
    ],
  });
  await router.push('/new-recipe');
  await router.isReady();

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  return mount(NewRecipeForm, {
    global: { plugins: [Quasar, [VueQueryPlugin, { queryClient }], router] },
    attachTo: document.body,
  });
};

/** Quasar renders the field label as a sibling div, not a `for`-linked <label>. */
const field = (wrapper: VueWrapper, label: string) => {
  const found = wrapper
    .findAll('.q-field')
    .find((candidate) => candidate.find('.q-field__label').text() === label);

  if (!found) {
    throw new Error(`No field labelled "${label}"`);
  }

  return found;
};

const fillField = async (wrapper: VueWrapper, label: string, value: string) => {
  await field(wrapper, label)
    .find<HTMLInputElement | HTMLTextAreaElement>('input, textarea')
    .setValue(value);
};

/** Fills the Nth (0-indexed) field sharing `label`, for repeated rows like step editors. */
const fillFieldAt = async (wrapper: VueWrapper, label: string, occurrence: number, value: string) => {
  const matches = wrapper
    .findAll('.q-field')
    .filter((candidate) => candidate.find('.q-field__label').text() === label);

  await matches[occurrence].find<HTMLInputElement | HTMLTextAreaElement>('input, textarea').setValue(value);
};

/**
 * Matches icon-only buttons by their `aria-label`, and labelled buttons by their visible text
 * (Quasar prepends the icon's ligature text, e.g. "add", so text is matched with `endsWith`).
 */
const clickButton = async (wrapper: VueWrapper, name: string) => {
  const byAriaLabel = wrapper.find(`[aria-label="${name}"]`);
  if (byAriaLabel.exists()) {
    await byAriaLabel.trigger('click');
    return;
  }

  const byText = wrapper.findAll('button').find((candidate) => candidate.text().endsWith(name));
  if (!byText) {
    throw new Error(`No button labelled "${name}"`);
  }
  await byText.trigger('click');
};

const submit = async (wrapper: VueWrapper) => {
  await wrapper.find('form').trigger('submit');
  await flushPromises();
};

/**
 * TanStack Query calls the mutation function with a second (context) argument,
 * so assert on the recipe payload alone.
 */
const submittedRecipe = (call = 0) => postRecipe.mock.calls[call]?.[0] as unknown;

describe('new-recipe-form', () => {
  beforeEach(() => {
    postRecipe.mockReset();
    postRecipe.mockResolvedValue(createdRecipe);
  });

  it('renders every recipe metadata field and the submit button', async () => {
    const wrapper = await mountForm();

    const labels = wrapper.findAll('.q-field__label').map((label) => label.text());
    expect(labels).toEqual([
      'Name',
      'Summary',
      'Yield',
      'Servings',
      'Prep time (minutes)',
      'Cook time (minutes)',
      'Total time (minutes)',
    ]);
    expect(wrapper.find('button[type="submit"]').text()).toBe('Save recipe');
  });

  it('renders Summary as a multi-line field and Yield as a single-line one', async () => {
    const wrapper = await mountForm();

    expect(field(wrapper, 'Summary').find('textarea').exists()).toBe(true);
    expect(field(wrapper, 'Yield').find('input').exists()).toBe(true);
  });

  it('renders the numeric fields as number inputs', async () => {
    const wrapper = await mountForm();

    for (const label of [
      'Servings',
      'Prep time (minutes)',
      'Cook time (minutes)',
      'Total time (minutes)',
    ]) {
      expect(field(wrapper, label).find('input').attributes('type')).toBe('number');
    }
  });

  it('does not show the error banner before a submit has failed', async () => {
    const wrapper = await mountForm();

    expect(wrapper.text()).not.toContain(ERROR_MESSAGE);
  });

  it('blocks submit and reports the error when the title is empty', async () => {
    const wrapper = await mountForm();

    await submit(wrapper);

    expect(postRecipe).not.toHaveBeenCalled();
    expect(wrapper.text()).toContain('Name is required');
  });

  it('blocks submit when the title is whitespace only', async () => {
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', '   ');
    await submit(wrapper);

    expect(postRecipe).not.toHaveBeenCalled();
    expect(wrapper.text()).toContain('Name is required');
  });

  it('blocks submit when servings is negative', async () => {
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Lasagne');
    await fillField(wrapper, 'Servings', '-5');
    await submit(wrapper);

    expect(postRecipe).not.toHaveBeenCalled();
    expect(wrapper.text()).toContain('Must be 0 or greater');
  });

  it('blocks submit when servings is not a whole number', async () => {
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Lasagne');
    await fillField(wrapper, 'Servings', '3.5');
    await submit(wrapper);

    expect(postRecipe).not.toHaveBeenCalled();
    expect(wrapper.text()).toContain('Must be a whole number');
  });

  it('blocks submit when a time field is negative', async () => {
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Lasagne');
    await fillField(wrapper, 'Prep time (minutes)', '-1');
    await submit(wrapper);

    expect(postRecipe).not.toHaveBeenCalled();
    expect(wrapper.text()).toContain('Must be 0 or greater');
  });

  it('accepts zero for the numeric fields', async () => {
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Lasagne');
    await fillField(wrapper, 'Servings', '0');
    await submit(wrapper);

    expect(submittedRecipe()).toMatchObject({ servings: 0 });
  });

  it('submits a title-only recipe with the optional fields left null', async () => {
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Lasagne');
    await submit(wrapper);

    expect(postRecipe).toHaveBeenCalledTimes(1);
    expect(submittedRecipe()).toEqual({
      title: 'Lasagne',
      summary: null,
      yieldText: null,
      servings: null,
      prepTimeMinutes: null,
      cookTimeMinutes: null,
      totalTimeMinutes: null,
      accessScope: 'Private',
      ingredients: [],
      steps: [],
    });
  });

  it('submits added steps with their edited fields and a recomputed sortOrder', async () => {
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Lasagne');
    await clickButton(wrapper, 'Add step');
    await clickButton(wrapper, 'Add step');

    await fillFieldAt(wrapper, 'Instructions', 0, 'Preheat the oven');
    await fillFieldAt(wrapper, 'Title', 0, 'Preheat');
    await fillFieldAt(wrapper, 'Duration (minutes)', 0, '10');
    await fillFieldAt(wrapper, 'Instructions', 1, 'Mix the batter');

    await submit(wrapper);

    expect(submittedRecipe()).toMatchObject({
      steps: [
        { instructionText: 'Preheat the oven', title: 'Preheat', durationMinutes: 10, sortOrder: 0 },
        { instructionText: 'Mix the batter', title: null, durationMinutes: null, sortOrder: 1 },
      ],
    });
  });

  it('recomputes step sortOrder after a reorder and does not leak the internal rowId', async () => {
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Lasagne');
    await clickButton(wrapper, 'Add step');
    await clickButton(wrapper, 'Add step');

    await fillFieldAt(wrapper, 'Instructions', 0, 'Preheat the oven');
    await fillFieldAt(wrapper, 'Instructions', 1, 'Mix the batter');

    await clickButton(wrapper, 'Move step down');
    await submit(wrapper);

    expect(submittedRecipe()).toMatchObject({
      steps: [
        { instructionText: 'Mix the batter', sortOrder: 0 },
        { instructionText: 'Preheat the oven', sortOrder: 1 },
      ],
    });
    for (const step of (submittedRecipe() as { steps: object[] }).steps) {
      expect(step).not.toHaveProperty('rowId');
    }
  });

  it('submits the populated metadata fields as numbers', async () => {
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Chocolate Cake');
    await fillField(wrapper, 'Summary', 'A rich, moist chocolate cake.');
    await fillField(wrapper, 'Yield', 'One 9-inch cake');
    await fillField(wrapper, 'Servings', '8');
    await fillField(wrapper, 'Prep time (minutes)', '20');
    await fillField(wrapper, 'Cook time (minutes)', '35');
    await fillField(wrapper, 'Total time (minutes)', '55');
    await submit(wrapper);

    expect(submittedRecipe()).toEqual({
      title: 'Chocolate Cake',
      summary: 'A rich, moist chocolate cake.',
      yieldText: 'One 9-inch cake',
      servings: 8,
      prepTimeMinutes: 20,
      cookTimeMinutes: 35,
      totalTimeMinutes: 55,
      accessScope: 'Private',
      ingredients: [],
      steps: [],
    });
  });

  it('navigates to the new recipe detail page on success', async () => {
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Lasagne');
    await submit(wrapper);

    expect(router.currentRoute.value.path).toBe('/recipe/1');
    expect(wrapper.text()).not.toContain(ERROR_MESSAGE);
  });

  it('shows the error banner and stays on the form when the mutation fails', async () => {
    postRecipe.mockRejectedValue(new Error('Failed to post recipe'));
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Lasagne');
    await submit(wrapper);

    expect(wrapper.text()).toContain(ERROR_MESSAGE);
    expect(router.currentRoute.value.path).toBe('/new-recipe');
  });

  it('does not leave the failed mutation as an unhandled rejection', async () => {
    postRecipe.mockRejectedValue(new Error('Failed to post recipe'));
    const unhandled = vi.fn();
    window.addEventListener('unhandledrejection', unhandled);

    const wrapper = await mountForm();
    await fillField(wrapper, 'Name', 'Lasagne');
    await submit(wrapper);
    await flushPromises();

    window.removeEventListener('unhandledrejection', unhandled);
    expect(unhandled).not.toHaveBeenCalled();
  });

  it('clears the error banner once a retried submit succeeds', async () => {
    postRecipe.mockRejectedValueOnce(new Error('Failed to post recipe'));
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Lasagne');
    await submit(wrapper);
    expect(wrapper.text()).toContain(ERROR_MESSAGE);

    await submit(wrapper);

    expect(wrapper.text()).not.toContain(ERROR_MESSAGE);
    expect(router.currentRoute.value.path).toBe('/recipe/1');
  });

  it('puts the submit button into its loading state while the mutation is in flight', async () => {
    let resolvePost: (recipe: RecipeDetail) => void = () => {};
    postRecipe.mockReturnValue(
      new Promise<RecipeDetail>((resolve) => {
        resolvePost = resolve;
      }),
    );
    const wrapper = await mountForm();

    await fillField(wrapper, 'Name', 'Lasagne');
    await submit(wrapper);

    expect(wrapper.find('button[type="submit"] .q-spinner').exists()).toBe(true);

    resolvePost(createdRecipe);
    await flushPromises();

    expect(wrapper.find('button[type="submit"] .q-spinner').exists()).toBe(false);
  });
});
