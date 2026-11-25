import { ClickhouseClient } from '@effect/sql-clickhouse';
import { assert, layer, type Vitest } from '@effect/vitest';
import { Catalog, Column, Relation, Tokenizer } from '@rasm/database';
import { Clickhouse } from '@rasm/database/clickhouse';
import { ClickHouseContainer, type StartedClickHouseContainer } from '@testcontainers/clickhouse';
import { Array, BigDecimal, Context, Effect, Layer, Number, Option, Predicate, Ref, Schema, Struct } from 'effect';
import { SqlClient, SqlError } from 'effect/sql';
import { declared, pending, reconciling, sampled, type Store, stored } from '#/reconciling';

// --- [CONSTANTS] -----------------------------------------------------------------------

const store: Store = {
    tables: SqlClient.SqlClient.use((sql) => Effect.map(sql`SELECT name FROM system.tables WHERE database = currentDatabase() ORDER BY name`, Array.map(Struct.get('name')))),
    fault: (error) => (SqlError.isSqlError(error) && Predicate.hasProperty(error.reason.cause, 'code') ? error.reason.cause.code : error),
    check: '469',
    command: Effect.provideService(ClickhouseClient.ClientMethod, 'command'),
    isolated: 4,
};
const integers = [
    { type: 'UInt8', minimum: 0n, maximum: 255n },
    { type: 'UInt16', minimum: 0n, maximum: 65_535n },
    { type: 'UInt32', minimum: 0n, maximum: 4_294_967_295n },
    { type: 'UInt64', minimum: 0n, maximum: 18_446_744_073_709_551_615n },
    { type: 'UInt128', minimum: 0n, maximum: 340_282_366_920_938_463_463_374_607_431_768_211_455n },
    { type: 'UInt256', minimum: 0n, maximum: 115_792_089_237_316_195_423_570_985_008_687_907_853_269_984_665_640_564_039_457_584_007_913_129_639_935n },
    { type: 'Int8', minimum: -128n, maximum: 127n },
    { type: 'Int16', minimum: -32_768n, maximum: 32_767n },
    { type: 'Int32', minimum: -2_147_483_648n, maximum: 2_147_483_647n },
    { type: 'Int64', minimum: -9_223_372_036_854_775_808n, maximum: 9_223_372_036_854_775_807n },
    { type: 'Int128', minimum: -170_141_183_460_469_231_731_687_303_715_884_105_728n, maximum: 170_141_183_460_469_231_731_687_303_715_884_105_727n },
    { type: 'Int256', minimum: -57_896_044_618_658_097_711_785_492_504_343_953_926_634_992_332_820_282_019_728_792_003_956_564_819_968n, maximum: 57_896_044_618_658_097_711_785_492_504_343_953_926_634_992_332_820_282_019_728_792_003_956_564_819_967n },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const person = { id: Schema.String, label: Schema.String, rank: Schema.Int };
const people = Relation.make('people', person);
const numbered = Relation.make('people', { ...person, label: Schema.Int }, { retype: ['label'] });
const measures = Relation.make('measures', {
    id: Column.Uuid,
    label: Schema.String,
    count: Schema.Int,
    amount: Schema.BigDecimalFromString.check(Schema.isGreaterThanBigDecimal(BigDecimal.fromBigInt(0n))),
    ratio: Schema.Finite,
    flag: Schema.BooleanFromBit,
    at: Schema.DateTimeUtcFromString,
    bytes: Column.Bytes,
    note: Schema.NullOr(Schema.String),
    body: Schema.fromJsonString(Schema.Struct({ plan: Schema.String })),
    tags: Schema.fromJsonString(Schema.Array(Schema.String)),
    weights: Schema.fromJsonString(Schema.Record(Schema.String, Schema.Finite)),
    points: Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, score: Schema.Finite }))),
});
const measured = { id: '01900000-0000-7000-8000-000000000000', label: 'a', count: 1, amount: '12.5', ratio: 0.5, flag: true, at: '2026-01-01 00:00:00.123', bytes: '', note: null, body: '{"plan":"pro"}', tags: ['a', 'b'], weights: { x: 1.5 }, points: [{ name: 'p', score: 2 }] };
const reading = { id: Schema.String, at: Schema.DateTimeUtcFromString, version: Schema.Int, value: Schema.Finite };
const readings = Relation.make('readings', reading, { mergeTree: { expires: { column: 'at', after: '30 days' }, version: 'version', codecs: { value: ['Gorilla', 'ZSTD(3)'] } } });
const relaxed = Relation.make('readings', reading, { mergeTree: { expires: { column: 'at', after: '7 days' }, version: 'version' } });
const visit = { id: Schema.String, tenant: Schema.Int, at: Schema.DateTimeUtcFromString, email: Schema.String, version: Schema.Int };
const versioned = { version: 'version' } as const;
const visits = Relation.make('visits', visit, { mergeTree: versioned });
const sought = Relation.make('visits', visit, {
    mergeTree: versioned,
    indexes: [
        { columns: ['tenant', 'at'], include: ['email'] },
        { columns: ['at'], method: 'brin' },
        { columns: ['email'], method: 'gin' },
    ],
});
const unsought = Relation.make('visits', visit, {
    mergeTree: versioned,
    indexes: [{ columns: ['tenant', 'at'], include: ['email'] }],
    retiredIndexes: [
        ['at', 'brin'],
        ['email', 'gin'],
    ],
});
const linked = Relation.make('linked', { id: Schema.String, personId: Schema.String }, { references: [{ columns: ['personId'], table: people }] });
const single = Relation.make('single', { id: Schema.String, email: Schema.String }, { indexes: [{ columns: ['email'], unique: true }] });
const partial = Relation.make('partial', { id: Schema.String, email: Schema.NullOr(Schema.String) }, { indexes: [{ columns: ['id'], where: { email: null } }] });
const nearby = Relation.make('nearby', { id: Schema.String, email: Schema.String }, { indexes: [{ columns: ['email'], method: 'gist' }] });
const worded = Relation.make('worded', { id: Schema.String, title: Schema.String }, { search: { columns: ['title'], tokenizer: Tokenizer.Trigram() } });
const rented = Relation.make('rented', { id: Schema.String, tenant: Schema.String }, { tenant: { column: 'tenant', setting: 'app.tenant' } });
const listed = Relation.make('listed', { id: Schema.String, tags: Schema.NullOr(Schema.fromJsonString(Schema.Array(Schema.String))) });
const aged = Relation.make('aged', { id: Schema.String, at: Schema.String }, { mergeTree: { expires: { column: 'at', after: '1 day' } } });
const revised = Relation.make('revised', { id: Schema.String, version: Schema.String }, { mergeTree: { version: 'version' } });
const refusals: readonly { readonly label: string; readonly relation: Catalog['tables'][number]; readonly finding: object }[] = [
    { label: 'a reference', relation: linked, finding: { table: 'linked', columns: ['person_id'], reason: { _tag: 'Unsupported', refusal: 'reference' } } },
    { label: 'a unique index', relation: single, finding: { table: 'single', columns: ['email'], reason: { _tag: 'Unsupported', refusal: 'unique' } } },
    { label: 'a partial index', relation: partial, finding: { table: 'partial', columns: ['id'], reason: { _tag: 'Unsupported', refusal: 'unique' } } },
    { label: 'a search tree index', relation: nearby, finding: { table: 'nearby', columns: ['email'], reason: { _tag: 'Unsupported', refusal: 'unique' } } },
    { label: 'declared text search', relation: worded, finding: { table: 'worded', columns: ['title'], reason: { _tag: 'Unsupported', refusal: 'fulltext' } } },
    { label: 'a tenant scope', relation: rented, finding: { table: 'rented', columns: ['tenant'], reason: { _tag: 'Unsupported', refusal: 'tenant' } } },
    { label: 'a nullable collection', relation: listed, finding: { table: 'listed', columns: ['tags'], reason: { _tag: 'Unsupported', refusal: 'collection' } } },
    { label: 'an expiry over a column that is no instant', relation: aged, finding: { table: 'aged', columns: ['at'], reason: { _tag: 'Unsupported', refusal: 'mergeTreeColumn' } } },
    { label: 'a version column that is neither integer nor instant', relation: revised, finding: { table: 'revised', columns: ['version'], reason: { _tag: 'Unsupported', refusal: 'mergeTreeColumn' } } },
];
const limits = Array.dedupe(integers.flatMap(({ minimum, maximum }) => [minimum - 1n, minimum, minimum + 1n, maximum - 1n, maximum, maximum + 1n])).filter((limit) => limit >= integers[11].minimum && limit <= integers[5].maximum);
const narrowest = (minimum: bigint, maximum: bigint) => Array.findFirst(integers, (range) => range.minimum <= minimum && range.maximum >= maximum && (minimum >= 0n || range.minimum < 0n));
const Bounds = Schema.Struct({ minimum: Schema.Literals(limits), maximum: Schema.Literals(limits) }).check(Schema.makeFilter(({ minimum, maximum }) => minimum <= maximum && Option.isSome(narrowest(minimum, maximum)), { expected: 'ordered bounds one ClickHouse integer type holds' }));

