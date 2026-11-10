import { defineConfig } from '@playwright/test';
import { config } from '@rasm/test-support/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(config, {
    webServer: { command: 'pnpm exec vite --port 0', cwd: '../../../../../apps/web/portfolio', wait: { stdout: /Local:\s+(?<playwright_test_base_url>http:\/\/[\w.]+:[1-9]\d*)/u }, timeout: 120_000 },
});
