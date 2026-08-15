import { afterEach, describe, expect, it } from 'vitest';
import { mount } from '@vue/test-utils';
import { Quasar } from 'quasar';
import ComboboxField from './combobox-field.vue';

const mounted: { unmount: () => void }[] = [];

// QSelect debounces its virtual-scroll handler on a timer and only cancels it in onBeforeUnmount.
// Left mounted, that timer can outlive the file and fire after jsdom has torn `window` down, which
// Vitest reports as an unhandled `ReferenceError: window is not defined` and fails the whole run
// even though every test passed.
afterEach(() => {
  while (mounted.length) mounted.pop()?.unmount();
});

const mountField = (props: Record<string, unknown> = {}) => {
  const wrapper = mount(ComboboxField, {
    props: { label: 'Section', ...props },
    global: { plugins: [Quasar] },
    attachTo: document.body,
  });

  mounted.push(wrapper);
  return wrapper;
};

const input = (wrapper: ReturnType<typeof mountField>) =>
  wrapper.find<HTMLInputElement>('input');

describe('combobox-field', () => {
  it('keeps a typed value that matches nothing in the suggestions', async () => {
    // The first row to use a new section has to be able to invent it, so this is free text with
    // suggestions rather than a closed list.
    const wrapper = mountField({ suggestions: ['For the sauce'] });

    await input(wrapper).setValue('For the topping');
    await input(wrapper).trigger('keydown', { key: 'Enter', keyCode: 13 });

    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual(['For the topping']);
  });

  it('selects an existing suggestion', async () => {
    const wrapper = mountField({ suggestions: ['For the sauce'] });

    await input(wrapper).setValue('For the sauce');
    await input(wrapper).trigger('keydown', { key: 'Enter', keyCode: 13 });

    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual(['For the sauce']);
  });

  it('normalises a whitespace-only entry to null', async () => {
    // Otherwise a row would count as "has a section" and the dirty check would see a change.
    const wrapper = mountField();

    await input(wrapper).setValue('   ');
    await input(wrapper).trigger('keydown', { key: 'Enter', keyCode: 13 });

    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual([null]);
  });

  it('shows the current value in the input', () => {
    const wrapper = mountField({ modelValue: 'For the sauce' });

    expect(input(wrapper).element.value).toBe('For the sauce');
  });
});
