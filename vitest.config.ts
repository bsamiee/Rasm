import { NodeServices } from '@effect/platform-node';
import { Array, Config, Effect, FileSystem, flow, Path, Schema } from 'effect';
import { configDefaults, type ViteUserConfig } from 'vitest/config';
import { parse } from 'yaml';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = import.meta.dirname;
const _ARTIFACTS = `${_ROOT}/.artifacts/typescript`;
const _EXCLUDE = [...configDefaults.exclude, '**/.cache/**', '**/.artifacts/**', '**/.archive/**'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const _project = Effect.fnUntraced(
    function* (directory: string) {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const { name } = yield* Schema.decodeEffect(Schema.fromJsonString(Schema.Struct({ name: Schema.String })))(yield* fs.readFileString(path.join(directory, 'package.json')));
        const ci = yield* Config.withDefault(Config.Boolean('CI'), false);
        const results = `${_ARTIFACTS}/test-results/${name}`;
        return {
            root: directory,
            cacheDir: `${_ROOT}/.cache/vitest/${name}`,
            test: {
                benchmark: { exclude: _EXCLUDE, include: ['**/*.bench.{ts,tsx}'] },
                chaiConfig: { includeStack: true, truncateThreshold: 0 },
                coverage: {
                    enabled: true,
                    exclude: [..._EXCLUDE, '**/*.config.*', '**/*.d.ts', '**/__mocks__/**', '**/__tests__/**', '**/gen/**', '**/test/**', '**/tests/**'],
                    include: ['**/*.{ts,tsx,mts,cts}'],
                    reporter: ['text', 'json', 'json-summary', 'html', 'lcov'],
                    reportOnFailure: true,
                    reportsDirectory: `${_ARTIFACTS}/coverage/${name}`,
                    skipFull: true,
                },
                exclude: _EXCLUDE,
                fakeTimers: { toFake: ['setTimeout', 'setInterval', 'Date', 'performance'] },
                hideSkippedTests: ci,
                include: ['**/*.{test,spec}.{ts,tsx,mts,cts}'],
                maxWorkers: '50%',
                name,
                outputFile: { blob: `${_ARTIFACTS}/test-results/.vitest-reports/${name}.json`, json: `${results}/results.json`, junit: `${results}/junit.xml` },
                pool: 'threads',
                reporters: ci ? ['dot', 'json', 'junit', 'github-actions', 'blob'] : ['tree', 'blob'],
                restoreMocks: true,
                sequence: { shuffle: ci },
                setupFiles: [`${_ROOT}/tests/typescript/support/setup.ts`],
                silent: 'passed-only',
                slowTestThreshold: 5000,
                testTimeout: 10_000,
                unstubEnvs: true,
                unstubGlobals: true,
            },
        } satisfies ViteUserConfig;
    },
    Effect.orDie,
    Effect.provide(NodeServices.layer),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const createVitestConfig: (directory: string) => Promise<ViteUserConfig> = flow(_project, Effect.runPromise);

const rootConfig = (): Promise<ViteUserConfig> =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const project = yield* _project(_ROOT);
        const workspace = yield* Schema.decodeUnknownEffect(Schema.Struct({ packages: Schema.Array(Schema.String) }))(parse(yield* fs.readFileString(`${_ROOT}/pnpm-workspace.yaml`)));
        return {
            ...project,
            test: {
                ...project.test,
                coverage: { ...project.test.coverage, clean: false, reporter: ['lcovonly', 'json'], reportsDirectory: `${_ARTIFACTS}/coverage` },
                projects: Array.map(workspace.packages, (glob) => `${glob}/vitest.config.ts`),
                reporters: Array.filter(project.test.reporters, (reporter) => reporter !== 'blob'),
            },
        };
    }).pipe(Effect.orDie, Effect.provide(NodeServices.layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createVitestConfig };
export default rootConfig;
