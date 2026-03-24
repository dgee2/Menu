import addonA11y from '@storybook/addon-a11y';
import addonDocs from '@storybook/addon-docs';
import addonLinks from '@storybook/addon-links';
import { setup, definePreview } from '@storybook/vue3-vite';
import { QLayout, QPageContainer, Quasar } from 'quasar';
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query';
import { createMemoryHistory, createRouter } from 'vue-router';
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
  quiet: true,
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
  history: createMemoryHistory(),
  routes: [
    { path: '/', component: StoryRouteView },
    { path: '/recipes', component: StoryRouteView },
    { path: '/new-recipe', component: StoryRouteView },
    { path: '/profile', component: StoryRouteView },
  ],
});

setup((app) => {
  // Guard against the setup callback being invoked multiple times on the same app instance.
  // Quasar always installs $q; if it's already present, all plugins are already registered.
  if (app.config.globalProperties.$q) return;

  app.use(Quasar, {
    config: {},
  });
  app.use(VueQueryPlugin, { queryClient });
  if (!app.config.globalProperties.$router) {
    app.use(router);
  }
});

export const withPageLayout = () => ({
  components: {
    QLayout,
    QPageContainer,
  },
  template:
    '<q-layout view="lHh lpr lFf"><q-page-container><story /></q-page-container></q-layout>',
});

export default definePreview({
  loaders: [mswLoader],

  initialGlobals: {
    backgrounds: { value: 'light' },
  },

  decorators: [
    () => ({
      setup() {
        queryClient.clear();
        void router.replace('/');
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
      options: {
        light: { name: 'Light', value: '#ffffff' },
        dark: { name: 'Dark', value: '#1d1d1d' },
      },
    },

    viewport: {
      options: {
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

  addons: [addonLinks(), addonDocs(), addonA11y()],
});
