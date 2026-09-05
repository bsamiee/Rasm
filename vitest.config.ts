// --- [IMPORTS] -------------------------------------------------------------------------

import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { Array, Boolean, Config, Effect, Schema } from 'effect';
import type { ViteUserConfig } from 'vitest/config';
import { parse } from 'yaml';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = import.meta.dirname;
const _ARTIFACTS = `${_ROOT}/.artifacts/typescript`;
const _TIMEOUTS = { slow: 5000, test: 10_000 };
const _RETRIES = { ci: 2, local: 0 };
const _EXCLUDE = ['**/node_modules/**', '**/dist/**', '**/.cache/**'];
const _COVERAGE_EXCLUDE = [
    '**/*.config.*',
    '**/*.d.ts',
    '**/__mocks__/**',
    '**/__tests__/**',
    '**/dist/**',
    '**/node_modules/**',
    '**/gen/**',
    '**/test/**',
    '**/tests/**',
];
const _REPORTERS = { ci: ['dot', 'json', 'junit', 'github-actions', 'blob'], local: ['tree', 'blob'] } as const;

// --- [MODELS] --------------------------------------------------------------------------

const _Manifest = Schema.parseJson(Schema.Struct({ name: Schema.String }));
const _Workspace = Schema.Struct({ packages: Schema.Array(Schema.String) });

// --- [CONFIGURATION] -------------------------------------------------------------------

const _ci = Config.withDefault(Config.boolean('CI'), false);

const _reporters = (ci: boolean): (typeof _REPORTERS)[keyof typeof _REPORTERS] =>
    Boolean.match(ci, { onFalse: () => _REPORTERS.local, onTrue: () => _REPORTERS.ci });

// --- [OPERATIONS] ----------------------------------------------------------------------

// Every vitest.config.ts is its own root and @nx/vitest infers one test target per file
const _project = (directory: string): Effect.Effect<ViteUserConfig, never, FileSystem.FileSystem | Path.Path> =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const { name } = yield* Schema.decode(_Manifest)(yield* fs.readFileString(path.join(directory, 'package.json')));
        const ci = yield* _ci;
        const results = `${_ARTIFACTS}/test-results/${name}`;
        return {
            cacheDir: `${_ROOT}/.cache/vitest/${name}`,
            optimizeDeps: { include: ['@effect/vitest', 'effect'] },
            test: {
                benchmark: { exclude: _EXCLUDE, include: ['**/*.bench.{ts,tsx}'], outputJson: `${_ARTIFACTS}/bench/${name}.json` },
                chaiConfig: { includeStack: true, truncateThreshold: 0 },
                coverage: {
                    enabled: true,
                    exclude: _COVERAGE_EXCLUDE,
                    include: ['**/*.{ts,tsx,mts,cts}'],
                    reporter: ['text', 'json', 'json-summary', 'html', 'lcov'],
                    reportOnFailure: true,
                    reportsDirectory: `${_ARTIFACTS}/coverage/${name}`,
                    skipFull: true,
                },
                diff: { expand: true },
                exclude: _EXCLUDE,
                fakeTimers: { toFake: ['setTimeout', 'setInterval', 'Date', 'performance'] },
                hideSkippedTests: ci,
                include: ['**/*.{test,spec}.{ts,tsx,mts,cts}'],
                maxWorkers: '50%',
                name,
                onConsoleLog: (log) => !log.includes('Download the React DevTools'),
                outputFile: {
                    blob: `${_ARTIFACTS}/test-results/.vitest-reports/${name}.json`,
                    json: `${results}/results.json`,
                    junit: `${results}/junit.xml`,
                },
                pool: 'threads',
                reporters: Array.fromIterable(_reporters(ci)),
                restoreMocks: true,
                retry: Boolean.match(ci, { onFalse: () => _RETRIES.local, onTrue: () => _RETRIES.ci }),
                sequence: { shuffle: ci },
                setupFiles: [`${_ROOT}/tests/typescript/support/setup.ts`],
                silent: 'passed-only',
                slowTestThreshold: _TIMEOUTS.slow,
                testTimeout: _TIMEOUTS.test,
                unstubEnvs: true,
                unstubGlobals: true,
            },
        } satisfies ViteUserConfig;
    }).pipe(Effect.orDie);

// The root configuration lists every project, and the mutation runner and --merge-reports run from it
const _root: Effect.Effect<ViteUserConfig, never, FileSystem.FileSystem | Path.Path> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const ci = yield* _ci;
    const project = yield* _project(_ROOT);
    const workspace = yield* Schema.decodeUnknown(_Workspace)(parse(yield* fs.readFileString(`${_ROOT}/pnpm-workspace.yaml`)));
    return {
        ...project,
        test: {
            ...project.test,
            // Per-project report directories sit under the merged report directory
            coverage: { ...project.test?.coverage, clean: false, reporter: ['lcovonly', 'json'], reportsDirectory: `${_ARTIFACTS}/coverage` },
            // Every glob in the pnpm workspace names a project through the vitest.config.ts beside its manifest
            projects: Array.map(workspace.packages, (glob) => `${glob}/vitest.config.ts`),
            reporters: Array.filter(_reporters(ci), (reporter) => reporter !== 'blob'),
        },
    };
}).pipe(Effect.orDie);

// --- [COMPOSITION] ---------------------------------------------------------------------

const createVitestConfig = (directory: string): Promise<ViteUserConfig> => Effect.runPromise(Effect.provide(_project(directory), NodeContext.layer));

const rootConfig: Promise<ViteUserConfig> = Effect.runPromise(Effect.provide(_root, NodeContext.layer));

// --- [EXPORTS] -------------------------------------------------------------------------

export { createVitestConfig };
export default rootConfig;
