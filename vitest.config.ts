/// <reference types="vitest/config" />
/**
 * Root Vitest authority: the estate aggregate over per-package projects plus one armed browser
 * lane. Each package with specs carries a one-call vitest.config.ts deriving from `createProject`
 * exported here — the per-package file is what lets Nx infer a per-project `test` target, and the
 * import back into this module is acyclic because the `projects` rows are glob strings, never
 * imports. This file keeps the estate-level options — coverage, reporters, output routing, worker
 * caps — and is the single entry Stryker and whole-estate runs consume. Artifacts route to
 * .artifacts/typescript.
 */

import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { playwright } from '@vitest/browser-playwright';
import { defineConfig, type ViteUserConfig } from 'vitest/config';

// --- [TYPES] ---------------------------------------------------------------------------

type ProjectTest = NonNullable<ViteUserConfig['test']>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const Root = path.dirname(fileURLToPath(import.meta.url));
const _CI = process.env['CI'] === 'true';
const _ARTIFACTS = {
    bench: path.resolve(Root, '.artifacts/typescript/bench'),
    coverage: path.resolve(Root, '.artifacts/typescript/coverage'),
    results: path.resolve(Root, '.artifacts/typescript/test-results'),
} as const;
const _CONFIG = {
    cacheDir: '.cache/vitest',
    deps: { interopDefault: true },
    fakeTimers: {
        loopLimit: 10_000,
        shouldClearNativeTimers: true,
        toFake: ['setTimeout', 'setInterval', 'Date', 'performance'] as const,
    },
    optimizeDeps: ['@effect/vitest', 'rfc6902', 'effect'],
    output: {
        chaiConfig: { includeStack: true, showDiff: true, truncateThreshold: 0 },
        diff: { expand: true, truncateThreshold: 0 },
        outputFile: {
            blob: path.resolve(_ARTIFACTS.results, '.vitest-reports'),
            json: path.resolve(_ARTIFACTS.results, 'results.json'),
            junit: path.resolve(_ARTIFACTS.results, 'junit.xml'),
        },
    },
    patterns: {
        benchExclude: ['**/node_modules/**', '**/dist/**', '**/.cache/**', '**/*.browser.bench.{ts,tsx}'],
        benchInclude: ['**/*.bench.{ts,tsx}'],
        browserBenchInclude: ['tests/typescript/**/*.browser.bench.{ts,tsx}', 'libs/typescript/**/*.browser.bench.{ts,tsx}'],
        browserInclude: ['tests/typescript/**/*.browser.{test,spec}.{ts,tsx}', 'libs/typescript/**/*.browser.{test,spec}.{ts,tsx}'],
        coverageExclude: [
            '**/*.config.*',
            '**/*.d.ts',
            '**/__mocks__/**',
            '**/__tests__/**',
            '**/dist/**',
            '**/node_modules/**',
            '**/gen/**',
            '**/test/**',
            '**/tests/**',
        ],
        coverageInclude: ['libs/typescript/**/*.{ts,tsx,mts,cts}'],
        testExclude: ['**/node_modules/**', '**/dist/**', '**/.cache/**', '**/*.browser.{test,spec}.{ts,tsx}'],
        testInclude: ['**/*.{test,spec}.{ts,tsx,mts,cts}'],
    },
    reporters: {
        coverage: ['text', 'json', 'json-summary', 'html', 'lcov'] as const,
        test: _CI ? (['dot', 'json', 'junit', 'github-actions', 'blob'] as const) : (['tree'] as const),
    },
    setupFiles: [path.resolve(Root, 'tests/typescript/_testkit/setup.ts')],
    snapshot: { format: { printBasicPrototype: false } },
    timeouts: { hook: 10_000, slow: 5_000, test: 10_000 },
    workers: { max: '50%' },
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

/**
 * One package project: the node lane over the package's own tree, spine options baked in. The
 * project roots at its own package (estate anchors ride module-relative import.meta.glob, never
 * root-slash), so discovery is package-scoped by construction.
 */
const createProject = (dir: string): { test: ProjectTest } => ({
    test: {
        benchmark: {
            exclude: [..._CONFIG.patterns.benchExclude],
            include: [..._CONFIG.patterns.benchInclude],
            // autosave: every bench run feeds the sustained-regression ledger
            outputJson: path.resolve(_ARTIFACTS.bench, `${path.basename(dir)}.json`),
        },
        deps: { ..._CONFIG.deps },
        environment: 'node',
        exclude: [..._CONFIG.patterns.testExclude],
        fakeTimers: { ..._CONFIG.fakeTimers, toFake: [..._CONFIG.fakeTimers.toFake] },
        hookTimeout: _CONFIG.timeouts.hook,
        include: [..._CONFIG.patterns.testInclude],
        isolate: true,
        name: path.basename(dir),
        pool: 'threads',
        restoreMocks: true,
        sequence: { concurrent: false, hooks: 'stack', shuffle: _CI },
        setupFiles: [..._CONFIG.setupFiles],
        slowTestThreshold: _CONFIG.timeouts.slow,
        snapshotFormat: { ..._CONFIG.snapshot.format },
        testTimeout: _CONFIG.timeouts.test,
        unstubEnvs: true,
        unstubGlobals: true,
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

const config: ViteUserConfig = defineConfig({
    cacheDir: _CONFIG.cacheDir,
    optimizeDeps: { include: [..._CONFIG.optimizeDeps] },
    test: {
        allowOnly: !_CI,
        chaiConfig: { ..._CONFIG.output.chaiConfig },
        coverage: {
            clean: true,
            cleanOnRerun: true,
            enabled: false,
            exclude: [..._CONFIG.patterns.coverageExclude],
            include: [..._CONFIG.patterns.coverageInclude],
            provider: 'v8',
            reporter: [..._CONFIG.reporters.coverage],
            reportOnFailure: true,
            reportsDirectory: _ARTIFACTS.coverage,
            skipFull: true,
            thresholds: {
                branches: 95,
                functions: 95,
                lines: 95,
                perFile: true,
                statements: 95,
            },
        },
        diff: { ..._CONFIG.output.diff },
        fileParallelism: true,
        // Watch reruns track the gauge inputs too: container pins feed the harness lanes, grit rules feed the admission live-fire.
        forceRerunTriggers: [
            '**/package.json/**',
            '**/{vitest,vite}.config.*/**',
            '**/tsconfig*.json',
            '**/tests/containers.json',
            '**/tools/biome/*.grit',
        ],
        hideSkippedTests: _CI,
        maxWorkers: _CONFIG.workers.max,
        // stderr passes through: a failing lane's diagnostics are evidence, never noise to blanket-drop.
        onConsoleLog: (log) => !log.includes('Download the React DevTools'),
        outputFile: { ..._CONFIG.output.outputFile },
        passWithNoTests: false,
        printConsoleTrace: false,
        projects: [
            'tests/typescript/*/vitest.config.ts',
            'libs/typescript/*/vitest.config.ts',
            'libs/typescript/ui/*/vitest.config.ts',
            'apps/*/*/vitest.config.ts',
            {
                extends: true,
                test: {
                    // The browser bench include pins the lane to its own dialect: without it, bench mode
                    // falls back to the default glob and sweeps node-only benches into chromium.
                    benchmark: { include: [..._CONFIG.patterns.browserBenchInclude] },
                    browser: {
                        enabled: true,
                        headless: true,
                        instances: [{ browser: 'chromium' }],
                        provider: playwright(),
                    },
                    // The lane is armed, not red: it activates the day the first *.browser.spec lands.
                    include: [..._CONFIG.patterns.browserInclude],
                    name: 'browser',
                    // The one boot file serves both lanes: structural toEqual equality holds in browser
                    // specs from day one, and the node-only socket default self-gates.
                    setupFiles: [..._CONFIG.setupFiles],
                },
            },
        ],
        reporters: [..._CONFIG.reporters.test],
        retry: _CI ? 2 : 0,
        silent: 'passed-only',
    },
});

export default config;
export { createProject };
