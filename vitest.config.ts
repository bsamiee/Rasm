/// <reference types="vitest/config" />
import { readFileSync } from 'node:fs';
import path from 'node:path';
import { Schema } from 'effect';
import { defineConfig, type ViteUserConfig } from 'vitest/config';

// --- [CONSTANTS] -----------------------------------------------------------------------

const rootDirectory = import.meta.dirname;
const isCI = process.env['CI'] === 'true';
const artifacts = {
    benchmarks: path.resolve(rootDirectory, '.artifacts/typescript/bench'),
    blobs: path.resolve(rootDirectory, '.artifacts/typescript/test-results/.vitest-reports'),
    coverage: path.resolve(rootDirectory, '.artifacts/typescript/coverage'),
    results: path.resolve(rootDirectory, '.artifacts/typescript/test-results'),
} as const;
const defaults = {
    cacheDir: path.resolve(rootDirectory, '.cache/vitest'),
    fakeTimers: { toFake: ['setTimeout', 'setInterval', 'Date', 'performance'] as const },
    optimizeDeps: ['@effect/vitest', 'effect'],
    output: {
        chaiConfig: { includeStack: true, truncateThreshold: 0 },
        diff: { expand: true },
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
        test: isCI ? (['dot', 'json', 'junit', 'github-actions', 'blob'] as const) : (['tree', 'blob'] as const),
    },
    setupFiles: [path.resolve(rootDirectory, 'tests/typescript/support/setup.ts')],
    timeouts: { slow: 5_000, test: 10_000 },
    workers: { max: '50%' },
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _Manifest = Schema.parseJson(Schema.Struct({ name: Schema.String }));

// The package name beside the config names the Vitest project as it names the Nx project, and every artifact path takes it
const _packageName = (directory: string): string =>
    Schema.decodeUnknownSync(_Manifest)(readFileSync(path.join(directory, 'package.json'), 'utf8')).name;

// Every vitest.config.ts is its own root, @nx/vitest infers one test target per file, and the artifacts split by package name
const createVitestConfig = (directory: string): ViteUserConfig => {
    const name = _packageName(directory);
    const results = path.resolve(artifacts.results, name);
    return defineConfig({
        cacheDir: path.resolve(defaults.cacheDir, name),
        optimizeDeps: { include: [...defaults.optimizeDeps] },
        test: {
            benchmark: {
                exclude: [...defaults.patterns.benchmarkExclude],
                include: [...defaults.patterns.benchmarkInclude],
                outputJson: path.resolve(artifacts.benchmarks, `${name}.json`),
            },
            chaiConfig: { ...defaults.output.chaiConfig },
            coverage: {
                enabled: true,
                exclude: [...defaults.patterns.coverageExclude],
                include: [...defaults.patterns.coverageInclude],
                reporter: [...defaults.reporters.coverage],
                reportOnFailure: true,
                reportsDirectory: path.resolve(artifacts.coverage, name),
                skipFull: true,
            },
            diff: { ...defaults.output.diff },
            exclude: [...defaults.patterns.testExclude],
            fakeTimers: { toFake: [...defaults.fakeTimers.toFake] },
            forceRerunTriggers: ['**/package.json/**', '**/{vitest,vite}.config.*/**', '**/tsconfig*.json'],
            hideSkippedTests: isCI,
            include: [...defaults.patterns.testInclude],
            maxWorkers: defaults.workers.max,
            name,
            onConsoleLog: (log) => !log.includes('Download the React DevTools'),
            outputFile: {
                blob: path.resolve(artifacts.blobs, `${name}.json`),
                json: path.resolve(results, 'results.json'),
                junit: path.resolve(results, 'junit.xml'),
            },
            pool: 'threads',
            reporters: [...defaults.reporters.test],
            restoreMocks: true,
            retry: isCI ? 2 : 0,
            sequence: { shuffle: isCI },
            setupFiles: [...defaults.setupFiles],
            silent: 'passed-only',
            slowTestThreshold: defaults.timeouts.slow,
            testTimeout: defaults.timeouts.test,
            unstubEnvs: true,
            unstubGlobals: true,
        },
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

// The root configuration lists every project, the mutation runner and --merge-reports run from it, and the merge rejects a blob reporter
const projectConfig: ViteUserConfig = createVitestConfig(rootDirectory);
const rootConfig: ViteUserConfig = defineConfig({
    ...projectConfig,
    test: {
        ...projectConfig.test,
        coverage: {
            ...projectConfig.test?.coverage,
            clean: false, // The per-project report directories sit under the merged report directory
            reporter: ['lcovonly', 'json'],
            reportsDirectory: artifacts.coverage,
        },
        // The pnpm-workspace.yaml packages globs, each with its vitest.config.ts
        projects: [
            'libs/typescript/*/vitest.config.ts',
            'libs/typescript/ui/*/vitest.config.ts',
            'apps/*/*/vitest.config.ts',
            'tests/typescript/*/vitest.config.ts',
        ],
        reporters: defaults.reporters.test.filter((reporter) => reporter !== 'blob'),
    },
});

export default rootConfig;
export { createVitestConfig };
