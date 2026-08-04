import { describe, expect, it } from 'vitest';
import { mount } from '@vue/test-utils';
import { QInput, Quasar, type ValidationRule } from 'quasar';
import { nextTick } from 'vue';
import NumberField from './number-field.vue';

const mountNumberField = (props: Record<string, unknown> = {}) =>
  mount(NumberField, {
    props: { label: 'Servings', ...props },
    global: { plugins: [Quasar] },
    attachTo: document.body,
  });

/** Emits from the inner q-input, which is what the browser feeds the component at runtime. */
const emitFromInput = async (
  wrapper: ReturnType<typeof mountNumberField>,
  value: string | number | null,
) => {
  wrapper.findComponent(QInput).vm.$emit('update:model-value', value);
  await nextTick();
};

describe('number-field', () => {
  it('renders a numeric input with the supplied label', () => {
    const wrapper = mountNumberField();

    expect(wrapper.find('input').attributes('type')).toBe('number');
    expect(wrapper.text()).toContain('Servings');
  });

  it('forwards min and step to the underlying input', () => {
    const wrapper = mountNumberField({ min: 0, step: 1 });

    const input = wrapper.find('input');
    expect(input.attributes('min')).toBe('0');
    expect(input.attributes('step')).toBe('1');
  });

  it('renders the hint when one is supplied', () => {
    const wrapper = mountNumberField({ hint: 'Enter a whole number.' });

    expect(wrapper.text()).toContain('Enter a whole number.');
  });

  it('displays a pre-filled model value', () => {
    const wrapper = mountNumberField({ modelValue: 8 });

    expect(wrapper.find('input').element.value).toBe('8');
  });

  it('emits a number when the input holds a numeric string', async () => {
    const wrapper = mountNumberField();

    await emitFromInput(wrapper, '42');

    expect(wrapper.emitted('update:modelValue')).toEqual([[42]]);
  });

  it('emits a negative number rather than clamping it, leaving rejection to the rules', async () => {
    const wrapper = mountNumberField({ min: 0 });

    await emitFromInput(wrapper, '-5');

    expect(wrapper.emitted('update:modelValue')).toEqual([[-5]]);
  });

  it('emits a fractional number rather than rounding it, leaving rejection to the rules', async () => {
    const wrapper = mountNumberField();

    await emitFromInput(wrapper, '3.5');

    expect(wrapper.emitted('update:modelValue')).toEqual([[3.5]]);
  });

  it('emits null when the input is cleared to an empty string', async () => {
    const wrapper = mountNumberField({ modelValue: 8 });

    await emitFromInput(wrapper, '');

    expect(wrapper.emitted('update:modelValue')).toEqual([[null]]);
  });

  it('emits null when the input reports null', async () => {
    const wrapper = mountNumberField({ modelValue: 8 });

    await emitFromInput(wrapper, null);

    expect(wrapper.emitted('update:modelValue')).toEqual([[null]]);
  });

  it('emits null rather than NaN for an unparseable value', async () => {
    const wrapper = mountNumberField();

    await emitFromInput(wrapper, 'abc');

    expect(wrapper.emitted('update:modelValue')).toEqual([[null]]);
  });

  it('emits zero rather than null, so a legitimate 0 is not discarded', async () => {
    const wrapper = mountNumberField();

    await emitFromInput(wrapper, '0');

    expect(wrapper.emitted('update:modelValue')).toEqual([[0]]);
  });

  it('fails validation and shows the message when a rule rejects the value', async () => {
    const rules: ValidationRule[] = [
      (val: number | null) => val == null || val >= 0 || 'Must be 0 or greater',
    ];
    const wrapper = mountNumberField({ rules, modelValue: -5 });

    const isValid = wrapper.findComponent(QInput).vm.validate();
    await nextTick();

    expect(isValid).toBe(false);
    expect(wrapper.text()).toContain('Must be 0 or greater');
  });

  it('passes validation when the value satisfies the rules', async () => {
    const rules: ValidationRule[] = [
      (val: number | null) => val == null || val >= 0 || 'Must be 0 or greater',
    ];
    const wrapper = mountNumberField({ rules, modelValue: 4 });

    const isValid = wrapper.findComponent(QInput).vm.validate();
    await nextTick();

    expect(isValid).toBe(true);
    expect(wrapper.text()).not.toContain('Must be 0 or greater');
  });
});
