import { assert, type Vitest } from '@effect/vitest';
import { Catalog, Column, type Engine, Relation, type Rows, Tokenizer } from '@rasm/database';
import { Array, Effect, Schema, Struct } from 'effect';
import { SqlClient } from 'effect/sql';
import { declared, pending, type Store, stored } from '#/reconciling';
import { values } from '#/rows';

// --- [MODELS] --------------------------------------------------------------------------

const keyed = Relation.make('keyed', { code: Schema.String, label: Schema.String }, { key: ['code'], withoutRowid: true });
const loose = Relation.make('loose', { id: Schema.String, count: Schema.Int });
const legacy = Relation.make('legacy', { id: Schema.String, count: Schema.Int, body: Schema.fromJsonString(Schema.Struct({ camelKey: Schema.Finite })) }, { retype: ['count', 'body'] });
const amounts = { id: Schema.Int, amount: Schema.Finite };
const emptied = Relation.make('empty_legacy', amounts, { retype: ['amount'] });
const used = Relation.make('used', amounts, { retype: ['amount'], renamedFrom: 'used_legacy' });
const reserved = Relation.make('reserved', { id: Schema.String, rowid: Schema.Int });
const lettered = Relation.make('notes', { id: Column.Uuid, title: Schema.String }, { search: { columns: ['title'], tokenizer: Tokenizer.Trigram() } });
const ledgers = Relation.make('ledgers', { id: Schema.String, tenant: Schema.String }, { tenant: { column: 'tenant', setting: 'app.tenant' } });
const handles = Relation.make('handles', { id: Schema.String, handle: Schema.String }, { indexes: [{ columns: [{ casefold: 'handle' }], unique: true }] });
const expiring = Relation.make('events', { id: Schema.String, at: Schema.DateTimeUtcFromString }, { mergeTree: { expires: { column: 'at', after: '1 day' } } });
const refusals: readonly { readonly label: string; readonly relation: Catalog['tables'][number]; readonly finding: object }[] = [
    { label: 'a column named as a rowid alias', relation: reserved, finding: { table: 'reserved', columns: ['rowid'], reason: { _tag: 'Unsupported', refusal: 'reserved' } } },
    { label: 'text search over a table whose key is no integer', relation: lettered, finding: { table: 'notes', columns: ['id'], reason: { _tag: 'Unsupported', refusal: 'search' } } },
    { label: 'a tenant scope', relation: ledgers, finding: { table: 'ledgers', columns: ['tenant'], reason: { _tag: 'Unsupported', refusal: 'tenant' } } },
    { label: 'a case-insensitive index over text that may hold non-ASCII', relation: handles, finding: { table: 'handles', columns: ['handle'], reason: { _tag: 'Unsupported', refusal: 'casefold' } } },
    { label: 'MergeTree options', relation: expiring, finding: { table: 'events', columns: ['at'], reason: { _tag: 'Unsupported', refusal: 'mergeTree' } } },
];
const settings = Relation.make('settings', { id: Schema.String, flag: Schema.BooleanFromBit, body: Schema.fromJsonString(Schema.Struct({ plan: Schema.String })) });
const whole = { id: 'a', flag: 1, body: '{"plan":"pro"}' };
const domains: readonly { readonly field: keyof typeof whole; readonly refused: string | number }[] = [
    { field: 'flag', refused: 2 },
    { field: 'body', refused: 'not JSON' },
];

// --- [OPERATIONS] ----------------------------------------------------------------------

