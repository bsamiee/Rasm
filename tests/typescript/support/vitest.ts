import { layer } from '@effect/platform-node/NodeServices';
import { playwright } from '@vitest/browser-playwright';
import { Config, Effect, FileSystem, flow, Option, Path } from 'effect';
import { type ConfigEnv, searchForWorkspaceRoot } from 'vite';
import { configDefaults, mergeConfig, type ViteUserConfig, type ViteUserConfigFn, type ViteUserConfigFnPromise } from 'vitest/config';
import { suitePath, VITEST_INTEGRATION_TESTS } from '#/suite.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type ProjectConfig = ViteUserConfig | ViteUserConfigFn;

// --- [OPERATIONS] ----------------------------------------------------------------------

const defaults = Effect.fnUntraced(function* (directory: string, mode: string) {
    const [path, ci] = yield* Effect.all([Path.Path, Config.withDefault(Config.Boolean('CI'), false)]);
    const root = searchForWorkspaceRoot(directory);
    const project = path.relative(root, directory);
    const results = path.join(root, '.artifacts', 'typescript', 'vitest', project);
    const integration = mode === 'integration';
    return {
        attachmentsDir: path.join(results, 'attachments'),
        browser: { provider: playwright(), headless: true, instances: [{ browser: 'chromium' }], locators: { errorFormat: 'aria' }, trace: { mode: 'retain-on-failure', tracesDir: path.join(results, 'traces') } },
        chaiConfig: { includeStack: true, truncateThreshold: 0 },
        dir: path.join(root, suitePath(path, path.join('tests', 'typescript'), project)),
        coverage: {
            exclude: ['**/*.d.ts', '**/*.config.ts', '**/tests/**', '**/.storybook/**'],
            include: ['**/*.{ts,tsx}'],
            reporter: ['text', 'html'],
            reportOnFailure: true,
            reportsDirectory: path.join(results, integration ? 'integration-coverage' : 'coverage'),
            skipFull: true,
        },
        fsModuleCachePath: path.join(root, '.cache', 'vitest', project, 'modules'),
        hideSkippedTests: ci,
        maxWorkers: '50%',
        outputFile: { junit: path.join(results, integration ? 'integration.xml' : 'test.xml') },
        pool: 'threads',
        provide: { artifacts: results },
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { defaults, vitestProject };
