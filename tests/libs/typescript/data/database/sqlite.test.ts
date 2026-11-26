import { layer as services } from '@effect/platform-node/NodeServices';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { assert, layer } from '@effect/vitest';
import { Catalog } from '@rasm/database/catalog';
import { Relation } from '@rasm/database/relation';
import { Sqlite } from '@rasm/database/sqlite';
import { Effect, FileSystem, Function, Layer, Predicate, Schema } from 'effect';
import { SqlError } from 'effect/sql';
import { pending, reconciling } from '#/reconciling';
import { type Relational, rows } from '#/rows';
import { storage, tables } from '#/storage.sqlite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const store: Relational = { tables, fault: (error) => (SqlError.isSqlError(error) && Predicate.hasProperty(error.reason.cause, 'errcode') ? error.reason.cause.errcode : error), check: 275, unique: 2067, reference: 787, command: Function.identity, isolated: 4 };

// --- [MODELS] --------------------------------------------------------------------------

const records = Relation.make('_cf_records', { id: Schema.String });

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(
    Sqlite.layer((names) => SqliteClient.layer({ filename: ':memory:', ...names })),
    { buildPerTest: true, excludeTestServices: true },
)('sqlite', (it) => {
    reconciling(it, store);
    rows(it, store);
    storage(it, store);
    it.effect(
        'reopens a database file with its rows and a table named under the prefix D1 reserves',
        Effect.fnUntraced(function* () {
            const filename = yield* FileSystem.FileSystem.use((fs) => fs.makeTempFileScoped({ suffix: '.sqlite' }));
            const catalog = Catalog.make({ tables: [records] });
            const opened = Catalog.layer(
                catalog,
                Sqlite.layer((names) => SqliteClient.layer({ filename, ...names })),
            );
            yield* Relation.insert(records, [{ id: 'saved' }]).pipe(Effect.provide(opened));
            assert.deepStrictEqual(yield* Effect.all({ rows: Relation.select(records, { id: ['saved'] }), pending: pending(catalog) }).pipe(Effect.provide(Layer.fresh(opened))), {
                rows: [{ id: 'saved' }],
                pending: { statements: [], concurrent: [] },
            });
        }, Effect.provide(services)),
    );
});
