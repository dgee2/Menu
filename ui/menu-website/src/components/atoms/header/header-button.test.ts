import { mount } from '@vue/test-utils';
import { Quasar } from 'quasar';
import { describe, expect, it } from 'vitest';
import HeaderButton from './header-button.vue';

describe('HeaderButton', () => {
  it('renders its label and emits click events', async () => {
    const wrapper = mount(HeaderButton, {
      props: { label: 'Recipes' },
      global: { plugins: [Quasar] },
    });

    expect(wrapper.get('button').text()).toBe('Recipes');

    await wrapper.get('button').trigger('click');

    expect(wrapper.emitted('click')).toHaveLength(1);
  });
});
