import { defineConfig } from '@playwright/test';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PORTFOLIO = 'http://localhost:5173';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    outputDir: '../../../.artifacts/playwright-test/e2e',
    use: { baseURL: _PORTFOLIO, trace: 'retain-on-failure', timezoneId: 'UTC', reducedMotion: 'reduce' },
    webServer: {
        command: 'nx run architectural-portfolio:serve',
        url: _PORTFOLIO,
        reuseExistingServer: true,
        gracefulShutdown: { signal: 'SIGTERM', timeout: 0 },
    },
});
