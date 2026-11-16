import { Effect, Exit, flow, Scope } from 'effect';
import type { ProvidedContext } from 'vitest';
import type { ViteUserConfig } from 'vitest/config';
import type { TestProject } from 'vitest/node';

// --- [CONSTANTS] -----------------------------------------------------------------------

const reporting: Pick<NonNullable<ViteUserConfig['test']>, 'chaiConfig'> = { chaiConfig: { includeStack: true, truncateThreshold: 0 } };

// --- [OPERATIONS] ----------------------------------------------------------------------

const provided = <K extends Parameters<TestProject['provide']>[0], E>(key: K, acquire: Effect.Effect<ProvidedContext[K], E, Scope.Scope>) =>
    flow(
        Effect.fnUntraced(function* (project: TestProject) {
            const [scope, services] = yield* Effect.all([Scope.make(), Effect.context<never>()]);
            project.provide(key, yield* Effect.onError(Scope.provide(acquire, scope), (cause) => Scope.close(scope, Exit.failCause(cause))));
            return () => Effect.runPromiseWith(services)(Scope.close(scope, Exit.void));
        }),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { provided, reporting };
