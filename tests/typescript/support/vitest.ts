import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { playwright } from '@vitest/browser-playwright';
import { Array, Effect, Exit, flow, Option, Predicate, Record, Result, Scope } from 'effect';
import type { Plugin } from 'vite';
import type { ProvidedContext, RunnerTask, RunnerTestCase } from 'vitest';
import { configDefaults, type ViteUserConfig } from 'vitest/config';
import type { TestProject } from 'vitest/node';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'vitest' {
    interface ProvidedContext {
        readonly artifacts: string;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const settings = (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && !Object.values(value).some(Predicate.isFunction);

const unstated = (policy: Record.ReadonlyRecord<string, unknown>, stated: Record.ReadonlyRecord<string, unknown>): Record.ReadonlyRecord<string, unknown> =>
    Record.filterMap(policy, (value, key) => {
        const set = stated[key];
        const nested = settings(value) && settings(set) ? Option.some<unknown>(unstated(value, set)) : Option.none();
        return Result.fromOption(set === undefined ? Option.some(value) : nested, () => set);
    });

const provided = <K extends keyof ProvidedContext & string, E>(key: K, acquire: Effect.Effect<ProvidedContext[K], E, Scope.Scope>) =>
    flow(
        Effect.fnUntraced(function* (project: TestProject) {
            const [scope, services] = yield* Effect.all([Scope.make(), Effect.context<never>()]);
            project.provide(key, yield* Effect.onError(Scope.provide(acquire, scope), (cause) => Scope.close(scope, Exit.failCause(cause))));
            return () => Effect.runPromiseWith(services)(Scope.close(scope, Exit.void));
        }),
        Effect.runPromise,
    );

const lost = (task: RunnerTask): boolean => task.result?.state === 'fail' || ('tasks' in task && task.tasks.some(lost));

const failed = Effect.map(
    Effect.promise(() => import('vitest')),
    ({ TestRunner }) => TestRunner.getCurrentTest<RunnerTestCase | undefined>()?.result?.state !== 'run' && lost(TestRunner.getCurrentSuite().file),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const testPolicy = (): Plugin => {
    const html = fileURLToPath(import.meta.resolve('@vitest/ui/reporter'));
    return {
        name: '@rasm/test-support/vitest',
        enforce: 'pre',
        config(config) {
            const policy = {
                browser: { provider: playwright(), headless: true, instances: [{ browser: 'chromium' }], locators: { errorFormat: 'aria' }, trace: { mode: 'retain-on-failure' } },
                chaiConfig: { includeStack: true, truncateThreshold: 0 },
                coverage: { allowExternal: true, reporter: ['text', 'html'], reportOnFailure: true, skipFull: true },
                reporters: [...configDefaults.reporters, 'junit', [html, {}]],
                silent: 'passed-only',
            } satisfies NonNullable<ViteUserConfig['test']>;
            return { resolve: { alias: { '@vitest/coverage-v8/browser': fileURLToPath(import.meta.resolve('@vitest/coverage-v8/browser')) } }, test: unstated(policy, { ...config.test }) };
        },
        configureVitest({ vitest, project }) {
            const artifacts = dirname(vitest.config.attachmentsDir);
            project.provide('artifacts', artifacts);
            project.config.browser.trace.tracesDir ??= join(artifacts, 'traces');
            Array.filterMap(vitest.config.reporters, (entry) => (Array.isArray(entry) && entry[0] === html ? Result.succeed(entry[1]) : Result.failVoid)).forEach((options) => Object.assign(options, { outputDir: join(artifacts, 'html'), ...options }));
        },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { failed, provided, testPolicy };
