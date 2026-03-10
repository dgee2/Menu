import { fileURLToPath, URL } from 'node:url';
import { quasar } from '@quasar/vite-plugin';
import type { StorybookConfig } from '@storybook/vue3-vite';
import tsConfigPaths from 'vite-tsconfig-paths';

const config: StorybookConfig = {
  stories: ['../src/**/*.mdx', '../src/**/*.stories.@(js|jsx|mjs|ts|tsx)'],
  addons: [
    '@storybook/addon-links',
    '@storybook/addon-docs',
    '@storybook/addon-vitest',
    '@storybook/addon-a11y', // Accessibility testing
  ],
  framework: {
    name: '@storybook/vue3-vite',
    options: {
      docgen: 'vue-component-meta', // Better type inference for Vue components
    },
  },
  viteFinal: (config) => {
    if (config.plugins === undefined) {
      config.plugins = [];
    }
    // the vite builder for storybook needs to know how to resolve our import statements, so we use the vite-tsconfig-paths plugin for that
    config.plugins.push(tsConfigPaths());

    // we also need the vite quasar plugin to be able to embed our quasar based project (and it's componets) into an existing (this storybook) project
    // see https://quasar.dev/start/vite-plugin/ for reference
    config.plugins.push(
      quasar({
        // required so that quasar is using our custom theme
        sassVariables: fileURLToPath(new URL('../src/css/quasar.variables.scss', import.meta.url)),
      }),
    );

    return config;
  },
  typescript: {
    check: true,
  },
  staticDirs: ['../public'], // Serve static assets from the public directory
};
export default config;
