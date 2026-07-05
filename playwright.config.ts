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

// Software-rendering rows for the gpu lane, keyed by host platform: linux CI pins the WebGPU
// adapter to swiftshader so the acquisition gauge stays deterministic off real hardware.
const _GPU = ['--enable-unsafe-swiftshader', '--enable-unsafe-webgpu', ...(process.platform === 'linux' ? ['--use-webgpu-adapter=swiftshader'] : [])];

// The engine roster is config data: a new engine is one row here plus its out-of-band
// `playwright install <engine>`; firefox's row lands when a served product flow demands
// tri-engine coverage. Per-lane testMatch keeps screenshot goldens single-engine.
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
    expect: {
        timeout: 5_000,
        toHaveScreenshot: { maxDiffPixelRatio: 0.02 },
        // Aria goldens are engine-invariant — one golden per test across browsers, so the template
        // deliberately omits {projectName}/{platform}, unlike the pixel goldens below.
        toMatchAriaSnapshot: { pathTemplate: '{testDir}/goldens/aria/{testFilePath}/{arg}{ext}' },
    },
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
    reportSlowTests: { max: 5, threshold: 15_000 },
    retries: _CI ? 2 : 0,
    // Goldens key per-project and per-platform by decision: a new CI platform's first run WRITES its
    // missing goldens and fails (updateSnapshots stays 'missing'), so the mint lands as committed
    // files under review, never as a mismatch break against another platform's pixels.
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
