import { assert, type Vitest } from '@effect/vitest';
import { Catalog, Column, type Engine, Relation, Rows, Tokenizer } from '@rasm/database';
import { Array, BigDecimal, DateTime, Effect, Exit, Option, Order, Schema, Struct } from 'effect';
import { SqlClient } from 'effect/sql';
import { declared, pending, sampled, type Store, stored } from '#/reconciling';

// --- [TYPES] ---------------------------------------------------------------------------

interface Relational extends Store {
    readonly unique: unknown;
    readonly reference: unknown;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const volume = 3000;
const newest = 2;
const int64 = { minimum: -(2n ** 63n), maximum: 2n ** 63n - 1n };
const ids = ['01900000-0000-7000-8000-000000000000', '01900000-0000-7000-8000-000000000001', '01900000-0000-7000-8000-000000000002', '01900000-0000-7000-8000-000000000003'] as const;
const marks = { open: '[', close: ']' };

// --- [MODELS] --------------------------------------------------------------------------

const Text = Schema.String.check(Schema.makeFilter((text) => text.isWellFormed() && !text.includes('\0'), { expected: 'well-formed text without U+0000' }));
const parents = Relation.make('parents', { id: Schema.String, label: Schema.String });
const children = Relation.make('children', { id: Schema.String, parentId: Schema.String, rank: Schema.Int }, { references: [{ columns: ['parentId'], table: parents, onDelete: 'CASCADE' }] });
const annotated = Relation.make('parents', { ...parents.schema.fields, note: Schema.NullOr(Schema.String) });
const ranked = Relation.make('parents', { ...parents.schema.fields, label: Schema.Literals(['Parent', 'Guardian']) }, { retype: ['label'] });
const authors = Relation.make('authors', { id: Schema.String, favorite: Schema.NullOr(Schema.String) });
const books = Relation.make('books', { id: Schema.String, authorId: Schema.String }, { references: [{ columns: ['authorId'], table: authors }] });
const readers = Relation.make('authors', authors.schema.fields, { references: [{ columns: ['favorite'], table: books }] });
const values = Relation.make('values', {
    id: Column.Uuid,
    count: Schema.BigIntFromString.check(Schema.isBetweenBigInt(int64)),
    amount: Schema.BigDecimalFromString,
    body: Schema.fromJsonString(Schema.Struct({ camelKey: Schema.Finite, label: Text })),
    bytes: Column.Bytes,
    at: Schema.DateFromString.check(Schema.isBetweenDate({ minimum: DateTime.toDateUtc(DateTime.makeUnsafe('0001-01-01T00:00:00.000Z')), maximum: DateTime.toDateUtc(DateTime.makeUnsafe('9999-12-31T23:59:59.999Z')) })),
    flag: Schema.BooleanFromBit,
    ratio: Schema.Finite,
    code: Text,
    note: Schema.OptionFromNullOr(Text),
});
const Written = Schema.fromJsonString(Schema.Array(values.schema));
const Generated = Schema.NonEmptyArray(Schema.Struct(Struct.omit(values.schema.fields, ['id']))).check(Schema.isMaxLength(ids.length));
const events = Relation.make('events', { id: Schema.String, tenant: Schema.Int, at: Schema.DateTimeUtcFromString, kind: Schema.Literals(['view', 'buy']), note: Schema.NullOr(Schema.String) }, { indexes: [{ columns: ['tenant', 'at'] }] });
const accounts = Relation.make(
    'accounts',
    { id: Column.Uuid, email: Schema.String.check(Schema.isAscii()), deletedAt: Schema.NullOr(Schema.DateTimeUtcFromString), profile: Schema.fromJsonString(Schema.Struct({ plan: Schema.String })), plan: Schema.String, active: Schema.BooleanFromBit },
    {
        projections: { plan: { from: 'profile', path: ['plan'] }, active: { where: { deletedAt: null } } },
        indexes: [
            { columns: [{ casefold: 'email' }], unique: true, where: { deletedAt: null } },
            { columns: ['plan'], include: ['email'] },
        ],
    },
);
const searched = (tokenizer: Tokenizer) => ({ search: { columns: ['title', 'body'], tokenizer } }) as const;
const notes = Relation.make('notes', { id: Schema.Int, title: Schema.String, body: Schema.String }, searched(Tokenizer.Words({ configuration: 'simple', filters: ['unaccent'], tokenize: 'unicode61 remove_diacritics 2' })));
const fragments = Relation.make('fragments', notes.schema.fields, searched(Tokenizer.Trigram()));
const written = Array.make({ id: 1, title: 'Concrete shell', body: 'Thin vaults' }, { id: 2, title: 'Timber frame', body: 'Glulam beams' }, { id: 3, title: 'Café crème', body: 'Voûte mince' });
const searches: readonly { readonly relation: typeof notes; readonly text: string; readonly titles: readonly string[] }[] = [
    { relation: notes, text: 'vaults', titles: ['Concrete shell'] },
    { relation: notes, text: '"vaults', titles: ['Concrete shell'] },
    { relation: notes, text: 'cafe CREME', titles: ['Café crème'] },
    { relation: notes, text: 'shell thin', titles: ['Concrete shell'] },
    { relation: notes, text: 'vault', titles: [] },
    { relation: fragments, text: 'lulam', titles: ['Timber frame'] },
    { relation: fragments, text: 'am', titles: ['Timber frame'] },
    { relation: fragments, text: 'é', titles: ['Café crème'] },
    { relation: fragments, text: 'shell Thin', titles: [] },
    { relation: fragments, text: '%', titles: [] },
    { relation: fragments, text: '_', titles: [] },
];

// --- [OPERATIONS] ----------------------------------------------------------------------

const epoch = DateTime.makeUnsafe('2026-01-01T00:00:00.000Z');
const event = (id: string, tenant: number, seconds: number, kind: 'view' | 'buy' = 'view', note: string | null = null) => ({ id, tenant, at: DateTime.add(epoch, { seconds }), kind, note });
const account = (id: string, email: string, deleted: boolean) => ({ id, email, deletedAt: deleted ? epoch : null, profile: { plan: 'pro' } });
const named = Effect.map((found: readonly { readonly id: string }[]) => found.map(Struct.get('id')));
const rows = (it: Vitest.MethodsNonLive<Engine | Rows | SqlClient.SqlClient>, store: Relational) => {
    it.effect(
        'maps camelCase fields to snake_case columns in every statement form',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            yield* Catalog.reconcile(declared([parents, children]));
            yield* Rows.atomic([sql`INSERT INTO parents ${sql.insert({ id: 'p', label: 'Parent' })}`, sql`INSERT INTO children ${sql.insert({ id: 'a', parentId: 'p', rank: 1 })}`]);
            yield* sql`UPDATE children SET ${sql.update({ parentId: 'p', rank: 2 })} WHERE ${sql('parentId')} = ${'p'}`;
            assert.deepStrictEqual({ decoded: yield* Relation.select(children, { parentId: ['p'] }), raw: yield* sql.withoutTransforms()`SELECT parent_id FROM children` }, { decoded: [{ id: 'a', parentId: 'p', rank: 2 }], raw: [{ parent_id: 'p' }] });
        }),
    );
    it.effect(
        'refuses a row whose reference has no target and deletes rows with the target they cascade from',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            yield* Catalog.reconcile(declared([parents, children]));
            yield* Relation.insert(parents, [{ id: 'p', label: 'Parent' }]);
            yield* Relation.insert(children, [{ id: 'a', parentId: 'p', rank: 1 }]);
            const orphan = yield* Effect.flip(sql`INSERT INTO children ${sql.insert({ id: 'b', parentId: 'missing', rank: 2 })}`);
            yield* Relation.remove(parents, { id: ['p'] });
            assert.deepStrictEqual({ fault: store.fault(orphan), children: yield* stored('children') }, { fault: store.reference, children: [] });
        }),
    );
    it.effect(
        'reconciles tables that reference each other and enforces both references',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            const catalog = declared([readers, books]);
            yield* Catalog.reconcile(catalog);
            yield* Relation.insert(readers, [{ id: 'a', favorite: null }]);
            yield* Relation.insert(books, [{ id: 'b', authorId: 'a' }]);
            const faults = yield* Effect.forEach([sql`INSERT INTO books ${sql.insert({ id: 'c', authorId: 'missing' })}`, sql`UPDATE authors SET favorite = 'missing' WHERE id = 'a'`], (write) => Effect.map(Effect.flip(write), store.fault));
            assert.deepStrictEqual({ faults, pending: yield* pending(catalog) }, { faults: [store.reference, store.reference], pending: { statements: [], concurrent: [] } });
        }),
    );
    it.effect(
        'keeps rows when concurrent catalogs extend and retype one table',
        Effect.fnUntraced(function* () {
            yield* Catalog.reconcile(declared([parents, children]));
            yield* Relation.insert(parents, [{ id: 'p', label: 'Parent' }]);
            yield* Relation.insert(children, [{ id: 'a', parentId: 'p', rank: 1 }]);
            const [extended, retyped] = yield* Effect.all([Effect.exit(Catalog.reconcile(declared([annotated, children]))), Effect.exit(Catalog.reconcile(declared([ranked, children])))], { concurrency: 'unbounded' });
            assert.deepStrictEqual({ settled: Exit.isSuccess(extended) || Exit.isSuccess(retyped), parents: (yield* stored('parents')).map(Struct.pick(['id', 'label'])), children: yield* stored('children') }, { settled: true, parents: [{ id: 'p', label: 'Parent' }], children: [{ id: 'a', parentId: 'p', rank: 1 }] });
        }),
    );
    it.effect(
        'plans nothing after reconciling references, partial, covering, and case-insensitive indexes, computed columns, and text search',
        Effect.fnUntraced(function* () {
            const catalog = declared([parents, children, accounts, events, notes, fragments, values]);
            yield* Catalog.reconcile(catalog);
            assert.deepStrictEqual({ pending: yield* pending(catalog), undeclared: yield* Catalog.reconcile(catalog) }, { pending: { statements: [], concurrent: [] }, undeclared: { tables: [], columns: [], indexes: [], references: [] } });
        }),
    );
    it.effect(
        'stores 64-bit integer limits, exact decimals, empty and full bytes, and astral code points',
        Effect.fnUntraced(function* () {
            const limits = Array.make(
                {
                    id: ids[0],
                    count: int64.maximum,
                    amount: BigDecimal.fromStringUnsafe('12345678901234567890.123456789'),
                    body: { camelKey: Math.PI, label: '😀' },
                    bytes: Uint8Array.of(0, 0xff, 0),
                    at: DateTime.toDateUtc(DateTime.makeUnsafe('2026-10-17T12:00:00.123Z')),
                    flag: true,
                    ratio: -Number.MAX_VALUE,
                    code: '😀😀',
                    note: Option.some('kept'),
                },
                {
                    id: ids[1],
                    count: int64.minimum,
                    amount: BigDecimal.fromStringUnsafe('-0.000000000000000001'),
                    body: { camelKey: -Number.MIN_VALUE, label: '' },
                    bytes: new Uint8Array(),
                    at: DateTime.toDateUtc(DateTime.makeUnsafe('1969-07-20T20:17:40.500Z')),
                    flag: false,
                    ratio: Number.MIN_VALUE,
                    code: '',
                    note: Option.none(),
                },
            );
            const encoded = Schema.encodeEffect(Written);
            yield* Catalog.reconcile(declared([values]));
            yield* Relation.insert(values, limits);
            assert.deepStrictEqual(yield* Effect.flatMap(Relation.select(values, { count: [int64.minimum, int64.maximum] }, { order: ['id'] }), encoded), yield* encoded(limits));
        }),
    );
    it.effect.prop(
        'returns every generated row unchanged after insert',
        { generated: Generated },
        Effect.fnUntraced(function* ({ generated }) {
            const batch = Array.zipWith(generated, ids, (row, id) => ({ ...row, id }));
            const encoded = Schema.encodeEffect(Written);
            const expected = yield* encoded(batch);
            yield* Catalog.reconcile(declared([values]));
            assert.deepStrictEqual(yield* Effect.all({ inserted: Effect.flatMap(Relation.insert(values, batch), encoded), selected: Effect.flatMap(Relation.select(values, { id: Array.map(batch, Struct.get('id')) }), encoded) }), { inserted: expected, selected: expected });
        }),
        sampled(store, 12),
    );
    it.effect(
        'writes and selects more rows than a statement takes parameters',
        Effect.fnUntraced(function* () {
            const batch = Array.makeBy(volume, (index) => event(`e${index}`, index % 3, index));
            yield* Catalog.reconcile(declared([events]));
            const inserted = yield* Relation.insert(events, batch);
            const selected = yield* Relation.select(events, { id: Array.map(batch, Struct.get('id')) }, { order: ['at'] });
            assert.deepStrictEqual({ inserted: inserted.length, selected }, { inserted: volume, selected: batch });
        }),
    );
    it.effect(
        'upserts new rows as inserted, changed rows as updated keeping named fields, and leaves equal rows unreturned',
        Effect.fnUntraced(function* () {
            const [stays, changes, arrives] = [event('a', 0, 1), event('b', 0, 2), event('c', 1, 3)];
            yield* Catalog.reconcile(declared([events]));
            const first = yield* Relation.upsert(events, [stays, changes]);
            const second = yield* Relation.upsert(events, [stays, { ...changes, kind: 'buy', at: DateTime.add(epoch, { days: 1 }) }, arrives], 'at');
            const repeated = yield* Effect.flip(Relation.upsert(events, [stays, stays]));
            assert.deepStrictEqual(
                { first, second: Array.sortWith(second, ({ row }) => row.id, Order.String), repeated: repeated._tag, stored: yield* Relation.select(events, { tenant: [0, 1] }, { order: ['id'] }) },
                {
                    first: [
                        { row: stays, inserted: true },
                        { row: changes, inserted: true },
                    ],
                    second: [
                        { row: { ...changes, kind: 'buy' }, inserted: false },
                        { row: arrives, inserted: true },
                    ],
                    repeated: 'SchemaError',
                    stored: [stays, { ...changes, kind: 'buy' }, arrives],
                },
            );
        }),
    );
    it.effect(
        'ensures a missing row by inserting it and a stored row by returning it unchanged, once for a repeated key',
        Effect.fnUntraced(function* () {
            const [kept, added] = [event('a', 0, 1), event('b', 0, 2)];
            yield* Catalog.reconcile(declared([events]));
            yield* Relation.insert(events, [kept]);
            const ensured = yield* Relation.ensure(events, [{ ...kept, kind: 'buy' }, added]);
            const repeated = yield* Relation.ensure(events, [event('c', 1, 3), event('c', 1, 4, 'buy')]);
            const settled = yield* Relation.select(events, { id: ['c'] });
            assert.deepStrictEqual({ ensured, repeated, once: settled.length, stored: yield* Relation.select(events, { tenant: [0] }, { order: ['id'] }) }, { ensured: [kept, added], repeated: settled, once: 1, stored: [kept, added] });
        }),
    );
    it.effect(
        'selects by listed, excluded, and null values, ordered, limited, and newest per group',
        Effect.fnUntraced(function* () {
            const batch = Array.make(event('a', 0, 1), event('b', 0, 2), event('c', 0, 3), event('d', 1, 4), event('e', 1, 5, 'buy', 'kept'), event('f', 2, 6));
            yield* Catalog.reconcile(declared([events]));
            yield* Relation.insert(events, batch);
            assert.deepStrictEqual(
                yield* Effect.all({
                    listed: named(Relation.select(events, { tenant: [0, 2], kind: ['view'] })),
                    excluded: named(Relation.select(events, { tenant: { not: [0] } })),
                    absent: named(Relation.select(events, { note: [null] }, { limit: newest })),
                    present: named(Relation.select(events, { note: { not: [null] } })),
                    either: named(Relation.select(events, { note: [null, 'kept'], tenant: [1] })),
                    descending: named(Relation.select(events, { tenant: [0] }, { order: [{ descending: 'at' }] })),
                    newest: named(Relation.select(events, { tenant: [0, 1, 2] }, { order: [{ descending: 'at' }], limit: newest, per: 'tenant' })),
                }),
                { listed: ['a', 'b', 'c', 'f'], excluded: ['d', 'e', 'f'], absent: ['a', 'b'], present: ['e'], either: ['d', 'e'], descending: ['c', 'b', 'a'], newest: ['c', 'b', 'e', 'd', 'f'] },
            );
        }),
    );
    it.effect(
        'removes matching rows and returns them',
        Effect.fnUntraced(function* () {
            const batch = Array.make(event('a', 0, 1), event('b', 1, 2), event('c', 0, 3));
            yield* Catalog.reconcile(declared([events]));
            yield* Relation.insert(events, batch);
            const removed = yield* Relation.remove(events, { tenant: [0] });
            assert.deepStrictEqual({ removed: Array.sortWith(removed, Struct.get('id'), Order.String), stored: yield* Relation.select(events, { tenant: [0, 1] }) }, { removed: [batch[0], batch[2]], stored: [batch[1]] });
        }),
    );
    it.effect(
        'keeps one live account per email ignoring ASCII case and computes columns from JSON and predicates',
        Effect.fnUntraced(function* () {
            yield* Catalog.reconcile(declared([accounts]));
            yield* Relation.insert(accounts, [account(ids[0], 'A@example.com', true), account(ids[1], 'a@example.com', false)]);
            const duplicate = yield* Effect.flip(Relation.insert(accounts, [account(ids[2], 'A@EXAMPLE.COM', false)]));
            assert.deepStrictEqual(
                { fault: duplicate._tag === 'SqlError' ? store.fault(duplicate) : duplicate, stored: (yield* Relation.select(accounts, { plan: ['pro'] }, { order: ['id'] })).map(Struct.pick(['email', 'plan', 'active'])) },
                {
                    fault: store.unique,
                    stored: [
                        { email: 'A@example.com', plan: 'pro', active: false },
                        { email: 'a@example.com', plan: 'pro', active: true },
                    ],
                },
            );
        }),
    );
    it.effect.each(searches)(
        'finds $titles for $text',
        Effect.fnUntraced(function* ({ relation, text, titles }) {
            yield* Catalog.reconcile(declared([notes, fragments]));
            yield* Relation.insert(relation, written);
            assert.deepStrictEqual(
                (yield* Relation.search(relation, text, written.length)).map(({ row }) => row.title),
                titles,
            );
        }),
    );
    it.effect(
        'marks each matched word and fragment in its own column and follows updates',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            yield* Catalog.reconcile(declared([notes, fragments]));
            yield* Relation.insert(notes, written);
            yield* Relation.insert(fragments, written);
            const highlights = yield* Effect.all({
                words: Effect.map(Relation.search(notes, 'cafe CREME', written.length, marks), Array.map(Struct.get('highlight'))),
                fragments: Effect.map(Relation.search(fragments, 'lulam', written.length, marks), Array.map(Struct.get('highlight'))),
            });
            yield* sql`UPDATE notes SET body = 'Steel cables' WHERE id = 1`;
            const followed = yield* Effect.forEach(['vaults', 'steel'], (text) =>
                Effect.map(
                    Relation.search(notes, text, written.length),
                    Array.map(({ row }) => row.title),
                ),
            );
            assert.deepStrictEqual({ highlights, followed }, { highlights: { words: [Option.some({ title: '[Café] [crème]', body: 'Voûte mince' })], fragments: [Option.some({ title: 'Timber frame', body: 'G[lulam] beams' })] }, followed: [[], ['Concrete shell']] });
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Relational, rows, values };
