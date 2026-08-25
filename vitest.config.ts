/// <reference types="vitest/config" />
import path from 'node:path';
import { fileURLToPath } from 'node:url';
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
    optimizeDeps: ['@effect/vitest', 'effect'],
    output: {
        chaiConfig: {
            includeStack: true,
            showDiff: true,
            truncateThreshold: 0,
        },
        diff: { expand: true, truncateThreshold: 0 },
        outputFile: {
            blob: path.resolve(_ARTIFACTS.results, '.vitest-reports'),
            json: path.resolve(_ARTIFACTS.results, 'results.json'),
            junit: path.resolve(_ARTIFACTS.results, 'junit.xml'),
        },
    },
    patterns: {
        benchExclude: ['**/node_modules/**', '**/dist/**', '**/.cache/**'],
        benchInclude: ['**/*.bench.{ts,tsx}'],
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
        test: _CI
            ? (['dot', 'json', 'junit', 'github-actions', 'blob'] as const)
            : (['tree'] as const),
    },
    setupFiles: [path.resolve(Root, 'tests/typescript/testkit/setup.ts')],
    snapshot: { format: { printBasicPrototype: false } },
    timeouts: { hook: 10_000, slow: 5_000, test: 10_000 },
    workers: { max: '50%' },
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const createProject = (dir: string): { test: ProjectTest } => ({
    test: {
        benchmark: {
            exclude: [..._CONFIG.patterns.benchExclude],
            include: [..._CONFIG.patterns.benchInclude],
            outputJson: path.resolve(
                _ARTIFACTS.bench,
                `${path.basename(dir)}.json`,
            ),
        },
        deps: { ..._CONFIG.deps },
        environment: 'node',
        exclude: [..._CONFIG.patterns.testExclude],
        fakeTimers: {
            ..._CONFIG.fakeTimers,
            toFake: [..._CONFIG.fakeTimers.toFake],
        },
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
        forceRerunTriggers: [
            '**/package.json/**',
            '**/{vitest,vite}.config.*/**',
            '**/tsconfig*.json',
        ],
        hideSkippedTests: _CI,
        maxWorkers: _CONFIG.workers.max,
        onConsoleLog: (log) => !log.includes('Download the React DevTools'),
        outputFile: { ..._CONFIG.output.outputFile },
        passWithNoTests: false,
        printConsoleTrace: false,
        projects: [
            'tests/typescript/*/vitest.config.ts',
            'libs/typescript/*/vitest.config.ts',
            'libs/typescript/ui/*/vitest.config.ts',
            'apps/*/*/vitest.config.ts',
        ],
        reporters: [..._CONFIG.reporters.test],
        retry: _CI ? 2 : 0,
        silent: 'passed-only',
    },
});

export default config;
export { createProject };
