import { createRequire } from 'node:module';
import { dirname, join } from 'node:path';
import { defineConfig, devices, type ReporterDescription } from '@playwright/test';
import { Config, Effect } from 'effect';
import { searchForWorkspaceRoot } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CI = Effect.runSync(Config.withDefault(Config.Boolean('CI'), false));
const ROOT = searchForWorkspaceRoot(import.meta.dirname);
const { name }: { readonly name: string } = createRequire(import.meta.url)('./package.json');
const ARTIFACTS = join(ROOT, '.artifacts', 'playwright', name);
const ORIGIN = 'http://localhost:5173';
const APP = dirname(createRequire(import.meta.url).resolve('@rasm/portfolio/package.json'));

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    forbidOnly: CI,
    retries: CI ? 2 : 0,
    outputDir: join(ARTIFACTS, 'results'),
    reporter: [['list'], ['html', { outputFolder: join(ARTIFACTS, 'report'), open: 'never' }], ...(CI ? ([['junit', { outputFile: join(ARTIFACTS, 'junit.xml') }], ['github']] satisfies ReporterDescription[]) : [])],
    use: { baseURL: ORIGIN, trace: { mode: 'retain-on-failure', snapshots: { dom: true, aria: true, screen: true } }, locale: 'en-US', timezoneId: 'UTC', reducedMotion: 'reduce' },
    webServer: { command: `${join(import.meta.dirname, 'node_modules', '.bin', 'vite')} --mode e2e --strictPort`, cwd: APP, url: ORIGIN, gracefulShutdown: { signal: 'SIGTERM', timeout: 10_000 } },
    projects: [{ name: 'chromium', use: devices['Desktop Chrome'] }],
});
