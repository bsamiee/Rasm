/**
 * Root residency is deliberate self-defense: playwright resolves its config from cwd ONLY (no upward
 * search), a config-less root invocation sweeps every *.spec.ts in the tree and executes foreign
 * module top-levels, and the default outputDir is a root test-results/. Auto-discovery of this file
 * bounds every bare `playwright test`: explicit testDir + *.pw.ts match (disjoint from the vitest
 * globs by suffix), capped workers, per-test and global timeouts, artifacts under .artifacts/.
 */

import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { defineConfig, devices, type PlaywrightTestConfig } from '@playwright/test';

// --- [CONSTANTS] -------------------------------------------------------------------------

const _ROOT = path.dirname(fileURLToPath(import.meta.url));
const _ARTIFACTS = path.join(_ROOT, '.artifacts/typescript/e2e');
const _CI = process.env['CI'] === 'true';

// Software-rendering permissions for the gpu lane; the Linux-CI determinism row adds --use-webgpu-adapter=swiftshader.
const _GPU = ['--enable-unsafe-swiftshader', '--enable-unsafe-webgpu'];

// The engine roster is data: a row activates with `playwright install <engine>`; firefox joins when
// a served product flow demands tri-engine coverage. Per-lane testMatch keeps goldens single-engine.
const _LANES = {
    chromium: {
        testMatch: ['platform/**/*.pw.ts', 'engine/**/*.pw.ts', 'load/**/*.pw.ts'],
        use: { ...devices['Desktop Chrome'] },
    },
    viewer: {
        testMatch: ['gpu/**/*.pw.ts'],
        use: { ...devices['Desktop Chrome'], launchOptions: { args: _GPU } },
    },
    webkit: {
        testMatch: ['engine/**/*.pw.ts'],
        use: { ...devices['Desktop Safari'] },
    },
};

// --- [EXPORTS] ---------------------------------------------------------------------------

const config: PlaywrightTestConfig = defineConfig({
    captureGitInfo: { commit: true, diff: false },
    expect: { timeout: 5_000, toHaveScreenshot: { maxDiffPixelRatio: 0.02 } },
    failOnFlakyTests: _CI,
    forbidOnly: _CI,
    fullyParallel: true,
    globalTimeout: 900_000,
    maxFailures: _CI ? 10 : 0,
    outputDir: path.join(_ARTIFACTS, 'test-results'),
    projects: Object.entries(_LANES).map(([name, lane]) => ({ name, ...lane })),
    reporter: _CI
        ? [['blob', { outputDir: path.join(_ARTIFACTS, 'blob') }], ['github']]
        : [['list'], ['html', { open: 'never', outputFolder: path.join(_ARTIFACTS, 'report') }]],
    retries: _CI ? 2 : 0,
    // Goldens key per-platform by decision: a Linux CI lane lands as a golden mint, never a break.
    snapshotPathTemplate: '{testDir}/goldens/{projectName}/{platform}/{testFilePath}/{arg}{ext}',
    testDir: path.join(_ROOT, 'tests/typescript/e2e'),
    testMatch: '**/*.pw.ts',
    timeout: 30_000,
    tsconfig: path.join(_ROOT, 'tests/typescript/e2e/tsconfig.json'),
    use: {
        actionTimeout: 10_000,
        colorScheme: 'light',
        // reducedMotion is a context option, not a test option; it rides the contextOptions pass-through.
        contextOptions: { reducedMotion: 'reduce' },
        locale: 'en-US',
        navigationTimeout: 15_000,
        screenshot: 'only-on-failure',
        testIdAttribute: 'data-testid',
        timezoneId: 'UTC',
        trace: 'on-first-retry',
        video: 'retain-on-failure',
    },
    workers: '50%',
});

export default config;