const tables: Store['tables'] = SqlClient.SqlClient.use((sql) => Effect.map(sql`SELECT name FROM pragma_table_list WHERE schema = 'main' AND type = 'table' AND name NOT GLOB 'sqlite_*' AND name NOT GLOB '_cf_*' ORDER BY name`, Array.map(Struct.get('name'))));
const storage = (it: Vitest.MethodsNonLive<Engine | Rows | SqlClient.SqlClient>, store: Store) => {
    it.effect(
        'stores each schema kind in a STRICT column of the SQLite type that holds it, and a declared table without rowid',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            yield* Catalog.reconcile(declared([values, keyed]));
            assert.deepStrictEqual(
                yield* Effect.all({
                    columns: sql`SELECT name, type, "notnull" AS required FROM pragma_table_xinfo('values') ORDER BY cid`,
                    tables: sql`SELECT name, strict, wr AS keyed FROM pragma_table_list WHERE schema = 'main' AND name IN ('values', 'keyed') ORDER BY name`,
                }),
                {
                    columns: [
                        { name: 'id', type: 'TEXT', required: 1 },
                        { name: 'count', type: 'INTEGER', required: 1 },
                        { name: 'amount', type: 'TEXT', required: 1 },
                        { name: 'body', type: 'TEXT', required: 1 },
                        { name: 'bytes', type: 'BLOB', required: 1 },
                        { name: 'at', type: 'TEXT', required: 1 },
                        { name: 'flag', type: 'INTEGER', required: 1 },
                        { name: 'ratio', type: 'REAL', required: 1 },
                        { name: 'code', type: 'TEXT', required: 1 },
                        { name: 'note', type: 'TEXT', required: 0 },
                    ],
                    tables: [
                        { name: 'keyed', strict: 1, keyed: 1 },
                        { name: 'values', strict: 1, keyed: 0 },
                    ],
                },
            );
        }),
    );
    it.effect.each(domains)(
        'refuses $refused as $field',
        Effect.fnUntraced(function* ({ field, refused }) {
            const sql = yield* SqlClient.SqlClient;
            yield* Catalog.reconcile(declared([settings]));
            yield* sql`INSERT INTO settings ${sql.insert(whole)}`;
            const error = yield* Effect.flip(sql`UPDATE settings SET ${sql.update({ [field]: refused })}`);
            assert.deepStrictEqual({ fault: store.fault(error), stored: yield* stored('settings') }, { fault: store.check, stored: [whole] });
        }),
    );
    it.effect.each(refusals)(
        'refuses $label',
        Effect.fnUntraced(function* ({ relation, finding }) {
            const refused = yield* Effect.flip(Catalog.reconcile(declared([relation])));
            assert.deepStrictEqual({ refused: refused._tag === 'ReconcileError' ? refused.findings : refused, tables: yield* store.tables }, { refused: [finding], tables: [] });
        }),
    );
    it.effect(
        'rebuilds tables made without STRICT as STRICT, untyped columns declared for retype in their stored types, keeping rows',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            yield* sql`CREATE TABLE legacy (id TEXT NOT NULL PRIMARY KEY, count, body)`;
            yield* sql`CREATE TABLE loose (id TEXT NOT NULL PRIMARY KEY, count INTEGER NOT NULL)`;
            yield* sql`INSERT INTO legacy ${sql.insert({ id: 'l', count: 7, body: '{"camelKey":1}' })}`;
            yield* sql`INSERT INTO loose ${sql.insert({ id: 'k', count: 3 })}`;
            yield* Catalog.reconcile(declared([legacy, loose]));
            assert.deepStrictEqual(
                { rows: yield* Relation.select(legacy, { id: ['l'] }), loose: yield* stored('loose'), columns: yield* sql`SELECT name, type FROM pragma_table_xinfo('legacy') ORDER BY cid`, strict: yield* sql`SELECT name, strict FROM pragma_table_list WHERE name IN ('legacy', 'loose') ORDER BY name` },
                {
                    rows: [{ id: 'l', count: 7, body: { camelKey: 1 } }],
                    loose: [{ id: 'k', count: 3 }],
                    columns: [
                        { name: 'id', type: 'TEXT' },
                        { name: 'count', type: 'INTEGER' },
                        { name: 'body', type: 'TEXT' },
                    ],
                    strict: [
                        { name: 'legacy', strict: 1 },
                        { name: 'loose', strict: 1 },
                    ],
                },
            );
        }),
    );
    it.effect(
        'keeps AUTOINCREMENT and the highest key ever used when it rebuilds a table',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            const catalog = declared([emptied, used]);
            yield* sql`CREATE TABLE empty_legacy (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, amount INTEGER NOT NULL) STRICT`;
            yield* sql`CREATE TABLE used_legacy (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, amount INTEGER NOT NULL) STRICT`;
            yield* sql`INSERT INTO used_legacy (id, amount) VALUES (7, 42), (90, 12)`;
            yield* sql`DELETE FROM used_legacy WHERE id = 90`;
            yield* Catalog.reconcile(catalog);
            yield* sql`INSERT INTO empty_legacy (amount) VALUES (1.5)`;
            yield* sql`DELETE FROM empty_legacy`;
            yield* sql`INSERT INTO empty_legacy (amount) VALUES (2.5)`;
            yield* sql`INSERT INTO used (amount) VALUES (3.5)`;
            assert.deepStrictEqual(
                { emptied: yield* stored('empty_legacy'), used: yield* stored('used'), pending: yield* pending(catalog) },
                {
                    emptied: [{ id: 2, amount: 2.5 }],
                    used: [
                        { id: 7, amount: 42 },
                        { id: 91, amount: 3.5 },
                    ],
                    pending: { statements: [], concurrent: [] },
                },
            );
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { storage, tables };
