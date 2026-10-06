import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { flushPromises, mount, type VueWrapper } from '@vue/test-utils';
import { QSelect, Quasar } from 'quasar';
import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query';
import { createMemoryHistory, createRouter, type Router } from 'vue-router';
import { defineComponent, nextTick } from 'vue';
import RecipeForm from './recipe-form.vue';
import { ApiError } from '@/services/api-error';
import type { RecipeDetail } from '@/services/recipe-api';

const postRecipe = vi.fn();
const putRecipe = vi.fn();

// Mocked at the API layer rather than the service layer, so the real
// useCreateRecipe()/useUpdateRecipe() mutations (and therefore the real isPending/error wiring
// the form renders from) are exercised by these tests.
vi.mock('@/services/recipe-api', () => ({
  useRecipeApi: () => ({
    postRecipe,
    putRecipe,
    deleteRecipe: vi.fn(),
    restoreRecipe: vi.fn(),
    getRecipes: vi.fn(),
    getRecipe: vi.fn(),
    getIngredientUnits: vi.fn(),
  }),
}));

const createdRecipe = { id: 1, title: 'Lasagne' } as unknown as RecipeDetail;

const existingRecipe = {
  id: 7,
  title: 'Existing Lasagne',
  accessScope: 'AuthenticatedUsers',
  summary: 'Layers.',
  yieldText: 'One tray',
  servings: 6,
  prepTimeMinutes: 20,
  cookTimeMinutes: 40,
  totalTimeMinutes: null,
  canEdit: true,
  canDelete: true,
  ingredients: [
    {
      sortOrder: 0,
      ingredientText: 'Pasta',
      measureText: '250g',
      sectionTitle: 'For the layers',
      preparationText: null,
      isOptional: false,
      // The form does not expose these, but the API accepts them and an edit must not drop them.
      amount: 250,
      unitText: 'g',
      canonicalIngredientId: 17,
      canonicalUnitId: 4,
    },
  ],
  steps: [{ sortOrder: 0, instructionText: 'Assemble', title: null, durationMinutes: null }],
} as unknown as RecipeDetail;

const StubPage = defineComponent({ template: '<div />' });

const ERROR_MESSAGE = 'Failed to save recipe. Please try again.';

let router: Router;

/**
 * Mounted through a `<router-view>` rather than directly, because `onBeforeRouteLeave` only
 * registers against a matched route record — mounting the form standalone silently disables the
 * unsaved-changes guard these tests are here to check.
 */
const RouterHost = defineComponent({ template: '<router-view />' });

let editRecipe = existingRecipe;
const EditHost = defineComponent({
  components: { RecipeForm },
  setup: () => ({ recipe: editRecipe }),
  template: '<recipe-form :initial-recipe="recipe" />',
});

// Tracked so every mount is torn down between tests: the form registers a `beforeunload` listener
// on `window`, and a wrapper left mounted keeps answering for the next test's dispatch.
const mounted: VueWrapper[] = [];

const mountAt = async (path: string) => {
  router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: StubPage },
      { path: '/recipes', component: StubPage },
      { path: '/new-recipe', component: RecipeForm },
      { path: '/recipe/:recipeId', component: StubPage },
      { path: '/recipe/:recipeId/edit', component: EditHost },
    ],
  });
  await router.push(path);
  await router.isReady();

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  const wrapper = mount(RouterHost, {
    global: { plugins: [Quasar, [VueQueryPlugin, { queryClient }], router] },
    attachTo: document.body,
  });
  mounted.push(wrapper);

  return wrapper;
};

const mountForm = () => mountAt('/new-recipe');
const mountEditForm = () => mountAt('/recipe/7/edit');

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

const fields = (wrapper: VueWrapper, label: string) =>
  wrapper
    .findAll('.q-field')
    .filter((candidate) => candidate.find('.q-field__label').text() === label);

const fillField = async (wrapper: VueWrapper, label: string, value: string) => {
  await field(wrapper, label)
    .find<HTMLInputElement | HTMLTextAreaElement>('input, textarea')
    .setValue(value);
};

/**
 * Picks a dropdown option by label. The QSelect menu is portalled outside the wrapper, so the
 * selection is emitted from the QSelect itself; the click-through path is covered by the story.
 */
const selectOption = async (wrapper: VueWrapper, fieldLabel: string, optionLabel: string) => {
  const select = field(wrapper, fieldLabel).findComponent(QSelect);
  const options = select.props('options') as { label: string; value: string }[];
  const option = options.find((candidate) => candidate.label === optionLabel);

  if (!option) {
    throw new Error(`No option labelled "${optionLabel}" in field "${fieldLabel}"`);
  }

  select.vm.$emit('update:modelValue', option);
  await nextTick();
};

