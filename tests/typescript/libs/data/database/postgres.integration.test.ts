import { layer } from '@effect/platform-node/NodeServices';
import { PgClient } from '@effect/sql-pg';
import { assert, inject, it } from '@effect/vitest';
import { Catalog, type Column, Relation } from '@rasm/database';
import { pgClient } from '@rasm/database/pg';
import { PostgreSqlContainer } from '@testcontainers/postgresql';
import { Context, Effect, FileSystem, Layer, Option, Path, Redacted, Schema, String, Struct } from 'effect';
import { Reactivity } from 'effect/reactivity';
import { SqlClient, type SqlError } from 'effect/sql';
import { PullPolicy } from 'testcontainers';
import { afterAll } from 'vitest';
import { Postgres } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { cases } from '#/tests/typescript/libs/data/database/cases';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'vitest' {
    interface ProvidedContext {
        readonly artifacts: string;
        readonly squawkConfig: string;
    }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const plans = [inject('artifacts'), 'sql', 'postgres'] as const;

// --- [SERVICES] ------------------------------------------------------------------------

class Container extends Context.Service<Container, { readonly restore: Effect.Effect<void, SqlError.SqlError>; readonly url: Redacted.Redacted }>()('rasm/tests/typescript/libs/data/database/postgres.integration.test/Container') {
    static client(options: typeof Column.transforms) {
        return Layer.unwrap(Container.use(({ restore, url }) => Effect.as(restore, pgClient({ url, ...options }))));
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(
    Layer.effect(
        Container,
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const folder = path.join(...plans);
            yield* fs.remove(folder, { recursive: true, force: true });
            yield* fs.makeDirectory(folder, { recursive: true });
            const started = yield* Effect.acquireDisposable(Effect.tryPromise(() => new PostgreSqlContainer(Postgres.image).withPullPolicy(PullPolicy.alwaysPull()).withEnvironment(Postgres.environment).withCommand(Postgres.command).start()));
            const url = Redacted.make(started.getConnectionUri());
            const admin = yield* PgClient.makeClient({ url, database: 'postgres', connectTimeout: '5 seconds', startupParameters: { statement_timeout: '5000' } }).pipe(Effect.provide(Reactivity.layer));
            const database = admin(started.getDatabase());
            const restore = admin`DROP DATABASE IF EXISTS ${database} WITH (FORCE)`.pipe(Effect.andThen(admin`CREATE DATABASE ${database}`), Effect.asVoid);
            return { restore, url };
        }),
    ).pipe(Layer.provideMerge(layer)),
    { concurrent: false, timeout: '5 minutes' },
)('container', (methods) => {
    afterAll(() => Path.Path.use((path) => runCommand('squawk', ['--config', inject('squawkConfig'), '--no-error-on-unmatched-pattern', path.join(...plans, '*.sql')])).pipe(Effect.asVoid, Effect.provide(layer), Effect.runPromise));
    cases(methods, Container.client, {
        postgres: true,
        record: Effect.fnUntraced(function* (title, { preamble, statements, concurrent }) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const text = ['BEGIN', ...preamble, ...statements, 'COMMIT', ...concurrent].map(String.concat(';\n')).join('');
            yield* fs.writeFileString(path.join(...plans, `${title.toLowerCase().replaceAll(/[^a-z0-9]+/gu, '-')}.sql`), text, { flag: 'a' });
        }),
    });
    methods.effect(
        'searches trigrams under an application role whose path excludes the extension schema',
        Effect.fnUntraced(
            function* () {
                const sql = yield* SqlClient.SqlClient;
                const app = sql('search_app');
                const extensions = sql('search_extensions');
                const runtime = 'search-runtime';
                const terms = Relation.make('terms', { id: Schema.String, text: Schema.String }, { search: { columns: ['text'], tokenizer: 'trigram' } });
                const row = { id: 'term', text: 'Timber frame' };
                yield* sql`CREATE SCHEMA ${app}`;
                yield* sql`CREATE SCHEMA ${extensions}`;
                yield* sql`CREATE EXTENSION pg_trgm WITH SCHEMA ${extensions}`;
                yield* sql`CREATE ROLE ${sql(runtime)} NOLOGIN`;
                yield* sql`GRANT USAGE ON SCHEMA ${app}, ${extensions} TO ${sql(runtime)}`;
                const catalog = Catalog.make({ tables: [terms] });
                yield* sql.withTransaction(
                    Effect.gen(function* () {
                        yield* sql`SET LOCAL search_path TO ${app}`;
                        yield* Catalog.reconcile(catalog, Option.some(runtime));
                        yield* Relation.insert(terms, [row]);
                    }),
                );
                yield* sql.withTransaction(
                    Effect.gen(function* () {
                        yield* sql`SET LOCAL ROLE ${sql(runtime)}`;
                        yield* sql`SET LOCAL search_path TO ${app}`;
                        assert.deepStrictEqual((yield* Relation.search(terms, 'imber', 1)).map(Struct.get('row')), [row]);
                    }),
                );
            },
            Effect.provide(Catalog.client(Container.client)),
        ),
    );
});
