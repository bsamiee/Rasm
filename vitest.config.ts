/// <reference types="vitest/config" />
/**
 * Root Vitest skeleton. The TS test estate is currently EMPTY: every former inline project matched
 * zero files and was deleted; per-package projects return with the TS buildout. Artifacts route to .artifacts/typescript.
 */

import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { defineConfig, type ViteUserConfig } from 'vitest/config';

// --- [TYPES] -----------------------------------------------------------------

type RuntimeEnv = NodeJS.ProcessEnv & {
    readonly CI?: string;
};

// --- [CONSTANTS] -------------------------------------------------------------

const Dirname = path.dirname(fileURLToPath(import.meta.url));
const _ENV: RuntimeEnv = process.env;
const _ARTIFACTS = {
    bench: path.resolve(Dirname, '.artifacts/typescript/bench'),
    coverage: path.resolve(Dirname, '.artifacts/typescript/coverage'),
    results: path.resolve(Dirname, '.artifacts/typescript/test-results'),
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
        benchInclude: ['**/*.bench.{ts,tsx}'],
        coverageExclude: [
            '**/*.config.*',
            '**/*.d.ts',
            '**/__mocks__/**',
            '**/__tests__/**',
            '**/dist/**',
            '**/node_modules/**',
            '**/test/**',
            '**/tests/**',
        ],
        coverageInclude: ['libs/typescript/**/src/**/*.{ts,tsx,mts,cts}'],
        testExclude: ['**/node_modules/**', '**/dist/**', '**/.cache/**'],
        testInclude: ['tests/typescript/**/*.{test,spec}.{ts,tsx,mts,cts}', 'libs/typescript/**/*.{test,spec}.{ts,tsx,mts,cts}'],
    },
    reporters: {
        coverage: ['text', 'json', 'json-summary', 'html', 'lcov'] as const,
        test: (_ENV.CI ? ['dot', 'json', 'junit', 'github-actions', 'blob'] : ['tree']) as readonly string[],
    },
    setupFiles: ['tests/typescript/_testkit/src/setup.ts'],
    snapshot: { format: { printBasicPrototype: false } },
    timeouts: { hook: 10_000, slow: 5_000, test: 10_000 },
    workers: { max: '50%' },
} as const;

// --- [EXPORTS] ---------------------------------------------------------------

const config: ViteUserConfig = defineConfig({
    cacheDir: _CONFIG.cacheDir,
    optimizeDeps: { include: [..._CONFIG.optimizeDeps] },
    test: {
        allowOnly: _ENV.CI !== 'true',
        benchmark: {
            exclude: [..._CONFIG.patterns.testExclude],
            include: [..._CONFIG.patterns.benchInclude],
            outputJson: path.resolve(_ARTIFACTS.bench, 'latest.json'), // autosave: every bench run feeds the sustained-regression ledger
        },
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
                perFile: false,
                statements: 95,
            },
        },
        deps: { ..._CONFIG.deps },
        diff: { ..._CONFIG.output.diff },
        exclude: [..._CONFIG.patterns.testExclude],
        fakeTimers: { ..._CONFIG.fakeTimers, toFake: [..._CONFIG.fakeTimers.toFake] },
        fileParallelism: true,
        forceRerunTriggers: ['**/package.json/**', '**/vitest.config.*/**', '**/tsconfig*.json'],
        globals: true,
        hideSkippedTests: _ENV.CI === 'true',
        hookTimeout: _CONFIG.timeouts.hook,
        include: [..._CONFIG.patterns.testInclude],
        isolate: true,
        maxWorkers: _CONFIG.workers.max,
        onConsoleLog: (log, type) => !log.includes('Download the React DevTools') && type !== 'stderr',
        outputFile: { ..._CONFIG.output.outputFile },
        passWithNoTests: false,
        pool: 'threads',
        printConsoleTrace: false,
        reporters: [..._CONFIG.reporters.test],
        restoreMocks: true,
        retry: _ENV.CI ? 2 : 0,
        sequence: { concurrent: false, hooks: 'stack', shuffle: _ENV.CI === 'true' },
        setupFiles: [..._CONFIG.setupFiles],
        silent: 'passed-only',
        slowTestThreshold: _CONFIG.timeouts.slow,
        snapshotFormat: { ..._CONFIG.snapshot.format },
        testTimeout: _CONFIG.timeouts.test,
        unstubEnvs: true,
        unstubGlobals: true,
    },
});

export default config;
