import { describe, expect, it } from 'vitest';
import { mount } from '@vue/test-utils';
import { QInput, Quasar, type ValidationRule } from 'quasar';
import { nextTick } from 'vue';
import TextField from './text-field.vue';

const mountTextField = (props: Record<string, unknown> = {}) =>
  mount(TextField, {
    props: { label: 'Text Field', ...props },
    global: { plugins: [Quasar] },
    attachTo: document.body,
  });

describe('text-field', () => {
  it('renders a single-line input by default', () => {
    const wrapper = mountTextField();

    expect(wrapper.find('input').exists()).toBe(true);
    expect(wrapper.find('textarea').exists()).toBe(false);
  });

  it('renders a textarea when type is textarea', () => {
    const wrapper = mountTextField({ type: 'textarea' });

    expect(wrapper.find('textarea').exists()).toBe(true);
    expect(wrapper.find('input').exists()).toBe(false);
  });

  it('renders the hint when one is supplied', () => {
    const wrapper = mountTextField({ hint: 'A short description' });

    expect(wrapper.text()).toContain('A short description');
  });

  it('emits update:modelValue as the user types', async () => {
    const wrapper = mountTextField();

    await wrapper.find('input').setValue('Lasagne');

    expect(wrapper.emitted('update:modelValue')).toEqual([['Lasagne']]);
  });

  it('accepts null as a model value', () => {
    const wrapper = mountTextField({ modelValue: null });

    expect(wrapper.find('input').element.value).toBe('');
  });

  it('fails validation and shows the message when a rule rejects the value', async () => {
    const rules: ValidationRule[] = [(val: string | null) => !!val || 'Required'];
    const wrapper = mountTextField({ rules, modelValue: '' });

    const isValid = wrapper.findComponent(QInput).vm.validate();
    await nextTick();

    expect(isValid).toBe(false);
    expect(wrapper.text()).toContain('Required');
  });

  it('passes validation and shows no message when the rule accepts the value', async () => {
    const rules: ValidationRule[] = [(val: string | null) => !!val || 'Required'];
    const wrapper = mountTextField({ rules, modelValue: 'Lasagne' });

    const isValid = wrapper.findComponent(QInput).vm.validate();
    await nextTick();

    expect(isValid).toBe(true);
    expect(wrapper.text()).not.toContain('Required');
  });
});
