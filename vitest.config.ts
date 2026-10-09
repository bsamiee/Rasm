import { NodeServices } from '@effect/platform-node';
import { Array, Config, Effect, FileSystem, flow, Path, Schema } from 'effect';
import type { ViteUserConfig } from 'vitest/config';
import { parse } from 'yaml';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = import.meta.dirname;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _project = Effect.fnUntraced(function* (directory: string) {
    const [fs, path, ci] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.withDefault(Config.Boolean('CI'), false)]);
    const { name } = yield* Schema.decodeEffect(Schema.fromJsonString(Schema.Struct({ name: Schema.String })))(yield* fs.readFileString(path.join(directory, 'package.json')));
    return {
        cacheDir: path.join(_ROOT, '.cache', 'vitest', name),
        test: {
            chaiConfig: { includeStack: true, truncateThreshold: 0 },
            coverage: {
                enabled: true,
                exclude: ['**/*.d.ts', '**/tests/**'],
                include: ['**/*.{ts,tsx,mts,cts}'],
                reporter: ['text', 'html'],
                reportOnFailure: true,
                reportsDirectory: path.join(_ROOT, '.artifacts', 'typescript', 'coverage', name),
                skipFull: true,
            },
            hideSkippedTests: ci,
            maxWorkers: '50%',
            pool: 'threads',
            reporters: ci ? ['dot', 'github-actions'] : ['tree'],
            sequence: { shuffle: ci },
            setupFiles: [path.join(_ROOT, 'tests', 'typescript', 'support', 'setup.ts')],
            silent: 'passed-only',
            slowTestThreshold: 5000,
            testTimeout: 10_000,
        },
    } satisfies ViteUserConfig;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createVitestConfig: (directory: string) => Promise<ViteUserConfig> = flow(_project, Effect.provide(NodeServices.layer), Effect.runPromise);

const rootConfig: () => Promise<ViteUserConfig> = Effect.fnUntraced(
    function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [project, { packages }] = yield* Effect.all([_project(_ROOT), Effect.flatMap(Effect.map(fs.readFileString(path.join(_ROOT, 'pnpm-workspace.yaml')), parse), Schema.decodeUnknownEffect(Schema.Struct({ packages: Schema.Array(Schema.String) })))], { concurrency: 'unbounded' });
        return {
            ...project,
            test: {
                ...project.test,
                coverage: { ...project.test.coverage, include: Array.flatMap(packages, (glob) => Array.map(project.test.coverage.include, (pattern) => `${glob}/${pattern}`)) },
                projects: Array.map(packages, (glob) => `${glob}/vitest.config.ts`),
            },
        };
    },
    Effect.provide(NodeServices.layer),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createVitestConfig };
// biome-ignore lint/style/noDefaultExport: Vitest loads its configuration from the module default export.
export default rootConfig;
