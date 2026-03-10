declare module '@quasar/app-vite/eslint' {
  import type { Linter } from 'eslint';

  interface QuasarEslintPlugin {
    configs: {
      recommended(): Linter.Config[];
    };
  }

  const plugin: QuasarEslintPlugin;
  export default plugin;
}