// --- [SERVICES] ------------------------------------------------------------------------

class Server extends Context.Service<Server, { readonly container: StartedClickHouseContainer; readonly admin: SqlClient.SqlClient; readonly created: Ref.Ref<number> }>()('@rasm/test-database/clickhouse.test/Server') {
    static readonly layer = Effect.gen(function* () {
        const container = yield* Effect.acquireDisposable(Effect.promise(() => new ClickHouseContainer('clickhouse/clickhouse-server:26.10.10.3-alpine').start()));
        const admin = Context.get(yield* Layer.build(ClickhouseClient.layer(container.getClientOptions())), SqlClient.SqlClient);
        return { container, admin, created: yield* Ref.make(0) };
    }).pipe(Layer.effect(Server));
    static readonly database = Layer.unwrap(
        Effect.gen(function* () {
            const { container, admin, created } = yield* Server;
            const name = `test_${yield* Ref.updateAndGet(created, Number.increment)}`;
            yield* Effect.acquireRelease(store.command(admin`CREATE DATABASE ${admin(name)}`), () => Effect.orDie(store.command(admin`DROP DATABASE ${admin(name)} SYNC`)));
            return Clickhouse.layer({ ...container.getClientOptions(), database: name });
        }),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const described = (table: string) =>
    SqlClient.SqlClient.use((sql) =>
        Effect.all({
            table: sql`SELECT engine, partition_key AS partition, primary_key AS key, sorting_key AS sorting FROM system.tables WHERE database = currentDatabase() AND name = ${table}`,
            codecs: sql`SELECT name, compression_codec AS codec FROM system.columns WHERE database = currentDatabase() AND table = ${table} AND compression_codec != '' ORDER BY position`,
            skipping: sql`SELECT type_full AS type, expr AS expression FROM system.data_skipping_indices WHERE database = currentDatabase() AND table = ${table} ORDER BY expr`,
            projections: sql`SELECT query FROM system.projections WHERE database = currentDatabase() AND table = ${table} ORDER BY name`,
        }),
    );
const surviving = Effect.fnUntraced(function* (ages: readonly number[]) {
    const sql = yield* SqlClient.SqlClient;
    yield* Effect.forEach(ages, (age) => store.command(sql`INSERT INTO readings SELECT ${`${age} days`} AS id, now64(3) - toIntervalDay(${sql.literal(`${age}`)}) AS at, 1 AS version, 1.5 AS value`), { discard: true });
    yield* store.command(sql`OPTIMIZE TABLE readings FINAL`);
    return Array.map(yield* sql`SELECT DISTINCT id FROM readings ORDER BY id`, Struct.get('id'));
});

const cases = (it: Vitest.MethodsNonLive<Layer.Success<typeof Server.database> | Server>) => {
    reconciling(it, store);
    it.effect(
        'stores each schema kind in the ClickHouse type that holds every value of it, collections included',
        Effect.fnUntraced(function* () {
            const [sql, client] = yield* Effect.all([SqlClient.SqlClient, ClickhouseClient.ClickhouseClient]);
            yield* Catalog.reconcile(declared([measures]));
            yield* client.insertQuery({ table: 'measures', values: [measured] });
            assert.deepStrictEqual(
                yield* Effect.all({
                    types: sql`SELECT name, type FROM system.columns WHERE database = currentDatabase() AND table = 'measures' ORDER BY position`,
                    stored: stored('measures'),
                }),
                {
                    types: [
                        { name: 'id', type: 'UUID' },
                        { name: 'label', type: 'String' },
                        { name: 'count', type: 'Int64' },
                        { name: 'amount', type: 'String' },
                        { name: 'ratio', type: 'Float64' },
                        { name: 'flag', type: 'Bool' },
                        { name: 'at', type: 'DateTime64(3)' },
                        { name: 'bytes', type: 'String' },
                        { name: 'note', type: 'Nullable(String)' },
                        { name: 'body', type: 'String' },
                        { name: 'tags', type: 'Array(String)' },
                        { name: 'weights', type: 'Map(String, Float64)' },
                        { name: 'points', type: 'Array(Tuple(name String, score Float64))' },
                    ],
                    stored: [measured],
                },
            );
        }),
    );
    it.effect.prop(
        'stores integers under any bounds in the narrowest ClickHouse integer type whose range holds both, refusing the next value past a bound that type can hold',
        { bounds: Bounds },
        Effect.fnUntraced(function* ({ bounds: { minimum, maximum } }) {
            const [sql, range] = yield* Effect.all([SqlClient.SqlClient, Effect.fromOption(narrowest(minimum, maximum))]);
            const bounded = Relation.make('bounded', { id: Schema.String, value: Schema.BigIntFromString.check(Schema.isBetweenBigInt({ minimum, maximum })) });
            const written = (id: string, value: bigint) => store.command(sql`INSERT INTO bounded ${sql.insert({ id, value: value.toString() })}`);
            const past = [minimum - 1n, maximum + 1n].filter((value) => value >= range.minimum && value <= range.maximum);
            yield* Catalog.reconcile(declared([bounded]));
            yield* Effect.all([written('low', minimum), written('high', maximum)]);
            const refused = yield* Effect.forEach(past, (value) => Effect.map(Effect.flip(written('past', value)), store.fault));
            assert.deepStrictEqual(
                { stored: yield* sql`SELECT toTypeName(value) AS type, toString(min(value)) AS low, toString(max(value)) AS high, count() AS rows FROM bounded GROUP BY type`, refused },
                { stored: [{ type: range.type, low: minimum.toString(), high: maximum.toString(), rows: 2 }], refused: past.map(() => store.check) },
            );
        }),
        sampled(store, 24),
    );
    it.effect(
        'builds a table as MergeTree keyed and sorted by its key, and versions, expiry, partitions, and codecs as declared',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            yield* Catalog.reconcile(declared([people, readings]));
            yield* Effect.forEach(
                [
                    { id: 'a', at: '2099-01-01 00:00:00.000', version: 2, value: 2.5 },
                    { id: 'a', at: '2099-01-01 00:00:00.000', version: 1, value: 1.5 },
                ],
                (inserted) => store.command(sql`INSERT INTO readings ${sql.insert(inserted)}`),
                { discard: true },
            );
            assert.deepStrictEqual(yield* Effect.all({ people: Effect.map(described('people'), Struct.pick(['table', 'codecs'])), readings: Effect.map(described('readings'), Struct.pick(['table', 'codecs'])), newest: sql`SELECT value FROM readings FINAL`, surviving: surviving([29, 31]) }), {
                people: { table: [{ engine: 'MergeTree', partition: '', key: '(id)', sorting: '(id)' }], codecs: [] },
                readings: { table: [{ engine: 'ReplacingMergeTree', partition: 'toDate(at)', key: '(id)', sorting: '(id)' }], codecs: [{ name: 'value', codec: 'CODEC(Gorilla(8), ZSTD(3))' }] },
                newest: [{ value: 2.5 }],
                surviving: ['29 days', 'a'],
            });
        }),
    );
    it.effect(
        'changes an expiry and removes a codec in place, keeping rows',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            const row = { id: 'a', at: '2099-01-01 00:00:00.000', version: 1, value: 1.5 };
            yield* Catalog.reconcile(declared([readings]));
            yield* store.command(sql`INSERT INTO readings ${sql.insert(row)}`);
            const before = yield* surviving([10]);
            yield* Catalog.reconcile(declared([relaxed]));
            const changed = yield* described('readings');
            assert.deepStrictEqual(
                { before, codecs: changed.codecs, after: yield* surviving([5]), stored: yield* sql`SELECT * FROM readings WHERE id = 'a'`, pending: yield* pending(declared([relaxed])) },
                { before: ['10 days', 'a'], codecs: [], after: ['5 days', 'a'], stored: [row], pending: { statements: [], concurrent: [] } },
            );
        }),
    );
    it.effect(
        'adds an ordered projection and range and membership skip indexes to a stored table, and drops retired ones',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            const row = { id: 'a', tenant: 1, at: '2026-01-01 00:00:00.000', email: 'a@example.com', version: 1 };
            yield* Catalog.reconcile(declared([visits]));
            yield* store.command(sql`INSERT INTO visits ${sql.insert(row)}`);
            yield* Catalog.reconcile(declared([sought]));
            const added = yield* Effect.map(described('visits'), Struct.pick(['skipping', 'projections']));
            yield* Catalog.reconcile(declared([unsought]));
            assert.deepStrictEqual(
                { added, retired: yield* Effect.map(described('visits'), Struct.pick(['skipping', 'projections'])), stored: yield* stored('visits'), pending: yield* pending(declared([unsought])) },
                {
                    added: {
                        skipping: [
                            { type: 'minmax', expression: '(at)' },
                            { type: 'bloom_filter', expression: '(email)' },
                        ],
                        projections: [{ query: 'SELECT _part_offset, email ORDER BY tenant, at' }],
                    },
                    retired: { skipping: [], projections: [{ query: 'SELECT _part_offset, email ORDER BY tenant, at' }] },
                    stored: [row],
                    pending: { statements: [], concurrent: [] },
                },
            );
        }),
    );
    it.effect.each(refusals)(
        'refuses $label',
        Effect.fnUntraced(function* ({ relation, finding }) {
            const refused = yield* Effect.flip(Catalog.reconcile(declared([people, relation])));
            assert.deepStrictEqual({ refused: refused._tag === 'ReconcileError' ? refused.findings : refused, tables: yield* store.tables }, { refused: [finding], tables: [] });
        }),
    );
    it.effect(
        'keeps a table and its rows when a rebuild fails, and holds the declared table alone after the next reconcile',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            const row = { id: 'p', label: 'Parent', rank: 1 };
            yield* Catalog.reconcile(declared([people]));
            yield* store.command(sql`INSERT INTO people ${sql.insert(row)}`);
            const failed = yield* Effect.flip(Catalog.reconcile(declared([numbered])));
            const left = yield* stored('people');
            yield* Catalog.reconcile(declared([people]));
            assert.deepStrictEqual({ failed: store.fault(failed), left, settled: yield* Effect.all({ tables: store.tables, stored: stored('people') }) }, { failed: '6', left: [row], settled: { tables: ['people'], stored: [row] } });
        }),
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Server.layer, { excludeTestServices: true, concurrent: true })('clickhouse', (outer) => outer.layer(Server.database, { buildPerTest: true })(cases));
