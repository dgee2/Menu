/**
 * plugins/index.ts
 *
 * Automatically included in `./src/main.ts`
 */

// Plugins
import { loadFonts } from './webfontloader'
import vuetify from './vuetify'
import pinia from '../store'
import router from '../router'

// Types
import type { App } from 'vue'

export async function registerPlugins (app: App) {
  await loadFonts()
  app
    .use(vuetify)
    .use(router)
    .use(pinia)
}
