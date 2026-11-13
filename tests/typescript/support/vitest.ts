import { Effect, Exit, flow, Scope } from 'effect';
import type { ProvidedContext, RunnerTask, RunnerTestCase } from 'vitest';
import type { TestProject } from 'vitest/node';

// --- [OPERATIONS] ----------------------------------------------------------------------

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

// --- [EXPORTS] -------------------------------------------------------------------------

export { failed, provided };
