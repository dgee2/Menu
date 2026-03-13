import type { Preview } from '@storybook/vue3-vite';
import { setup } from '@storybook/vue3-vite';
import { QLayout, QPageContainer, Quasar } from 'quasar';
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query';
import { createWebHashHistory, createRouter } from 'vue-router';
import { defineComponent } from 'vue';
import { initialize, mswLoader } from 'msw-storybook-addon';
import { ingredientUnitsHandler } from './msw-handlers';

import '@quasar/extras/roboto-font/roboto-font.css';
import '@quasar/extras/material-icons/material-icons.css';

import 'quasar/src/css/index.sass'; // as suggested in https://quasar.dev/start/vite-plugin
import '../src/css/app.scss';

initialize({
  serviceWorker: {
    url: '/mockServiceWorker.js',
  },
  onUnhandledRequest: 'bypass',
});

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
    },
  },
});

const StoryRouteView = defineComponent({
  template: '<div />',
});

export const router = createRouter({
  history: createWebHashHistory(),
  routes: [
    { path: '/', component: StoryRouteView },
    { path: '/recipes', component: StoryRouteView },
    { path: '/new-recipe', component: StoryRouteView },
    { path: '/profile', component: StoryRouteView },
  ],
});

setup((app) => {
  app.use(Quasar, {
    config: {},
  });
  app.use(VueQueryPlugin, { queryClient });
  app.use(router);
});

export const withPageLayout = () => ({
  components: {
    QLayout,
    QPageContainer,
  },
  template: '<q-layout view="lHh lpr lFf"><q-page-container><story /></q-page-container></q-layout>',
});

const preview: Preview = {
  loaders: [mswLoader],
  decorators: [
    () => ({
      async setup() {
        queryClient.clear();
        await router.replace('/');
        await router.isReady();
      },
      template: '<story />',
    }),
  ],
  parameters: {
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },

    backgrounds: {
      default: 'light',
      values: [
        { name: 'light', value: '#ffffff' },
        { name: 'dark', value: '#1d1d1d' },
      ],
    },

    viewport: {
      viewports: {
        mobile: {
          name: 'Mobile',
          styles: { width: '360px', height: '640px' },
        },
        tablet: {
          name: 'Tablet',
          styles: { width: '768px', height: '1024px' },
        },
        desktop: {
          name: 'Desktop',
          styles: { width: '1920px', height: '1080px' },
        },
      },
    },

    msw: {
      handlers: [ingredientUnitsHandler],
    },

    a11y: {
      // 'todo' - show a11y violations in the test UI only
      // 'error' - fail CI on a11y violations
      // 'off' - skip a11y checks entirely
      test: 'todo',
    },
  },
};

export default preview;
