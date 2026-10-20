import { layer } from '@effect/vitest';
import { direct } from '@rasm/database/pg.ts';
import { PostgreSqlContainer } from '@testcontainers/postgresql';
import { type Cause, Context, Effect, Layer, Redacted } from 'effect';
import { PullPolicy } from 'testcontainers';
import { cases } from './cases.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class Postgres extends Context.Service<Postgres, { readonly restored: Effect.Effect<Redacted.Redacted, Cause.UnknownError> }>()('rasm/tests/typescript/libs/data/database/postgres.test/Postgres') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(
    Layer.effect(
        Postgres,
        Effect.map(
            Effect.acquireRelease(
                Effect.tryPromise(() => new PostgreSqlContainer('postgres:latest').withPullPolicy(PullPolicy.alwaysPull()).withEnvironment({ POSTGRES_INITDB_ARGS: '--locale-provider=builtin --builtin-locale=PG_UNICODE_FAST' }).start()).pipe(Effect.tap((started) => Effect.tryPromise(() => started.snapshot()))),
                (started) => Effect.promise(() => started.stop()),
            ),
            (container) => ({
                restored: Effect.as(
                    Effect.tryPromise(() => container.restoreSnapshot()),
                    Redacted.make(container.getConnectionUri()),
                ),
            }),
        ),
    ),
    { timeout: '5 minutes' },
)('container', (methods) =>
    cases(
        methods,
        () => Layer.unwrap(Postgres.use(({ restored }) => Effect.map(restored, direct))),
        () => Effect.void,
    ),
);
