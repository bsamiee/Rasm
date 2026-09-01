/// <reference types="vitest/config" />
import path from 'node:path';
import { defineConfig, type ViteUserConfig } from 'vitest/config';

// --- [TYPES] ---------------------------------------------------------------------------

type VitestProjectOptions = NonNullable<ViteUserConfig['test']>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const rootDirectory = import.meta.dirname;
const isCI = process.env['CI'] === 'true';
const artifacts = {
    benchmarks: path.resolve(rootDirectory, '.artifacts/typescript/bench'),
    coverage: path.resolve(rootDirectory, '.artifacts/typescript/coverage'),
    results: path.resolve(rootDirectory, '.artifacts/typescript/test-results'),
} as const;
const defaults = {
    cacheDir: '.cache/vitest',
    dependencies: { interopDefault: true },
    fakeTimers: {
        loopLimit: 10_000,
        shouldClearNativeTimers: true,
        toFake: ['setTimeout', 'setInterval', 'Date', 'performance'] as const,
    },
    optimizeDeps: ['@effect/vitest', 'effect'],
    output: {
        chaiConfig: {
            includeStack: true,
            showDiff: true,
            truncateThreshold: 0,
        },
        diff: { expand: true, truncateThreshold: 0 },
        outputFile: {
            blob: path.resolve(artifacts.results, '.vitest-reports'),
            json: path.resolve(artifacts.results, 'results.json'),
            junit: path.resolve(artifacts.results, 'junit.xml'),
        },
    },
    patterns: {
        benchmarkExclude: ['**/node_modules/**', '**/dist/**', '**/.cache/**'],
        benchmarkInclude: ['**/*.bench.{ts,tsx}'],
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
        testExclude: ['**/node_modules/**', '**/dist/**', '**/.cache/**'],
        testInclude: ['**/*.{test,spec}.{ts,tsx,mts,cts}'],
    },
    reporters: {
        coverage: ['text', 'json', 'json-summary', 'html', 'lcov'] as const,
        test: isCI
            ? (['dot', 'json', 'junit', 'github-actions', 'blob'] as const)
            : (['tree'] as const),
    },
    setupFiles: [
        path.resolve(rootDirectory, 'tests/typescript/support/setup.ts'),
    ],
    snapshot: { format: { printBasicPrototype: false } },
    timeouts: { hook: 10_000, slow: 5_000, test: 10_000 },
    workers: { max: '50%' },
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const createVitestProject = (
    directory: string,
): { test: VitestProjectOptions } => ({
    test: {
        benchmark: {
            exclude: [...defaults.patterns.benchmarkExclude],
            include: [...defaults.patterns.benchmarkInclude],
            outputJson: path.resolve(
                artifacts.benchmarks,
                `${path.basename(directory)}.json`,
            ),
        },
        deps: { ...defaults.dependencies },
        environment: 'node',
        exclude: [...defaults.patterns.testExclude],
        fakeTimers: {
            ...defaults.fakeTimers,
            toFake: [...defaults.fakeTimers.toFake],
        },
        hookTimeout: defaults.timeouts.hook,
        include: [...defaults.patterns.testInclude],
        isolate: true,
        name: path.basename(directory),
        pool: 'threads',
        restoreMocks: true,
        sequence: { concurrent: false, hooks: 'stack', shuffle: isCI },
        setupFiles: [...defaults.setupFiles],
        slowTestThreshold: defaults.timeouts.slow,
        snapshotFormat: { ...defaults.snapshot.format },
        testTimeout: defaults.timeouts.test,
        unstubEnvs: true,
        unstubGlobals: true,
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

const rootConfig: ViteUserConfig = defineConfig({
    cacheDir: defaults.cacheDir,
    optimizeDeps: { include: [...defaults.optimizeDeps] },
    test: {
        allowOnly: !isCI,
        chaiConfig: { ...defaults.output.chaiConfig },
        coverage: {
            clean: true,
            cleanOnRerun: true,
            enabled: false,
            exclude: [...defaults.patterns.coverageExclude],
            include: [...defaults.patterns.coverageInclude],
            provider: 'v8',
            reporter: [...defaults.reporters.coverage],
            reportOnFailure: true,
            reportsDirectory: artifacts.coverage,
            skipFull: true,
            thresholds: {
                branches: 95,
                functions: 95,
                lines: 95,
                perFile: true,
                statements: 95,
            },
        },
        diff: { ...defaults.output.diff },
        fileParallelism: true,
        forceRerunTriggers: [
            '**/package.json/**',
            '**/{vitest,vite}.config.*/**',
            '**/tsconfig*.json',
        ],
        hideSkippedTests: isCI,
        maxWorkers: defaults.workers.max,
        onConsoleLog: (log) => !log.includes('Download the React DevTools'),
        outputFile: { ...defaults.output.outputFile },
        passWithNoTests: false,
        printConsoleTrace: false,
        projects: [
            'tests/typescript/*/vitest.config.ts',
            'libs/typescript/*/vitest.config.ts',
            'libs/typescript/ui/*/vitest.config.ts',
            'apps/*/*/vitest.config.ts',
        ],
        reporters: [...defaults.reporters.test],
        retry: isCI ? 2 : 0,
        silent: 'passed-only',
    },
});

export default rootConfig;
export { createVitestProject };
