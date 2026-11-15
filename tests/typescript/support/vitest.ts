import { Effect, Exit, flow, Scope } from 'effect';
import type { ProvidedContext } from 'vitest';
import { configDefaults, type ViteUserConfig } from 'vitest/config';
import type { TestProject } from 'vitest/node';

// --- [CONSTANTS] -----------------------------------------------------------------------

const reporting: Pick<NonNullable<ViteUserConfig['test']>, 'chaiConfig' | 'reporters'> = { chaiConfig: { includeStack: true, truncateThreshold: 0 }, reporters: [...configDefaults.reporters, 'junit'] };

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

// --- [EXPORTS] -------------------------------------------------------------------------

export { provided, reporting };
