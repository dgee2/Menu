// Since Storybook 10.3, @storybook/addon-vitest provisions preview annotations automatically.
// No need to call setProjectAnnotations here.
import { configure } from 'storybook/test';

// Increase the default async utility timeout (findByText etc.) for the browser test environment.
// The default 1000ms can be too short on Windows with Playwright.
configure({ asyncUtilTimeout: 5000 });
