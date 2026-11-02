import { layer } from '@effect/vitest';
import { client } from '@rasm/database/pg';
import { PostgreSqlContainer, type StartedPostgreSqlContainer } from '@testcontainers/postgresql';
import { Context, Effect, Layer, Redacted } from 'effect';
import { PullPolicy } from 'testcontainers';
import { cases } from '#/tests/typescript/libs/data/database/cases';

// --- [SERVICES] ------------------------------------------------------------------------

class Postgres extends Context.Service<Postgres, StartedPostgreSqlContainer>()('rasm/tests/typescript/libs/data/database/postgres.test/Postgres') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const restored = () =>
    Layer.unwrap(
        Postgres.use((started) =>
            Effect.as(
                Effect.tryPromise(() => started.restoreSnapshot()),
                client({ url: Redacted.make(started.getConnectionUri()) }),
            ),
        ),
    );

layer(
    Layer.effect(
        Postgres,
        Effect.acquireDisposable(Effect.tryPromise(() => new PostgreSqlContainer('postgres:latest').withPullPolicy(PullPolicy.alwaysPull()).withEnvironment({ POSTGRES_INITDB_ARGS: '--locale-provider=builtin --builtin-locale=PG_UNICODE_FAST' }).start())).pipe(
            Effect.tap((started) => Effect.tryPromise(() => started.snapshot())),
        ),
    ),
    { timeout: '5 minutes' },
)('container', (methods) => cases(methods, restored, () => Effect.void));
