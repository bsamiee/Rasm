import { layer } from '@effect/platform-node/NodeServices';
import { PgClient } from '@effect/sql-pg';
import { assert, inject, it } from '@effect/vitest';
import { Catalog, Relation, Tokenizer } from '@rasm/database';
import { Pg } from '@rasm/database/pg';
import { Store } from '@rasm/test-support/store';
import { failed } from '@rasm/test-support/vitest';
import { Effect, FileSystem, Layer, Option, Path, Schema, String, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { cases } from '#/cases';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'vitest' {
    interface ProvidedContext {
        readonly artifacts: string;
    }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const plans = [inject('artifacts'), 'sql', 'postgres'] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const engine = Layer.unwrap(Effect.map(Store.use(({ postgres }) => postgres), (config) => Pg.layer(config))).pipe(Layer.provide(Store.layer(undefined, failed)));

it.layer(
    Layer.effectDiscard(
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const folder = path.join(...plans);
            yield* fs.remove(folder, { recursive: true, force: true });
            yield* fs.makeDirectory(folder, { recursive: true });
        }),
    ).pipe(Layer.provideMerge(layer)),
    { concurrent: false, timeout: '5 minutes' },
)('postgres', (methods) => {
    cases(methods, engine, {
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
                const [sql, { uuidV7 }] = yield* Effect.all([PgClient.PgClient, SecureRandom.SecureRandom]);
                const app = sql('search_app');
                const extensions = sql('search_extensions');
                const runtime = `rasm-search-${yield* uuidV7()}`;
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
            },
            Effect.provide([engine, SecureRandom.layerWebCrypto]),
        ),
    );
});
