import { NodeServices } from '@effect/platform-node';
import { Array, Config, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { loadConfigFromFile } from 'vite';
import { mergeConfig, type ViteUserConfig, type ViteUserConfigFnPromise } from 'vitest/config';
import { decodeFile, Package } from '#/eng/typescript/manifest';
import { Yaml } from '#/eng/typescript/parsed';
import { located } from '#/vite.shared';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = import.meta.dirname;

// --- [MODELS] --------------------------------------------------------------------------

const Workspace = Yaml.pipe(Schema.decodeTo(Schema.Struct({ packages: Schema.Array(Schema.String) })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const defaults = Effect.fnUntraced(function* (directory: string) {
    const [path, ci, { devDependencies }] = yield* Effect.all([Path.Path, Config.withDefault(Config.Boolean('CI'), false), Package.read(directory)]);
    const results = located(path.join('.artifacts', 'typescript', 'vitest'), directory);
    const tests = path.join(ROOT, 'tests', 'typescript');
    return {
        test: {
            attachmentsDir: path.join(results, 'attachments'),
            ...(Record.has(devDependencies, '@vitest/browser-playwright')
                ? { browser: { provider: (yield* Effect.promise(() => import('@vitest/browser-playwright'))).playwright(), headless: true, instances: [{ browser: 'chromium' }], locators: { errorFormat: 'aria' }, trace: { mode: 'retain-on-failure', tracesDir: path.join(results, 'traces') } } }
                : {}),
            chaiConfig: { includeStack: true, truncateThreshold: 0 },
            dir: path.join(
                tests,
                ...path
                    .relative(ROOT, directory)
                    .split(path.sep)
                    .filter((segment, at) => at !== 1 || segment !== path.basename(tests)),
            ),
            coverage: {
                exclude: ['**/*.d.ts', '**/*.config.ts', '**/tests/**', '**/.storybook/**'],
                include: ['**/*.{ts,tsx,mts,cts}'],
                reporter: ['text', 'html'],
                reportOnFailure: true,
                reportsDirectory: path.join(results, 'coverage'),
                skipFull: true,
            },
            fsModuleCachePath: path.join(located(path.join('.cache', 'vitest'), directory), 'modules'),
            hideSkippedTests: ci,
            maxWorkers: '50%',
            pool: 'threads',
            provide: { artifacts: results },
            reporters: ci ? ['dot', 'github-actions'] : ['tree'],
            sequence: { shuffle: ci },
            silent: 'passed-only',
            slowTestThreshold: 5000,
            testTimeout: 10_000,
        },
    } satisfies ViteUserConfig;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const project =
    (directory: string, config: ViteUserConfig = {}): ViteUserConfigFnPromise =>
    (env) =>
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const storybook = path.join(directory, '.storybook');
            const [app, base, stories] = yield* Effect.all(
                [
                    Effect.promise(() => loadConfigFromFile(env, undefined, directory)),
                    defaults(directory),
                    Effect.when(
                        Effect.promise(() => import('@storybook/addon-vitest/vitest-plugin')),
                        fs.exists(path.join(storybook, 'main.ts')),
                    ),
                ],
                { concurrency: 'unbounded' },
            );
            const configs = [
                ...Option.toArray(Option.fromNullishOr(app?.config)),
                base,
                config,
                ...Option.toArray(Option.map(stories, ({ storybookTest }) => ({ test: { projects: [{ plugins: [storybookTest({ configDir: storybook })], test: { name: 'storybook', dir: storybook, browser: { enabled: true }, api: { allowWrite: true } } }] } }))),
            ];
            return configs.reduce((merged: ViteUserConfig, next) => mergeConfig(merged, next), {});
        }).pipe(Effect.provide(NodeServices.layer), Effect.runPromise);

const rootConfig: () => Promise<ViteUserConfig> = Effect.fnUntraced(
    function* () {
        const [base, { packages }] = yield* Effect.all([defaults(ROOT), decodeFile(Workspace, ROOT, 'pnpm-workspace.yaml')], { concurrency: 'unbounded' });
        return {
            ...base,
            test: {
                ...base.test,
                coverage: { ...base.test.coverage, include: Array.cartesianWith(packages, base.test.coverage.include, (glob, pattern) => `${glob}/${pattern}`) },
                projects: Array.map(packages, (glob) => `${glob}/vitest.config.ts`),
            },
        };
    },
    Effect.provide(NodeServices.layer),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { project };
export default rootConfig;
