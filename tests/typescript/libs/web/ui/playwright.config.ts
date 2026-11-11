import { getReadyUrl } from '@nx/devkit';
import { defineConfig } from '@playwright/test';
import { config } from '@rasm/test-support/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

const url = getReadyUrl('http://127.0.0.1:6006');

export default defineConfig(config, { testMatch: '*.spec.ts', use: { baseURL: url }, webServer: { command: 'pnpm exec nx run @rasm/test-ui:serve--storybook', url, reuseExistingServer: true } });
