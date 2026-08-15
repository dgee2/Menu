import { afterEach, describe, expect, it } from 'vitest';
import { mount, type VueWrapper } from '@vue/test-utils';
import { QSelect, Quasar } from 'quasar';
import { nextTick } from 'vue';
import IngredientRowEditor from './ingredient-row-editor.vue';

const mounted: VueWrapper[] = [];

// The section field is a QSelect, whose debounced virtual-scroll timer is only cancelled in
// onBeforeUnmount. Left mounted it can fire after jsdom has torn `window` down and fail the run
// as an unhandled error. See combobox-field.test.ts.
afterEach(() => {
  while (mounted.length) mounted.pop()?.unmount();
});

const mountRow = (props: Record<string, unknown> = {}) => {
  const wrapper = mount(IngredientRowEditor, {
    props: { canMoveUp: false, canMoveDown: false, ...props },
    global: { plugins: [Quasar] },
    attachTo: document.body,
  });

  mounted.push(wrapper);
  return wrapper;
};

const field = (wrapper: VueWrapper, label: string) => {
  const found = wrapper
    .findAll('.q-field')
    .find((candidate) => candidate.find('.q-field__label').text() === label);

  if (!found) throw new Error(`No field labelled "${label}"`);

  return found;
};

const setField = async (wrapper: VueWrapper, label: string, value: string) => {
  await field(wrapper, label).find<HTMLInputElement>('input').setValue(value);
};

/** Runs the field's own Quasar rules and returns the messages that failed. */
const validate = async (wrapper: VueWrapper, label: string) => {
  const input = field(wrapper, label).findComponent({ name: 'QInput' });
  await (input.vm as unknown as { validate: () => boolean | Promise<boolean> }).validate();
  await nextTick();

  return field(wrapper, label).find('.q-field__messages').text();
};

describe('ingredient-row-editor', () => {
  it('renders every editable field', () => {
    const wrapper = mountRow();

    const labels = wrapper.findAll('.q-field__label').map((label) => label.text());
    expect(labels).toEqual(['Measure', 'Ingredient', 'Preparation', 'Section']);
  });

  it('emits the edited values through v-model', async () => {
    const wrapper = mountRow();

    await setField(wrapper, 'Ingredient', 'Flour');
    await setField(wrapper, 'Measure', '200g');

    expect(wrapper.emitted('update:ingredientText')?.at(-1)).toEqual(['Flour']);
    expect(wrapper.emitted('update:measureText')?.at(-1)).toEqual(['200g']);
  });

  it('treats an untouched row as valid', async () => {
    // A seeded blank row must not fail validation the moment the form is submitted, or "zero
    // ingredients is a valid recipe" stops being true.
    const wrapper = mountRow();

    expect(await validate(wrapper, 'Ingredient')).not.toContain('required');
    expect(await validate(wrapper, 'Measure')).not.toContain('required');
  });

  it('requires the measure once the ingredient has been filled in', async () => {
    const wrapper = mountRow({ ingredientText: 'Flour' });

    expect(await validate(wrapper, 'Measure')).toContain('Measure is required');
  });

  it('requires the ingredient once the measure has been filled in', async () => {
    const wrapper = mountRow({ measureText: '200g' });

    expect(await validate(wrapper, 'Ingredient')).toContain('Ingredient is required');
  });

  it('treats toggling Optional as touching the row', async () => {
    const wrapper = mountRow({ isOptional: true });

    expect(await validate(wrapper, 'Ingredient')).toContain('Ingredient is required');
  });

  it('rejects whitespace as a filled-in value', async () => {
    const wrapper = mountRow({ ingredientText: 'Flour', measureText: '   ' });

    expect(await validate(wrapper, 'Measure')).toContain('Measure is required');
  });

  it('passes section suggestions through to the section field', () => {
    const wrapper = mountRow({ sectionSuggestions: ['For the sauce', 'For the topping'] });

    expect(field(wrapper, 'Section').findComponent(QSelect).exists()).toBe(true);
    expect(wrapper.findComponent({ name: 'combobox-field' }).props('suggestions')).toEqual([
      'For the sauce',
      'For the topping',
    ]);
  });

  it('disables the move buttons at the ends of the list', () => {
    const wrapper = mountRow({ canMoveUp: false, canMoveDown: true });

    expect(wrapper.find('[aria-label="Move ingredient up"]').attributes('disabled')).toBeDefined();
    expect(wrapper.find('[aria-label="Move ingredient down"]').attributes('disabled')).toBeUndefined();
  });

  it('emits move and remove events', async () => {
    const wrapper = mountRow({ canMoveUp: true, canMoveDown: true });

    await wrapper.find('[aria-label="Move ingredient up"]').trigger('click');
    await wrapper.find('[aria-label="Move ingredient down"]').trigger('click');
    await wrapper.find('[aria-label="Remove ingredient"]').trigger('click');

    expect(wrapper.emitted('moveUp')).toHaveLength(1);
    expect(wrapper.emitted('moveDown')).toHaveLength(1);
    expect(wrapper.emitted('remove')).toHaveLength(1);
  });
});
