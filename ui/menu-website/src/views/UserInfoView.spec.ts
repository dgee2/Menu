import { mount } from '@vue/test-utils';
import { describe, it, expect } from 'vitest';
import UserInfoView from './UserInfoView.vue';

describe('UserInfoView', () => {
  it('shows user name and image when authenticated', () => {
    const user = { name: 'Alice', picture: 'https://example.com/a.png' };
    const wrapper = mount(UserInfoView, {
      props: { isAuthenticated: true, user, auth0: {} },
      global: {
        stubs: {
          'q-page': { template: '<div><slot/></div>' },
          // Use a plain img stub to avoid recursive q-img component rendering
          'q-img': { template: '<img />' },
        },
      },
    });

    expect(wrapper.text()).toContain('Alice');
    // q-img is stubbed as <img />, so assert on img
    expect(wrapper.find('img').exists()).toBe(true);
  });

  it('shows Not authenticated when not authenticated', () => {
    const wrapper = mount(UserInfoView, {
      props: { isAuthenticated: false, user: null, auth0: {} },
      global: {
        stubs: {
          'q-page': { template: '<div><slot/></div>' },
          'q-img': { template: '<img />' },
        },
      },
    });

    expect(wrapper.text()).toContain('Not authenticated');
  });
});
