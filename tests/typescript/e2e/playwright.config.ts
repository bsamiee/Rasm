import { defineConfig } from '@playwright/test';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PORTFOLIO = 'http://localhost:5173';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    outputDir: '../../../.artifacts/playwright-test/e2e',
    use: { baseURL: _PORTFOLIO, trace: 'retain-on-failure', timezoneId: 'UTC', reducedMotion: 'reduce' },
    webServer: {
        command: 'vite',
        cwd: '../../../apps/web/portfolio',
        url: _PORTFOLIO,
        gracefulShutdown: { signal: 'SIGTERM', timeout: 0 },
    },
});
