import { setup } from '@storybook/vue3';

import type { Preview } from '@storybook/vue3';
import { Quasar } from 'quasar';
import { vueRouter } from 'storybook-vue3-router';

import '@quasar/extras/roboto-font/roboto-font.css'
import '@quasar/extras/material-icons/material-icons.css'

// Loads the quasar styles and registers quasar functionality with storybook
import 'quasar/dist/quasar.css';

// import '../src/css/quasar-variables.scss';
// import '../src/css/app.scss';

setup((app) => {
  app.use(Quasar, {
    config: {},
  });
});

const preview: Preview = {
  decorators: [
    vueRouter(),
    (story) => ({
      components: { story },
      template: `<story />`,
    }),
  ],
  parameters: {
    actions: { argTypesRegex: '^on[A-Z].*' },
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },
  },
};

export default preview;
