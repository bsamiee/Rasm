import { getReadyUrl } from '@nx/devkit';
import { defineConfig } from '@playwright/test';
import { config } from '@rasm/test-support/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

const url = getReadyUrl('http://localhost:5173');

export default defineConfig(config, { use: { baseURL: url }, webServer: { command: 'pnpm exec nx run @rasm/portfolio:serve--vite', url, reuseExistingServer: true } });
