import { layer } from '@effect/platform-node/NodeServices';
import { Array, Config, Effect, FileSystem, flow, Option, Path, Record } from 'effect';
import type { ConfigEnv } from 'vite';
import { configDefaults, mergeConfig, type ViteUserConfig, type ViteUserConfigFn, type ViteUserConfigFnPromise } from 'vitest/config';
import { Package, suitePath, VITEST_INTEGRATION_TESTS, Workspace } from '#/eng/typescript/manifest';
import { located } from '#/vite.shared';

// --- [TYPES] ---------------------------------------------------------------------------

type ProjectConfig = ViteUserConfig | ViteUserConfigFn;

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = import.meta.dirname;

// --- [OPERATIONS] ----------------------------------------------------------------------

const defaults = Effect.fnUntraced(function* (directory: string, mode: string) {
    const [path, ci, manifest] = yield* Effect.all([Path.Path, Config.withDefault(Config.Boolean('CI'), false), Effect.when(Package.read(directory), Effect.succeed(directory !== ROOT))]);
    const results = located(path.join('.artifacts', 'typescript', 'vitest'), directory);
    const integration = mode === 'integration';
    return {
        attachmentsDir: path.join(results, 'attachments'),
        ...(Option.exists(manifest, ({ devDependencies }) => Record.has(devDependencies, '@vitest/browser-playwright'))
            ? { browser: { provider: (yield* Effect.promise(() => import('@vitest/browser-playwright'))).playwright(), headless: true, instances: [{ browser: 'chromium' }], locators: { errorFormat: 'aria' }, trace: { mode: 'retain-on-failure', tracesDir: path.join(results, 'traces') } } }
            : {}),
        chaiConfig: { includeStack: true, truncateThreshold: 0 },
        dir: path.join(ROOT, suitePath(path, path.join('tests', 'typescript'), path.relative(ROOT, directory))),
        coverage: {
            exclude: ['**/*.d.ts', '**/*.config.ts', '**/tests/**', '**/.storybook/**'],
            include: ['**/*.{ts,tsx}'],
            reporter: ['text', 'html'],
            reportOnFailure: true,
            reportsDirectory: path.join(results, integration ? 'integration-coverage' : 'coverage'),
            skipFull: true,
        },
        fsModuleCachePath: path.join(located(path.join('.cache', 'vitest'), directory), 'modules'),
        hideSkippedTests: ci,
        maxWorkers: '50%',
        outputFile: { junit: path.join(results, integration ? 'integration.xml' : 'test.xml') },
        pool: 'threads',
        reporters: [...configDefaults.reporters, 'junit'],
        sequence: { shuffle: ci },
        silent: 'passed-only',
        slowTestThreshold: 5000,
        testTimeout: 10_000,
    } satisfies NonNullable<ViteUserConfig['test']>;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const vitestProject = (directory: string, { portable = {}, integration }: Readonly<{ portable?: ProjectConfig; integration?: ProjectConfig }> = {}): ViteUserConfigFnPromise =>
    flow(
        Effect.fnUntraced(function* (env: ConfigEnv) {
            const { mode } = env;
            const config = mode === 'integration' ? integration : portable;
            if (config === undefined) {
                return { root: directory, test: { include: [] } };
            }
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const storybook = path.join(directory, '.storybook');
            const [base, project, stories] = yield* Effect.all(
                [
                    defaults(directory, mode),
                    typeof config === 'function' ? Effect.promise(() => Promise.resolve(config(env))) : Effect.succeed(config),
                    Effect.when(
                        Effect.promise(() => import('@storybook/addon-vitest/vitest-plugin')),
                        mode === 'integration' ? Effect.succeed(false) : fs.exists(path.join(storybook, 'main.ts')),
                    ),
                ],
                { concurrency: 'unbounded' },
            );
            const selection = mode === 'integration' ? { include: [VITEST_INTEGRATION_TESTS] } : { exclude: [...configDefaults.exclude, VITEST_INTEGRATION_TESTS] };
            const configured = mergeConfig({ root: directory, test: { ...base, ...selection } }, project);
            return Option.match(stories, {
                onNone: () => configured,
                onSome: ({ storybookTest }) => mergeConfig(configured, { test: { projects: [{ plugins: [storybookTest({ configDir: storybook })], test: { name: 'storybook', dir: storybook, browser: { enabled: true }, api: { allowWrite: true } } }] } }),
            });
        }, Effect.provide(layer)),
        Effect.runPromise,
    );

const rootConfig: ViteUserConfigFnPromise = flow(
    Effect.fnUntraced(function* ({ mode }: ConfigEnv) {
        const [test, { packages }] = yield* Effect.all([defaults(ROOT, mode), Workspace.read(ROOT)], { concurrency: 'unbounded' });
        return { test: { ...test, coverage: { ...test.coverage, include: Array.cartesianWith(packages, test.coverage.include, (glob, pattern) => `${glob}/${pattern}`) }, projects: packages.map((glob) => `${glob}/vitest.config.ts`) } };
    }, Effect.provide(layer)),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { vitestProject };
export default rootConfig;
