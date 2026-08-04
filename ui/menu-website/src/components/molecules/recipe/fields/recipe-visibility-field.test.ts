import { describe, expect, it } from 'vitest';
import { mount } from '@vue/test-utils';
import { QSelect, Quasar } from 'quasar';
import { nextTick } from 'vue';
import RecipeVisibilityField from './recipe-visibility-field.vue';

type VisibilityOption = { label: string; value: string };

const mountVisibilityField = (props: Record<string, unknown> = {}) =>
  mount(RecipeVisibilityField, {
    props,
    global: { plugins: [Quasar] },
    attachTo: document.body,
  });

type Wrapper = ReturnType<typeof mountVisibilityField>;

const optionsOf = (wrapper: Wrapper) =>
  wrapper.findComponent(QSelect).props('options') as VisibilityOption[];

/** The text the QSelect currently displays, i.e. the option label rather than the raw scope. */
const displayedValue = (wrapper: Wrapper) => wrapper.find('.q-field__native').text();

/**
 * Emits from the inner q-select, which is what picking an item from the dropdown feeds the
 * component at runtime. The menu itself is portalled outside the wrapper, so it is not
 * reachable here — the dropdown-open-and-click path is covered by the Storybook story.
 */
const selectOption = async (wrapper: Wrapper, label: string) => {
  const option = optionsOf(wrapper).find((candidate) => candidate.label === label);
  if (!option) {
    throw new Error(`No option labelled "${label}"`);
  }

  wrapper.findComponent(QSelect).vm.$emit('update:modelValue', option);
  await nextTick();
};

describe('recipe-visibility-field', () => {
  it('renders a labelled select with its hint', () => {
    const wrapper = mountVisibilityField({ modelValue: 'Private' });

    expect(wrapper.text()).toContain('Visibility');
    expect(wrapper.text()).toContain('Who can see this recipe');
  });

  it('offers exactly the two visibility options', () => {
    const wrapper = mountVisibilityField({ modelValue: 'Private' });

    expect(optionsOf(wrapper)).toEqual([
      { label: 'Private', value: 'Private' },
      { label: 'Visible to all Menu users', value: 'AuthenticatedUsers' },
    ]);
  });

  it('displays Private for the default new-recipe scope', () => {
    const wrapper = mountVisibilityField({ modelValue: 'Private' });

    expect(displayedValue(wrapper)).toBe('Private');
  });

  it('displays the friendly label for a pre-filled AuthenticatedUsers scope', () => {
    const wrapper = mountVisibilityField({ modelValue: 'AuthenticatedUsers' });

    expect(displayedValue(wrapper)).toBe('Visible to all Menu users');
  });

  it('emits the raw scope when Visible to all Menu users is selected', async () => {
    const wrapper = mountVisibilityField({ modelValue: 'Private' });

    await selectOption(wrapper, 'Visible to all Menu users');

    expect(wrapper.emitted('update:modelValue')).toEqual([['AuthenticatedUsers']]);
  });

  it('emits the raw scope when Private is selected', async () => {
    const wrapper = mountVisibilityField({ modelValue: 'AuthenticatedUsers' });

    await selectOption(wrapper, 'Private');

    expect(wrapper.emitted('update:modelValue')).toEqual([['Private']]);
  });

  it('displays nothing when the model has no value', () => {
    const wrapper = mountVisibilityField();

    expect(displayedValue(wrapper)).toBe('');
  });

  it('displays nothing for an unrecognised scope rather than falling back to Private', () => {
    const wrapper = mountVisibilityField({ modelValue: 'SomeFutureScope' });

    expect(displayedValue(wrapper)).toBe('');
    expect(wrapper.emitted('update:modelValue')).toBeUndefined();
  });
});
