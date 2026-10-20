import { PgClient } from '@effect/sql-pg';
import { layer } from '@effect/vitest';
import { types } from '@rasm/database/pg.ts';
import type { names } from '@rasm/database/table.ts';
import { PostgreSqlContainer } from '@testcontainers/postgresql';
import { type Cause, Context, Effect, Function, Layer, Redacted } from 'effect';
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
        (options: typeof names) =>
            Layer.unwrap(
                Effect.map(
                    Postgres.use(({ restored }) => restored),
                    (url) => PgClient.layer({ url, ...options, types }),
                ),
            ),
        Function.identity,
        () => Effect.void,
    ),
);