/** Fills the Nth (0-indexed) field sharing `label`, for repeated rows like step editors. */
const fillFieldAt = async (
  wrapper: VueWrapper,
  label: string,
  occurrence: number,
  value: string,
) => {
  await fields(wrapper, label)
    [occurrence].find<HTMLInputElement | HTMLTextAreaElement>('input, textarea')
    .setValue(value);
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
/** The API layer's putRecipe takes (recipeId, recipe) positionally. */
const updatedRecipe = (call = 0) => putRecipe.mock.calls[call]?.[1] as unknown;

describe('recipe-form', () => {
  beforeEach(() => {
    editRecipe = existingRecipe;
    postRecipe.mockReset();
    postRecipe.mockResolvedValue(createdRecipe);
    putRecipe.mockReset();
    putRecipe.mockResolvedValue({ ...existingRecipe });
  });

  afterEach(() => {
    while (mounted.length) mounted.pop()?.unmount();
    vi.unstubAllGlobals();
  });

  describe('create mode', () => {
    it('renders every recipe metadata field and the submit button', async () => {
      const wrapper = await mountForm();

      const labels = wrapper.findAll('.q-field__label').map((label) => label.text());
      expect(labels.slice(0, 8)).toEqual([
        'Name',
        'Visibility',
        'Summary',
        'Yield',
        'Servings',
        'Prep time (minutes)',
        'Cook time (minutes)',
        'Total time (minutes)',
      ]);
      expect(wrapper.find('button[type="submit"]').text()).toBe('Save recipe');
    });

    it('seeds one blank ingredient row and one blank step row', async () => {
      const wrapper = await mountForm();

      expect(fields(wrapper, 'Ingredient')).toHaveLength(1);
      expect(fields(wrapper, 'Instructions')).toHaveLength(1);
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

    it('submits a title-only recipe, dropping the seeded blank rows', async () => {
      // The "zero ingredients and zero steps is a valid recipe" guarantee has to survive seeding.
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

    it('still blocks submit when a row is only partly filled in', async () => {
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await fillField(wrapper, 'Ingredient', 'Flour');
      await submit(wrapper);

      expect(postRecipe).not.toHaveBeenCalled();
      expect(wrapper.text()).toContain('Measure is required');
    });

    it('defaults the submitted visibility to Private', async () => {
      const wrapper = await mountForm();

      expect(field(wrapper, 'Visibility').find('.q-field__native').text()).toBe('Private');

      await fillField(wrapper, 'Name', 'Lasagne');
      await submit(wrapper);

      expect(submittedRecipe()).toMatchObject({ accessScope: 'Private' });
    });

    it('submits the selected visibility scope', async () => {
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await selectOption(wrapper, 'Visibility', 'Visible to all Menu users');
      await submit(wrapper);

      expect(submittedRecipe()).toMatchObject({ accessScope: 'AuthenticatedUsers' });
    });

    it('submits added steps with their edited fields and a recomputed sortOrder', async () => {
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await clickButton(wrapper, 'Add step');

      await fillFieldAt(wrapper, 'Instructions', 0, 'Preheat the oven');
      await fillFieldAt(wrapper, 'Title', 0, 'Preheat');
      await fillFieldAt(wrapper, 'Duration (minutes)', 0, '10');
      await fillFieldAt(wrapper, 'Instructions', 1, 'Mix the batter');

      await submit(wrapper);

      expect(submittedRecipe()).toMatchObject({
        steps: [
          {
            instructionText: 'Preheat the oven',
            title: 'Preheat',
            durationMinutes: 10,
            sortOrder: 0,
          },
          { instructionText: 'Mix the batter', title: null, durationMinutes: null, sortOrder: 1 },
        ],
      });
    });

    it('recomputes step sortOrder after a reorder and does not leak the internal rowId', async () => {
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
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

    it('submits added ingredients with their edited fields and a recomputed sortOrder', async () => {
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await clickButton(wrapper, 'Add ingredient');

      await fillFieldAt(wrapper, 'Ingredient', 0, 'Flour');
      await fillFieldAt(wrapper, 'Measure', 0, '200g');
      await fillFieldAt(wrapper, 'Preparation', 0, 'sifted');
      await fillFieldAt(wrapper, 'Ingredient', 1, 'Sugar');
      await fillFieldAt(wrapper, 'Measure', 1, '1 cup');

      await submit(wrapper);

      expect(submittedRecipe()).toMatchObject({
        ingredients: [
          {
            ingredientText: 'Flour',
            measureText: '200g',
            preparationText: 'sifted',
            isOptional: false,
            sectionTitle: null,
            sortOrder: 0,
          },
          { ingredientText: 'Sugar', measureText: '1 cup', sectionTitle: null, sortOrder: 1 },
        ],
      });
    });

    it('recomputes ingredient sortOrder after a reorder and does not leak the internal rowId', async () => {
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await clickButton(wrapper, 'Add ingredient');

      await fillFieldAt(wrapper, 'Ingredient', 0, 'Flour');
      await fillFieldAt(wrapper, 'Measure', 0, '200g');
      await fillFieldAt(wrapper, 'Ingredient', 1, 'Sugar');
      await fillFieldAt(wrapper, 'Measure', 1, '1 cup');

      await clickButton(wrapper, 'Move ingredient down');
      await submit(wrapper);

      expect(submittedRecipe()).toMatchObject({
        ingredients: [
          { ingredientText: 'Sugar', sortOrder: 0 },
          { ingredientText: 'Flour', sortOrder: 1 },
        ],
      });
      for (const ingredient of (submittedRecipe() as { ingredients: object[] }).ingredients) {
        expect(ingredient).not.toHaveProperty('rowId');
      }
    });

    it('removes an ingredient row and submits only what is left', async () => {
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await clickButton(wrapper, 'Add ingredient');

      await fillFieldAt(wrapper, 'Ingredient', 0, 'Flour');
      await fillFieldAt(wrapper, 'Measure', 0, '200g');
      await fillFieldAt(wrapper, 'Ingredient', 1, 'Sugar');
      await fillFieldAt(wrapper, 'Measure', 1, '1 cup');

      await clickButton(wrapper, 'Remove ingredient');
      await submit(wrapper);

      expect(submittedRecipe()).toMatchObject({
        ingredients: [{ ingredientText: 'Sugar', sortOrder: 0 }],
      });
    });

    it('derives section titles when a section is added and renamed', async () => {
      const wrapper = await mountForm();

      await fillFieldAt(wrapper, 'Ingredient', 0, 'Flour');
      await fillFieldAt(wrapper, 'Measure', 0, '200g');
      await clickButton(wrapper, 'Add section');
      await fillField(wrapper, 'Section heading', 'For the sponge');
      const namedSection = wrapper.findAll('.ingredient-section')[1];
      await namedSection
        .findAll('button')
        .find((button) => button.text().endsWith('Add ingredient'))!
        .trigger('click');
      await fillFieldAt(wrapper, 'Ingredient', 1, 'Sugar');
      await fillFieldAt(wrapper, 'Measure', 1, '1 cup');
      await fillField(wrapper, 'Section heading', 'For the topping');
      await fillField(wrapper, 'Name', 'Cake');
      await submit(wrapper);

      expect(
        (submittedRecipe() as { ingredients: { sectionTitle: string | null }[] }).ingredients,
      ).toMatchObject([{ sectionTitle: null }, { sectionTitle: 'For the topping' }]);
    });

    it('rejects a blank heading instead of saving its ingredients as unsectioned', async () => {
      const wrapper = await mountForm();
      await fillField(wrapper, 'Name', 'Cake');
      await clickButton(wrapper, 'Add section');
      const section = wrapper.findAll('.ingredient-section')[1];
      await section
        .findAll('button')
        .find((button) => button.text().endsWith('Add ingredient'))!
        .trigger('click');
      await fillField(wrapper, 'Ingredient', 'Flour');
      await fillField(wrapper, 'Measure', '200g');
      await fillField(wrapper, 'Section heading', '   ');
      await submit(wrapper);

      expect(wrapper.text()).toContain('Section heading is required');
      expect(postRecipe).not.toHaveBeenCalled();
    });

    it('requires a real ingredient in each new named section before saving', async () => {
      const wrapper = await mountForm();
      await fillField(wrapper, 'Name', 'Cake');
      await clickButton(wrapper, 'Add section');
      await fillField(wrapper, 'Section heading', 'Sauce');
      await submit(wrapper);

      expect(wrapper.text()).toContain('Add an ingredient or remove this section');
      expect(postRecipe).not.toHaveBeenCalled();

      await wrapper
        .findAll('.ingredient-section')[1]
        .findAll('button')
        .find((button) => button.text().endsWith('Add ingredient'))!
        .trigger('click');
      await fillFieldAt(wrapper, 'Ingredient', 1, 'Tomatoes');
      await fillFieldAt(wrapper, 'Measure', 1, '2 cups');
      await submit(wrapper);

      expect(submittedRecipe()).toMatchObject({
        ingredients: [{ ingredientText: 'Tomatoes', sectionTitle: 'Sauce' }],
      });
    });

    it('labels every move target by position, including repeated headings', async () => {
      const wrapper = await mountForm();
      for (const heading of ['Sauce', 'Filling', 'Sauce']) {
        await clickButton(wrapper, 'Add section');
        await fillFieldAt(
          wrapper,
          'Section heading',
          fields(wrapper, 'Section heading').length - 1,
          heading,
        );
      }

      const options = field(wrapper, 'Move ingredient to section')
        .findComponent(QSelect)
        .props('options') as { label: string }[];
      expect(options.map((option) => option.label)).toEqual([
        '1. Unsectioned',
        '2. Sauce',
        '3. Filling',
        '4. Sauce',
      ]);
    });

    it('keeps move target labels unique when a heading resembles a generated label', async () => {
      const wrapper = await mountForm();
      for (const heading of ['Sauce', 'Sauce (section 1)', 'Sauce']) {
        await clickButton(wrapper, 'Add section');
        await fillFieldAt(
          wrapper,
          'Section heading',
          fields(wrapper, 'Section heading').length - 1,
          heading,
        );
      }

      const options = field(wrapper, 'Move ingredient to section')
        .findComponent(QSelect)
        .props('options') as { label: string }[];
      const labels = options.map((option) => option.label);
      expect(labels).toEqual(['1. Unsectioned', '2. Sauce', '3. Sauce (section 1)', '4. Sauce']);
      expect(new Set(labels).size).toBe(labels.length);
    });

    it('rejects adjacent matching headings after a section reorder', async () => {
      const wrapper = await mountForm();
      await fillField(wrapper, 'Name', 'Cake');
      for (const heading of ['Sauce', 'Filling', 'Sauce']) {
        await clickButton(wrapper, 'Add section');
        const sectionIndex = fields(wrapper, 'Section heading').length - 1;
        await fillFieldAt(wrapper, 'Section heading', sectionIndex, heading);
        const section = wrapper.findAll('.ingredient-section')[sectionIndex + 1];
        await section
          .findAll('button')
          .find((button) => button.text().endsWith('Add ingredient'))!
          .trigger('click');
        const ingredientIndex = fields(wrapper, 'Ingredient').length - 1;
        await fillFieldAt(wrapper, 'Ingredient', ingredientIndex, heading);
        await fillFieldAt(wrapper, 'Measure', ingredientIndex, '1 cup');
      }
      await wrapper
        .findAll('.ingredient-section')[3]
        .find('[aria-label="Move section up"]')
        .trigger('click');
      await submit(wrapper);

      expect(wrapper.text()).toContain('Adjacent sections need different headings');
      expect(postRecipe).not.toHaveBeenCalled();
    });

    it('rejects matching headings separated only by a section with no saved rows', async () => {
      const wrapper = await mountForm();
      await fillField(wrapper, 'Name', 'Cake');
      for (const heading of ['Sauce', 'Filling', 'Sauce']) {
        await clickButton(wrapper, 'Add section');
        const sectionIndex = fields(wrapper, 'Section heading').length - 1;
        await fillFieldAt(wrapper, 'Section heading', sectionIndex, heading);
        if (heading === 'Filling') continue;
        const section = wrapper.findAll('.ingredient-section')[sectionIndex + 1];
        await section
          .findAll('button')
          .find((button) => button.text().endsWith('Add ingredient'))!
          .trigger('click');
        const ingredientIndex = fields(wrapper, 'Ingredient').length - 1;
        await fillFieldAt(wrapper, 'Ingredient', ingredientIndex, heading);
        await fillFieldAt(wrapper, 'Measure', ingredientIndex, '1 cup');
      }
      await submit(wrapper);

      expect(wrapper.text()).toContain('Adjacent sections need different headings');
      expect(postRecipe).not.toHaveBeenCalled();
    });

    it('moves a row between sections and recomputes its sort order', async () => {
      const wrapper = await mountForm();
      await fillField(wrapper, 'Name', 'Cake');
      await fillFieldAt(wrapper, 'Ingredient', 0, 'Flour');
      await fillFieldAt(wrapper, 'Measure', 0, '200g');
      await clickButton(wrapper, 'Add ingredient');
      await fillFieldAt(wrapper, 'Ingredient', 1, 'Sugar');
      await fillFieldAt(wrapper, 'Measure', 1, '1 cup');
      await clickButton(wrapper, 'Add section');
      await fillField(wrapper, 'Section heading', 'Topping');
      const namedSection = wrapper.findAll('.ingredient-section')[1];
      await namedSection
        .findAll('button')
        .find((button) => button.text().endsWith('Add ingredient'))!
        .trigger('click');
      await fillFieldAt(wrapper, 'Ingredient', 2, 'Cream');
      await fillFieldAt(wrapper, 'Measure', 2, '100ml');

      const targetId = namedSection.attributes('data-section-id');
      wrapper
        .findAll('.ingredient-row')[1]
        .findComponent(QSelect)
        .vm.$emit('update:modelValue', targetId);
      await nextTick();
      await submit(wrapper);

      expect(
        (
          submittedRecipe() as {
            ingredients: {
              ingredientText: string;
              sectionTitle: string | null;
              sortOrder: number;
            }[];
          }
        ).ingredients,
      ).toMatchObject([
        { ingredientText: 'Flour', sectionTitle: null, sortOrder: 0 },
        { ingredientText: 'Cream', sectionTitle: 'Topping', sortOrder: 1 },
        { ingredientText: 'Sugar', sectionTitle: 'Topping', sortOrder: 2 },
      ]);
    });

    it('reparents rows to the preceding section when a section is removed', async () => {
      const wrapper = await mountForm();
      await clickButton(wrapper, 'Add section');
      await fillField(wrapper, 'Section heading', 'Base');
      const base = wrapper.findAll('.ingredient-section')[1];
      await base
        .findAll('button')
        .find((button) => button.text().endsWith('Add ingredient'))!
        .trigger('click');
      await fillFieldAt(wrapper, 'Ingredient', 1, 'Flour');
      await fillFieldAt(wrapper, 'Measure', 1, '200g');
      await clickButton(wrapper, 'Add section');
      await fillFieldAt(wrapper, 'Section heading', 1, 'Topping');
      const topping = wrapper.findAll('.ingredient-section')[2];
      await topping
        .findAll('button')
        .find((button) => button.text().endsWith('Add ingredient'))!
        .trigger('click');
      await fillFieldAt(wrapper, 'Ingredient', 2, 'Sugar');
      await fillFieldAt(wrapper, 'Measure', 2, '1 cup');
      await topping.find('[aria-label="Remove section"]').trigger('click');
      await fillField(wrapper, 'Name', 'Cake');
      await submit(wrapper);

      expect(
        (
          submittedRecipe() as {
            ingredients: {
              ingredientText: string;
              sectionTitle: string | null;
              sortOrder: number;
            }[];
          }
        ).ingredients,
      ).toMatchObject([
        { ingredientText: 'Flour', sectionTitle: 'Base', sortOrder: 0 },
        { ingredientText: 'Sugar', sectionTitle: 'Base', sortOrder: 1 },
      ]);
    });

    it('reorders whole sections without changing their row membership', async () => {
      const wrapper = await mountForm();
      await clickButton(wrapper, 'Add section');
      await fillField(wrapper, 'Section heading', 'First');
      await clickButton(wrapper, 'Add section');
      await fillFieldAt(wrapper, 'Section heading', 1, 'Second');
      await wrapper
        .findAll('.ingredient-section')[2]
        .find('[aria-label="Move section up"]')
        .trigger('click');

      expect(
        fields(wrapper, 'Section heading').map((heading) => heading.find('input').element.value),
      ).toEqual(['Second', 'First']);
    });

    it('drags a row into a section and reorders the sections', async () => {
      const wrapper = await mountForm();
      await fillField(wrapper, 'Name', 'Cake');
      await fillFieldAt(wrapper, 'Ingredient', 0, 'Flour');
      await fillFieldAt(wrapper, 'Measure', 0, '200g');
      await clickButton(wrapper, 'Add section');
      await fillField(wrapper, 'Section heading', 'Base');
      const base = wrapper.findAll('.ingredient-section')[1];
      await base
        .findAll('button')
        .find((button) => button.text().endsWith('Add ingredient'))!
        .trigger('click');
      await fillFieldAt(wrapper, 'Ingredient', 1, 'Butter');
      await fillFieldAt(wrapper, 'Measure', 1, '1 tbsp');
      await clickButton(wrapper, 'Add section');
      await fillFieldAt(wrapper, 'Section heading', 1, 'Topping');

      expect(wrapper.find('.ingredient-row').attributes('draggable')).toBeUndefined();
      expect(wrapper.find('.ingredient-section .row').attributes('draggable')).toBeUndefined();
      await wrapper.findAll('.ingredient-row')[0].find('[draggable="true"]').trigger('dragstart');
      await wrapper.findAll('.ingredient-section')[2].trigger('drop');
      const sections = wrapper.findAll('.ingredient-section');
      await sections[2].find('[draggable="true"]').trigger('dragstart');
      await sections[1].trigger('drop');
      expect(
        fields(wrapper, 'Section heading').map((heading) => heading.find('input').element.value),
      ).toEqual(['Topping', 'Base']);
      await submit(wrapper);
      expect(
        (submittedRecipe() as { ingredients: { sectionTitle: string; sortOrder: number }[] })
          .ingredients,
      ).toMatchObject([
        { sectionTitle: 'Topping', sortOrder: 0 },
        { sectionTitle: 'Base', sortOrder: 1 },
      ]);
    });

    it('drops ingredients after lower targets and before upper targets in the same section', async () => {
      const wrapper = await mountForm();
      await fillField(wrapper, 'Name', 'Cake');
      for (const [index, ingredient] of ['Flour', 'Sugar', 'Butter'].entries()) {
        if (index > 0) await clickButton(wrapper, 'Add ingredient');
        await fillFieldAt(wrapper, 'Ingredient', index, ingredient);
        await fillFieldAt(wrapper, 'Measure', index, '1 cup');
      }

      await wrapper.findAll('.ingredient-row')[0].find('[draggable="true"]').trigger('dragstart');
      await wrapper.findAll('.ingredient-row')[1].trigger('drop');
      expect(fields(wrapper, 'Ingredient').map((item) => item.find('input').element.value)).toEqual(
        ['Sugar', 'Flour', 'Butter'],
      );

      await wrapper.findAll('.ingredient-row')[2].find('[draggable="true"]').trigger('dragstart');
      await wrapper.findAll('.ingredient-row')[1].trigger('drop');
      await submit(wrapper);
      expect(
        (submittedRecipe() as { ingredients: { ingredientText: string; sortOrder: number }[] })
          .ingredients,
      ).toMatchObject([
        { ingredientText: 'Sugar', sortOrder: 0 },
        { ingredientText: 'Butter', sortOrder: 1 },
        { ingredientText: 'Flour', sortOrder: 2 },
      ]);
    });

    it('sets drag data for ingredient and section handles', async () => {
      const wrapper = await mountForm();
      await clickButton(wrapper, 'Add section');
      const setData = vi.fn();
      const transfer = { setData, effectAllowed: 'none' } as unknown as DataTransfer;

      await wrapper
        .find('.ingredient-row [draggable="true"]')
        .trigger('dragstart', { dataTransfer: transfer });
      expect(setData).toHaveBeenCalledWith('text/plain', expect.stringMatching(/.+/));
      expect(transfer.effectAllowed).toBe('move');

      setData.mockClear();
      await wrapper
        .findAll('.ingredient-section')[1]
        .find('[draggable="true"]')
        .trigger('dragstart', { dataTransfer: transfer });
      expect(setData).toHaveBeenCalledWith(
        'text/plain',
        wrapper.findAll('.ingredient-section')[1].attributes('data-section-id'),
      );
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
      postRecipe.mockRejectedValue(new Error('boom'));
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await submit(wrapper);

      expect(wrapper.text()).toContain(ERROR_MESSAGE);
      expect(router.currentRoute.value.path).toBe('/new-recipe');
    });

    it('does not leave the failed mutation as an unhandled rejection', async () => {
      postRecipe.mockRejectedValue(new Error('boom'));
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
      postRecipe.mockRejectedValueOnce(new Error('boom'));
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

  describe('duplicate titles', () => {
    it('reports a 409 against the title field rather than the generic banner', async () => {
      // A banner reading "please try again" is actively wrong here: retrying the same title can
      // never succeed, and the message has to point at the field the user must change.
      postRecipe.mockRejectedValue(
        new ApiError('conflict', 409, {
          detail: "A recipe titled 'Lasagne' already exists.",
          status: 409,
        }),
      );
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await submit(wrapper);

      expect(wrapper.text()).toContain("A recipe titled 'Lasagne' already exists.");
      expect(wrapper.text()).not.toContain(ERROR_MESSAGE);
    });

    it('clears the conflict message once the title is changed', async () => {
      postRecipe.mockRejectedValue(new ApiError('conflict', 409, { detail: 'Already exists.' }));
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await submit(wrapper);
      expect(wrapper.text()).toContain('Already exists.');

      await fillField(wrapper, 'Name', 'Lasagne 2');
      await nextTick();

      expect(wrapper.text()).not.toContain('Already exists.');
    });

    it('surfaces a non-conflict problem detail in the banner', async () => {
      postRecipe.mockRejectedValue(
        new ApiError('bad request', 400, { detail: 'Servings too big.' }),
      );
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await submit(wrapper);

      expect(wrapper.text()).toContain('Servings too big.');
    });
  });

  describe('edit mode', () => {
    it('preserves distinct raw section titles during an unrelated edit', async () => {
      editRecipe = {
        ...existingRecipe,
        ingredients: [
          { ...existingRecipe.ingredients[0], sectionTitle: 'Sauce' },
          {
            ...existingRecipe.ingredients[0],
            ingredientText: 'Salt',
            sortOrder: 1,
            sectionTitle: ' Sauce ',
          },
        ],
      };
      const wrapper = await mountEditForm();

      expect(
        fields(wrapper, 'Section heading').map((field) => field.find('input').element.value),
      ).toEqual(['Sauce', ' Sauce ']);
      await fillField(wrapper, 'Name', 'Renamed Lasagne');
      await submit(wrapper);

      expect(putRecipe).toHaveBeenCalledTimes(1);
      expect(updatedRecipe()).toMatchObject({
        ingredients: [{ sectionTitle: 'Sauce' }, { sectionTitle: ' Sauce ' }],
      });
    });

    it('blocks adjacent unsectioned containers after a section reorder', async () => {
      editRecipe = {
        ...existingRecipe,
        ingredients: [
          { ...existingRecipe.ingredients[0], sectionTitle: null },
          {
            ...existingRecipe.ingredients[0],
            ingredientText: 'Tomatoes',
            sortOrder: 1,
            sectionTitle: 'Sauce',
          },
          {
            ...existingRecipe.ingredients[0],
            ingredientText: 'Salt',
            sortOrder: 2,
            sectionTitle: null,
          },
        ],
      };
      const wrapper = await mountEditForm();

      await wrapper
        .findAll('.ingredient-section')[1]
        .find('[aria-label="Move section down"]')
        .trigger('click');
      await submit(wrapper);

      expect(wrapper.text()).toContain('Move adjacent unsectioned ingredients into one group');
      expect(putRecipe).not.toHaveBeenCalled();
    });

    it('preserves a later unsectioned ingredient run during an unrelated edit', async () => {
      editRecipe = {
        ...existingRecipe,
        ingredients: [
          { ...existingRecipe.ingredients[0], sectionTitle: 'Sauce' },
          {
            ...existingRecipe.ingredients[0],
            ingredientText: 'Salt',
            sortOrder: 1,
            sectionTitle: null,
          },
        ],
      };
      const wrapper = await mountEditForm();

      expect(fields(wrapper, 'Section heading')).toHaveLength(1);
      expect(wrapper.findAll('.ingredient-section')[2].text()).toContain('Unsectioned ingredients');
      await fillField(wrapper, 'Name', 'Renamed Lasagne');
      await submit(wrapper);

      expect(putRecipe).toHaveBeenCalledTimes(1);
      expect(updatedRecipe()).toMatchObject({
        ingredients: [
          { ingredientText: 'Pasta', sectionTitle: 'Sauce', sortOrder: 0 },
          { ingredientText: 'Salt', sectionTitle: null, sortOrder: 1 },
        ],
      });
    });

    it('populates every field from the recipe being edited', async () => {
      const wrapper = await mountEditForm();

      expect(field(wrapper, 'Name').find('input').element.value).toBe('Existing Lasagne');
      expect(field(wrapper, 'Summary').find('textarea').element.value).toBe('Layers.');
      expect(field(wrapper, 'Yield').find('input').element.value).toBe('One tray');
      expect(field(wrapper, 'Servings').find('input').element.value).toBe('6');
      expect(field(wrapper, 'Visibility').find('.q-field__native').text()).toBe(
        'Visible to all Menu users',
      );
      expect(wrapper.find('button[type="submit"]').text()).toBe('Save changes');
    });

    it('seeds no extra rows, showing only the recipeitself', async () => {
      const wrapper = await mountEditForm();

      expect(fields(wrapper, 'Ingredient')).toHaveLength(1);
      expect(fields(wrapper, 'Instructions')).toHaveLength(1);
      expect(fields(wrapper, 'Ingredient')[0].find('input').element.value).toBe('Pasta');
    });

    it('leaves an unset total time unset rather than saving the derived value', async () => {
      // Populating the input would make derived indistinguishable from explicit, and would turn
      // "clear it to go back to derived" into what looks like data loss.
      const wrapper = await mountEditForm();

      expect(field(wrapper, 'Total time (minutes)').find('input').element.value).toBe('');
      expect(field(wrapper, 'Total time (minutes)').text()).toContain('Calculated: 60 min');

      await submit(wrapper);

      expect(updatedRecipe()).toMatchObject({ totalTimeMinutes: null });
    });

    it('PUTs to the recipe being edited and navigates back to it', async () => {
      const wrapper = await mountEditForm();

      await fillField(wrapper, 'Name', 'Renamed Lasagne');
      await submit(wrapper);

      expect(putRecipe).toHaveBeenCalledTimes(1);
      expect(putRecipe.mock.calls[0]?.[0]).toBe('7');
      expect(updatedRecipe()).toMatchObject({ title: 'Renamed Lasagne' });
      expect(router.currentRoute.value.path).toBe('/recipe/7');
    });

    it('carries structured ingredient fields through an edit untouched', async () => {
      // UpsertRecipeIngredientsAsync replaces the whole collection, so anything the payload omits is
      // deleted. The form does not expose amount/unit/canonical ids, but it must not destroy them.
      const wrapper = await mountEditForm();

      await fillField(wrapper, 'Name', 'Renamed Lasagne');
      await submit(wrapper);

      expect(updatedRecipe()).toMatchObject({
        ingredients: [
          {
            ingredientText: 'Pasta',
            amount: 250,
            unitText: 'g',
            canonicalIngredientId: 17,
            canonicalUnitId: 4,
          },
        ],
      });
    });

    it('does not call the create endpoint', async () => {
      const wrapper = await mountEditForm();

      await submit(wrapper);

      expect(postRecipe).not.toHaveBeenCalled();
    });
  });

  describe('unsaved changes guard', () => {
    it('lets an untouched form be left without prompting', async () => {
      const confirm = vi.fn(() => true);
      vi.stubGlobal('confirm', confirm);
      await mountForm();

      await router.push('/recipes');

      expect(confirm).not.toHaveBeenCalled();
      expect(router.currentRoute.value.path).toBe('/recipes');
    });

    it('does not treat the seeded blank rows as an edit', async () => {
      // The baseline is captured after seeding; capturing it before would make every freshly
      // opened create form dirty.
      const confirm = vi.fn(() => true);
      vi.stubGlobal('confirm', confirm);
      const wrapper = await mountForm();

      await clickButton(wrapper, 'Add ingredient');
      await clickButton(wrapper, 'Remove ingredient');
      await router.push('/recipes');

      expect(confirm).not.toHaveBeenCalled();
    });

    it('warns before discarding an empty named section and clears after removing it', async () => {
      const confirm = vi.fn(() => false);
      vi.stubGlobal('confirm', confirm);
      const wrapper = await mountForm();

      await clickButton(wrapper, 'Add section');
      await fillField(wrapper, 'Section heading', 'Sauce');
      await router.push('/recipes');
      expect(confirm).toHaveBeenCalledTimes(1);
      expect(router.currentRoute.value.path).toBe('/new-recipe');

      await clickButton(wrapper, 'Remove section');
      await router.push('/recipes');
      expect(confirm).toHaveBeenCalledTimes(1);
      expect(router.currentRoute.value.path).toBe('/recipes');
    });

    it('prompts before leaving with unsaved edits, and stays put when declined', async () => {
      const confirm = vi.fn(() => false);
      vi.stubGlobal('confirm', confirm);
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await router.push('/recipes');

      expect(confirm).toHaveBeenCalledTimes(1);
      expect(router.currentRoute.value.path).toBe('/new-recipe');
    });

    it('leaves when the prompt is accepted', async () => {
      vi.stubGlobal(
        'confirm',
        vi.fn(() => true),
      );
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await router.push('/recipes');

      expect(router.currentRoute.value.path).toBe('/recipes');
    });

    it('does not prompt after a successful save', async () => {
      // The guard has to be disarmed before the redirect, or saving prompts the user to discard
      // the changes they have just saved.
      const confirm = vi.fn(() => true);
      vi.stubGlobal('confirm', confirm);
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await submit(wrapper);

      expect(confirm).not.toHaveBeenCalled();
      expect(router.currentRoute.value.path).toBe('/recipe/1');
    });

    it('still prompts after a failed save', async () => {
      postRecipe.mockRejectedValue(new Error('boom'));
      const confirm = vi.fn(() => true);
      vi.stubGlobal('confirm', confirm);
      const wrapper = await mountForm();

      await fillField(wrapper, 'Name', 'Lasagne');
      await submit(wrapper);
      await router.push('/recipes');

      expect(confirm).toHaveBeenCalledTimes(1);
    });

    it('warns on tab close when dirty', async () => {
      // onBeforeRouteLeave never sees a tab close or a reload, so the browser-level guard is
      // separately necessary.
      const wrapper = await mountForm();
      await fillField(wrapper, 'Name', 'Lasagne');

      const event = new Event('beforeunload', { cancelable: true });
      const setReturnValue = vi.fn();
      Object.defineProperty(event, 'returnValue', { set: setReturnValue, get: () => undefined });
      window.dispatchEvent(event);

      expect(event.defaultPrevented).toBe(true);
      // Chromium and WebKit gate the prompt on returnValue as well as preventDefault(), so both
      // have to happen or the warning silently does nothing in those browsers.
      expect(setReturnValue).toHaveBeenCalled();
    });

    it('does not warn on tab close when clean', async () => {
      await mountForm();

      const event = new Event('beforeunload', { cancelable: true });
      window.dispatchEvent(event);

      expect(event.defaultPrevented).toBe(false);
    });

    it('stops warning on tab close once the form is unmounted', async () => {
      const wrapper = await mountForm();
      await fillField(wrapper, 'Name', 'Lasagne');
      wrapper.unmount();

      const event = new Event('beforeunload', { cancelable: true });
      window.dispatchEvent(event);

      expect(event.defaultPrevented).toBe(false);
    });
  });
});
