import { layer } from '@effect/platform-node/NodeServices';
import { PgClient } from '@effect/sql-pg';
import { assert, inject, it } from '@effect/vitest';
import { Catalog, Relation, Tokenizer } from '@rasm/database';
import { Pg } from '@rasm/database/pg';
import { service } from '@rasm/test-support/compose';
import { Context, Effect, FileSystem, Layer, Option, Path, Record, Redacted, Schema, String, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { Reactivity } from 'effect/reactivity';
import { SqlClient, type SqlError } from 'effect/sql';
import { cases } from '#/cases';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'vitest' {
    interface ProvidedContext {
        readonly artifacts: string;
    }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const plans = [inject('artifacts'), 'sql', 'postgres'] as const;

// --- [SERVICES] ------------------------------------------------------------------------

class Database extends Context.Service<Database, { readonly name: string; readonly restore: Effect.Effect<void, SqlError.SqlError>; readonly url: Redacted.Redacted }>()('@rasm/test-database/postgres.test/Database') {
    static readonly engine = Layer.unwrap(Database.use(({ restore, url }) => Effect.as(restore, Pg.layer({ url }))));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(
    Layer.effect(
        Database,
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const folder = path.join(...plans);
            yield* fs.remove(folder, { recursive: true, force: true });
            yield* fs.makeDirectory(folder, { recursive: true });
            const postgres = yield* service('rasm', 'postgres');
            const [user, password] = yield* Effect.all([Effect.fromOption(Record.get(postgres.environment, 'POSTGRES_USER')), Effect.fromOption(Record.get(postgres.environment, 'POSTGRES_PASSWORD'))]);
            const at = (name: string) => Redacted.make(`postgres://${user}:${password}@${postgres.address}/${name}`);
            const admin = yield* PgClient.makeClient({ url: at('postgres'), connectTimeout: '5 seconds', startupParameters: { statement_timeout: '5000' } }).pipe(Effect.provide(Reactivity.layer));
            const database = `catalog-${yield* SecureRandom.SecureRandom.use(({ uuidV7 }) => uuidV7())}`;
            const dropped = admin`DROP DATABASE IF EXISTS ${admin(database)} WITH (FORCE)`;
            const restore = dropped.pipe(Effect.andThen(admin`CREATE DATABASE ${admin(database)}`), Effect.asVoid);
            yield* Effect.addFinalizer(() => Effect.orDie(Effect.andThen(dropped, admin`DROP ROLE IF EXISTS ${admin([database, 'search'].join('-'))}`)));
            return { name: database, restore, url: at(database) };
        }),
    ).pipe(Layer.provideMerge(Layer.merge(layer, SecureRandom.layerWebCrypto))),
    { concurrent: false, timeout: '5 minutes' },
)('postgres', (methods) => {
    cases(methods, Database.engine, {
        postgres: true,
        record: Effect.fnUntraced(function* (title, { preamble, statements, concurrent }) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const text = ['BEGIN', ...preamble, ...statements, 'COMMIT', ...concurrent].map(String.concat(';\n')).join('');
            yield* fs.writeFileString(path.join(...plans, `${title.toLowerCase().replaceAll(/[^a-z0-9]+/gu, '-')}.sql`), text, { flag: 'a' });
        }),
    });
    methods.effect(
        'searches trigrams under an application role whose path excludes the extension schema',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            const app = sql('search_app');
            const extensions = sql('search_extensions');
            const runtime = `${(yield* Database).name}-search`;
            const terms = Relation.make('terms', { id: Schema.String, text: Schema.String }, { search: { columns: ['text'], tokenizer: Tokenizer.Trigram() } });
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
        }, Effect.provide(Database.engine)),
    );
});
