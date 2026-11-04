import { it } from '@effect/vitest';
import type { Column } from '@rasm/database';
import { pgPool } from '@rasm/database/pg';
import { PostgreSqlContainer } from '@testcontainers/postgresql';
import { type Cause, Context, Effect, Layer, Redacted } from 'effect';
import { PullPolicy } from 'testcontainers';
import { cases } from '#/tests/typescript/libs/data/database/cases';

// --- [SERVICES] ------------------------------------------------------------------------

class Container extends Context.Service<Container, { readonly restore: Effect.Effect<void, Cause.UnknownError>; readonly url: Redacted.Redacted }>()('rasm/tests/typescript/libs/data/database/postgres.test/Container') {
    static client(options: typeof Column.transforms) {
        return Layer.unwrap(Container.use(({ restore, url }) => Effect.as(restore, pgPool({ url, ...options }))));
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(
    Layer.effect(
        Container,
        Effect.acquireDisposable(Effect.tryPromise(() => new PostgreSqlContainer('postgres:latest').withPullPolicy(PullPolicy.alwaysPull()).withEnvironment({ POSTGRES_INITDB_ARGS: '--locale-provider=builtin --builtin-locale=PG_UNICODE_FAST' }).start())).pipe(
            Effect.tap((started) => Effect.tryPromise(() => started.snapshot())),
            Effect.map((started) => ({ restore: Effect.tryPromise(() => started.restoreSnapshot()), url: Redacted.make(started.getConnectionUri()) })),
        ),
    ),
    { timeout: '5 minutes' },
)('container', (methods) => cases(methods, Container.client, () => Effect.void));
