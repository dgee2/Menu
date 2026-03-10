import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import tsconfigPaths from 'vite-tsconfig-paths';
import vue from '@vitejs/plugin-vue';
import vueDevTools from 'vite-plugin-vue-devtools';
import { quasar, transformAssetUrls } from '@quasar/vite-plugin';

const isStorybook = process.env.STORYBOOK === 'true';

// https://vite.dev/config/
export default defineConfig({
  build: {
    outDir: 'dist',
    emptyOutDir: true,
  },
  plugins: [
    tsconfigPaths(),
    vue({
      template: { transformAssetUrls },
    }),
    // vite-plugin-vue-devtools (via vite-plugin-inspect) is incompatible with
    // Storybook's Vite builder environment, so skip it when Storybook is running.
    !isStorybook &&
      vueDevTools({
        launchEditor: 'webstorm',
      }),
    // @quasar/plugin-vite options list:
    // https://github.com/quasarframework/quasar/blob/dev/vite-plugin/index.d.ts
    quasar({
      autoImportComponentCase: 'combined',
      sassVariables: fileURLToPath(new URL('./src/css/quasar.variables.scss', import.meta.url)),
    }),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
});
