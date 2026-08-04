import { describe, expect, it } from 'vitest';
import { mount } from '@vue/test-utils';
import { QInput, Quasar } from 'quasar';
import { nextTick } from 'vue';
import RecipeNameField from './recipe-name-field.vue';

const mountNameField = (props: Record<string, unknown> = {}) =>
  mount(RecipeNameField, {
    props,
    global: { plugins: [Quasar] },
    attachTo: document.body,
  });

const validate = async (wrapper: ReturnType<typeof mountNameField>) => {
  const isValid = wrapper.findComponent(QInput).vm.validate();
  await nextTick();
  return isValid;
};

describe('recipe-name-field', () => {
  it('renders a labelled name input with its hint', () => {
    const wrapper = mountNameField();

    expect(wrapper.find('input').exists()).toBe(true);
    expect(wrapper.text()).toContain('Name');
    expect(wrapper.text()).toContain('Enter the name of the recipe');
  });

  it('displays a pre-filled model value', () => {
    const wrapper = mountNameField({ modelValue: 'Chocolate Cake' });

    expect(wrapper.find('input').element.value).toBe('Chocolate Cake');
  });

  it('emits update:modelValue as the user types', async () => {
    const wrapper = mountNameField();

    await wrapper.find('input').setValue('Lasagne');

    expect(wrapper.emitted('update:modelValue')).toEqual([['Lasagne']]);
  });

  it('rejects a null name', async () => {
    const wrapper = mountNameField({ modelValue: null });

    expect(await validate(wrapper)).toBe(false);
    expect(wrapper.text()).toContain('Name is required');
  });

  it('rejects an empty name', async () => {
    const wrapper = mountNameField({ modelValue: '' });

    expect(await validate(wrapper)).toBe(false);
    expect(wrapper.text()).toContain('Name is required');
  });

  it('rejects a whitespace-only name', async () => {
    const wrapper = mountNameField({ modelValue: '   ' });

    expect(await validate(wrapper)).toBe(false);
    expect(wrapper.text()).toContain('Name is required');
  });

  it('accepts a non-blank name', async () => {
    const wrapper = mountNameField({ modelValue: 'Lasagne' });

    expect(await validate(wrapper)).toBe(true);
    expect(wrapper.text()).not.toContain('Name is required');
  });

  it('accepts a name that is blank-padded but not blank', async () => {
    const wrapper = mountNameField({ modelValue: '  Lasagne  ' });

    expect(await validate(wrapper)).toBe(true);
    expect(wrapper.text()).not.toContain('Name is required');
  });
});
