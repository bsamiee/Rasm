import { defineConfig } from '@playwright/test';
import { config, served } from '@rasm/test-support/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(config, served('@rasm/test-ui:serve--storybook', 'http://127.0.0.1:6006'), { testMatch: '*.spec.ts' });
