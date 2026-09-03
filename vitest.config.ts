/// <reference types="vitest/config" />
import path from 'node:path';
import { defineConfig, type ViteUserConfig } from 'vitest/config';

// --- [CONSTANTS] -----------------------------------------------------------------------

const rootDirectory = import.meta.dirname;
const isCI = process.env['CI'] === 'true';
const artifacts = {
    benchmarks: path.resolve(rootDirectory, '.artifacts/typescript/bench'),
    coverage: path.resolve(rootDirectory, '.artifacts/typescript/coverage'),
    results: path.resolve(rootDirectory, '.artifacts/typescript/test-results'),
} as const;
const defaults = {
    cacheDir: path.resolve(rootDirectory, '.cache/vitest'),
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
        coverageInclude: ['**/*.{ts,tsx,mts,cts}'],
        testExclude: ['**/node_modules/**', '**/dist/**', '**/.cache/**'],
        testInclude: ['**/*.{test,spec}.{ts,tsx,mts,cts}'],
    },
    reporters: {
        coverage: ['text', 'json', 'json-summary', 'html', 'lcov'] as const,
        test: isCI ? (['dot', 'json', 'junit', 'github-actions', 'blob'] as const) : (['tree'] as const),
    },
    setupFiles: [path.resolve(rootDirectory, 'tests/typescript/support/setup.ts')],
    snapshot: { format: { printBasicPrototype: false } },
    timeouts: { hook: 10_000, slow: 5_000, test: 10_000 },
    workers: { max: '50%' },
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

// Every vitest.config.ts is its own root, @nx/vitest infers one test target per file, and the artifacts split by the directory name
const createVitestConfig = (directory: string): ViteUserConfig => {
    const name = path.basename(directory);
    const results = path.resolve(artifacts.results, name);
    return defineConfig({
        cacheDir: path.resolve(defaults.cacheDir, name),
        optimizeDeps: { include: [...defaults.optimizeDeps] },
        test: {
            allowOnly: !isCI,
            benchmark: {
                exclude: [...defaults.patterns.benchmarkExclude],
                include: [...defaults.patterns.benchmarkInclude],
                outputJson: path.resolve(artifacts.benchmarks, `${name}.json`),
            },
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
                reportsDirectory: path.resolve(artifacts.coverage, name),
                skipFull: true,
                thresholds: {
                    branches: 95,
                    functions: 95,
                    lines: 95,
                    perFile: true,
                    statements: 95,
                },
            },
            deps: { ...defaults.dependencies },
            diff: { ...defaults.output.diff },
            environment: 'node',
            exclude: [...defaults.patterns.testExclude],
            fakeTimers: {
                ...defaults.fakeTimers,
                toFake: [...defaults.fakeTimers.toFake],
            },
            fileParallelism: true,
            forceRerunTriggers: ['**/package.json/**', '**/{vitest,vite}.config.*/**', '**/tsconfig*.json'],
            hideSkippedTests: isCI,
            hookTimeout: defaults.timeouts.hook,
            include: [...defaults.patterns.testInclude],
            isolate: true,
            maxWorkers: defaults.workers.max,
            name,
            onConsoleLog: (log) => !log.includes('Download the React DevTools'),
            outputFile: {
                blob: path.resolve(results, '.vitest-reports'),
                json: path.resolve(results, 'results.json'),
                junit: path.resolve(results, 'junit.xml'),
            },
            passWithNoTests: false,
            pool: 'threads',
            printConsoleTrace: false,
            reporters: [...defaults.reporters.test],
            restoreMocks: true,
            retry: isCI ? 2 : 0,
            sequence: { concurrent: false, hooks: 'stack', shuffle: isCI },
            setupFiles: [...defaults.setupFiles],
            silent: 'passed-only',
            slowTestThreshold: defaults.timeouts.slow,
            snapshotFormat: { ...defaults.snapshot.format },
            testTimeout: defaults.timeouts.test,
            unstubEnvs: true,
            unstubGlobals: true,
        },
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

// The root configuration serves the mutation runner, which runs every suite from the workspace root
const rootConfig: ViteUserConfig = createVitestConfig(rootDirectory);

export default rootConfig;
export { createVitestConfig };
