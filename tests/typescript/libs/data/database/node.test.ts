import { layer } from '@effect/platform-node/NodeServices';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { assert, describe, it } from '@effect/vitest';
import { Catalog } from '@rasm/database/catalog';
import { Relation } from '@rasm/database/relation';
import { Effect, FileSystem, Layer, Path, Schema, Struct } from 'effect';
import { SqlClient } from 'effect/sql';
import { cases } from '#/tests/typescript/libs/data/database/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('sqlite', () => {
    cases(it, (options) => SqliteClient.layer({ filename: ':memory:', ...options }));
    it.effect(
        'reopens the SQLite file in WAL mode and retains application tables with a D1 reserved prefix',
        Effect.fnUntraced(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const directory = yield* fs.makeTempDirectoryScoped();
            const records = Relation.make('_cf_records', { id: Schema.String });
            const catalog = Catalog.make({ tables: [records] });
            const storage = Catalog.layer(catalog, (options) => SqliteClient.layer({ filename: path.join(directory, 'database.sqlite'), ...options }));
            const row = { id: 'saved' };
            yield* Relation.insert(records, [row]).pipe(Effect.provide(storage));
            const [stored, pending, journal] = yield* Effect.all([Relation.select(records, { id: [row.id] }), Effect.map(Catalog.plan(catalog), Struct.get('statements')), SqlClient.SqlClient.use((sql) => sql`PRAGMA journal_mode`)]).pipe(Effect.provide(Layer.fresh(storage)));
            assert.deepStrictEqual([stored, pending, journal], [[row], [], [{ journalMode: 'wal' }]]);
        }, Effect.provide(layer)),
    );
});
