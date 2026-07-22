/**
 * Root residency is deliberate self-defense: playwright resolves its config from cwd ONLY (no upward
 * search), a config-less root invocation sweeps every *.spec.ts in the tree and executes foreign
 * module top-levels, and the default outputDir is a root test-results/. Auto-discovery of this file
 * bounds every bare `playwright test`: explicit testDir + *.pw.ts match (disjoint from the vitest
 * globs by suffix), capped workers, per-test and global timeouts, artifacts under .artifacts/.
 */

import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { defineConfig, devices, type PlaywrightTestConfig, type PlaywrightWorkerOptions, type Project } from '@playwright/test';

// --- [TYPES] ---------------------------------------------------------------------------

type Lane = Pick<Project, 'testMatch' | 'use'>;
type Target = {
    readonly lanes: Readonly<Record<string, Lane>>;
    // Absent origin rides the kit hermetic corpus (fixtures fulfill routes in-context); a served
    // product carries its origin as baseURL and the fixtures discriminate on that presence.
    readonly origin?: string;
    readonly prefix: string;
    readonly serve?: {
        readonly command: string;
        readonly cwd?: string;
        readonly env?: Readonly<Record<string, string>>;
        readonly timeoutMs?: number;
        readonly url: string;
    };
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = path.dirname(fileURLToPath(import.meta.url));
const _ARTIFACTS = path.join(_ROOT, '.artifacts/typescript/e2e');
const _CI = process.env['CI'] === 'true';
const _SERVE = { shutdownMs: 10_000, startupMs: 120_000 } as const;

// Evidence policy rows keyed by profile: CI captures traces on the retry pass and keeps failure
// video; a local run has zero retries, so the trace itself is the failure evidence and video is off.
const _EVIDENCE = {
    ci: { screenshot: 'only-on-failure', trace: 'on-first-retry', video: 'retain-on-failure' },
    local: { screenshot: 'only-on-failure', trace: 'retain-on-failure', video: 'off' },
} as const satisfies Record<string, Pick<PlaywrightWorkerOptions, 'screenshot' | 'trace' | 'video'>>;

// Software-rendering rows for the gpu lane, keyed by host platform: linux CI pins the WebGPU
// adapter to swiftshader so the acquisition gauge stays deterministic off real hardware.
const _GPU = ['--enable-unsafe-swiftshader', '--enable-unsafe-webgpu', ...(process.platform === 'linux' ? ['--use-webgpu-adapter=swiftshader'] : [])];

// The target roster is config data: each row is one system-under-test the harness serves, and its
// lanes fan the engine matrix. The hermetic row owns the empty prefix, so committed golden paths
// ({projectName} keyed) never move; a served product lands as ONE row — origin, serve command,
// `<name>:`-prefixed lanes — and its projects plus webServer lifecycle mint from the roster.
// A new engine is one lane row plus its out-of-band `playwright install <engine>`; firefox's row
// lands when a served product flow demands tri-engine coverage. Per-lane testMatch keeps
// screenshot goldens single-engine.
const _TARGETS = {
    hermetic: {
        lanes: {
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
        },
        prefix: '',
    },
} as const satisfies Record<string, Target>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _rows: ReadonlyArray<Target> = Object.values(_TARGETS);

const _projects: ReadonlyArray<Project> = _rows.flatMap((target) =>
    Object.entries(target.lanes).map(([lane, shape]) => ({
        name: `${target.prefix}${lane}`,
        ...shape,
        use: { ...shape.use, ...(target.origin === undefined ? {} : { baseURL: target.origin }) },
    })),
);

const _servers = _rows.flatMap((target) =>
    target.serve === undefined
        ? []
        : [
              {
                  command: target.serve.command,
                  cwd: target.serve.cwd ?? _ROOT,
                  ...(target.serve.env === undefined ? {} : { env: { ...target.serve.env } }),
                  gracefulShutdown: { signal: 'SIGTERM' as const, timeout: _SERVE.shutdownMs },
                  reuseExistingServer: !_CI,
                  timeout: target.serve.timeoutMs ?? _SERVE.startupMs,
                  url: target.serve.url,
              },
          ],
);

// --- [EXPORTS] -------------------------------------------------------------------------

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
    projects: [..._projects],
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
        testIdAttribute: 'data-testid',
        timezoneId: 'UTC',
        ..._EVIDENCE[_CI ? 'ci' : 'local'],
    },
    ...(_servers.length > 0 ? { webServer: [..._servers] } : {}),
    workers: '50%',
});

export default config;
