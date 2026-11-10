import { defineConfig } from '@playwright/test';
import { config } from '#/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(config, { testDir: 'suite', testMatch: '*.spec.ts' });
