import { defineConfig } from '@playwright/test';
import { config } from '@rasm/test-support/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(config, {
    testMatch: '*.spec.ts',
    webServer: { command: 'pnpm exec storybook dev --ci', wait: { stdout: /Local:\s+(?<playwright_test_base_url>http:\/\/[\w.]+:[1-9]\d*)/u }, timeout: 120_000 },
});
