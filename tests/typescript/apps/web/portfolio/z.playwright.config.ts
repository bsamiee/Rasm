import { join, relative } from 'node:path';
import { defineConfig, devices, type ReporterDescription } from '@playwright/test';
import { Config, Effect } from 'effect';
import { searchForWorkspaceRoot } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CI = Effect.runSync(Config.withDefault(Config.Boolean('CI'), false));
const ROOT = searchForWorkspaceRoot(import.meta.dirname);
const ARTIFACTS = join(ROOT, '.artifacts', 'typescript', 'playwright', relative(ROOT, import.meta.dirname));
const ORIGIN = 'http://localhost:5173';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    forbidOnly: CI,
    retries: CI ? 2 : 0,
    outputDir: join(ARTIFACTS, 'results'),
    reporter: [['list'], ['html', { outputFolder: join(ARTIFACTS, 'report'), open: 'never' }], ...(CI ? ([['junit', { outputFile: join(ARTIFACTS, 'junit.xml') }], ['github']] satisfies ReporterDescription[]) : [])],
    use: { baseURL: ORIGIN, trace: { mode: 'retain-on-failure', snapshots: { dom: true, aria: true, screen: true } }, locale: 'en-US', timezoneId: 'UTC', reducedMotion: 'reduce' },
    webServer: { command: 'nx run @rasm/portfolio:serve:e2e', url: ORIGIN, gracefulShutdown: { signal: 'SIGTERM', timeout: 10_000 } },
    projects: [{ name: 'chromium', use: devices['Desktop Chrome'] }],
});
