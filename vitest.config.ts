import { NodeServices } from '@effect/platform-node';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import { playwright } from '@vitest/browser-playwright';
import { Array, Config, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { loadConfigFromFile } from 'vite';
import { mergeConfig, type ViteUserConfig, type ViteUserConfigFnPromise } from 'vitest/config';
import { parse } from 'yaml';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = import.meta.dirname;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _project = Effect.fnUntraced(function* (directory: string) {
    const [fs, path, ci] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.withDefault(Config.Boolean('CI'), false)]);
    const { name, devDependencies } = yield* Schema.decodeEffect(Schema.fromJsonString(Schema.Struct({ name: Schema.String, devDependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) })))(yield* fs.readFileString(path.join(directory, 'package.json')));
    const artifacts = path.join(_ROOT, '.artifacts', 'typescript', 'vitest', name);
    return {
        test: {
            attachmentsDir: path.join(artifacts, 'attachments'),
            ...(Record.has(devDependencies, '@vitest/browser-playwright')
                ? {
                      browser: {
                          provider: playwright(),
                          headless: true,
                          instances: [{ browser: 'chromium' }],
                          locators: { errorFormat: 'aria' },
                          trace: { mode: 'retain-on-failure', tracesDir: path.join(artifacts, 'traces') },
                      },
                  }
                : {}),
            chaiConfig: { includeStack: true, truncateThreshold: 0 },
            coverage: {
                enabled: true,
                exclude: ['**/*.d.ts', '**/*.config.ts', '**/tests/**', '**/*.stories.tsx', '**/.storybook/**'],
                include: ['**/*.{ts,tsx,mts,cts}'],
                reporter: ['text', 'html'],
                reportOnFailure: true,
                reportsDirectory: path.join(_ROOT, '.artifacts', 'typescript', 'coverage', name),
                skipFull: true,
            },
            fsModuleCachePath: path.join(_ROOT, '.cache', 'vitest', name, 'modules'),
            hideSkippedTests: ci,
            maxWorkers: '50%',
            pool: 'threads',
            reporters: ci ? ['dot', 'github-actions'] : ['tree'],
            sequence: { shuffle: ci },
            silent: 'passed-only',
            slowTestThreshold: 5000,
            testTimeout: 10_000,
        },
    } satisfies ViteUserConfig;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createVitestConfig =
    (directory: string, config: ViteUserConfig): ViteUserConfigFnPromise =>
    (env) =>
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const storybook = path.join(directory, '.storybook');
            const [app, project, stories] = yield* Effect.all([Effect.promise(() => loadConfigFromFile(env, undefined, directory)), _project(directory), fs.exists(path.join(storybook, 'main.ts'))], { concurrency: 'unbounded' });
            const layers = [
                ...Option.toArray(Option.map(Option.fromNullishOr(app), ({ config: loaded }) => loaded)),
                project,
                config,
                ...(stories ? [{ test: { projects: [{ plugins: [storybookTest({ configDir: storybook })], test: { name: 'storybook', browser: { enabled: true }, api: { allowWrite: true } } }] } }] : []),
            ];
            return layers.reduce((merged: ViteUserConfig, layer) => mergeConfig(merged, layer), {});
        }).pipe(Effect.provide(NodeServices.layer), Effect.runPromise);

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
export default rootConfig;
