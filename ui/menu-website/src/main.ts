import { createApp } from 'vue';
import { createPinia } from 'pinia';

import { Quasar } from 'quasar';
import quasarLang from 'quasar/lang/en-GB';

// Import icon libraries
import '@quasar/extras/roboto-font/roboto-font.css';
import '@quasar/extras/material-icons/material-icons.css';

// Import Quasar css
import 'quasar/src/css/index.sass';

import App from './App.vue';
import router from './router';
import { createAuth0 } from './boot/auth0';
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query';

const app = createApp(App);

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,
    },
  },
});

app.use(VueQueryPlugin, { queryClient, enableDevtoolsV6Plugin: true });

app.use(Quasar, {
  plugins: {}, // import Quasar plugins and add here
  lang: quasarLang,
  /*
  config: {
    brand: {
      // primary: '#e46262',
      // ... or all other brand colors
    },
    notify: {...}, // default set of options for Notify Quasar plugin
    loading: {...}, // default set of options for Loading Quasar plugin
    loadingBar: { ... }, // settings for LoadingBar Quasar plugin
    // ..and many more (check Installation card on each Quasar component/directive/plugin)
  }
  */
});
app.use(createPinia());
app.use(router);
app.use(createAuth0());

app.mount('#app');
