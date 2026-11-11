import { ClickhouseClient } from '@effect/sql-clickhouse';
import { assert, expectTypeOf, it } from '@effect/vitest';
import { Catalog, Engine, Relation, Tokenizer } from '@rasm/database';
import { Clickhouse } from '@rasm/database/clickhouse';
import { Store } from '@rasm/test-store';
import { failed } from '@rasm/testing/vitest';
import { Effect, Layer, Schema, Struct } from 'effect';
import type { SqlClient } from 'effect/sql';

// --- [MODELS] --------------------------------------------------------------------------

const fields = {
    Timestamp: Schema.DateTimeUtcFromString,
    TraceId: Schema.String,
    ServiceName: Schema.String,
    Duration: Schema.Natural,
    StatusCode: Schema.Literals(['Unset', 'Ok', 'Error']),
    SpanAttributes: Schema.fromJsonString(Schema.Record(Schema.String, Schema.String)),
    Events: Schema.fromJsonString(Schema.Array(Schema.Struct({ Timestamp: Schema.DateTimeUtcFromString, Name: Schema.String }))),
};
const retention = (after: '90 days' | '30 days') => ({ expires: { column: 'Timestamp', after }, codecs: { Timestamp: ['Delta', 'ZSTD(1)'] } }) as const;
const indexed = {
    key: ['ServiceName', 'Timestamp'],
    indexes: [
        { columns: ['TraceId'], include: ['Duration'] },
        { columns: ['Duration'], method: 'brin' },
        { columns: ['SpanAttributes'], method: 'gin' },
    ],
    mergeTree: retention('90 days'),
} as const;
const spans = Relation.make('spans', fields, indexed);
const traces = Relation.make(
    'traces',
    { Timestamp: fields.Timestamp, Trace: fields.TraceId, ServiceName: fields.ServiceName, Duration: fields.Duration, StatusCode: fields.StatusCode, Events: fields.Events, Note: Schema.NullOr(Schema.String) },
    { key: ['ServiceName', 'Timestamp'], renamedFrom: 'spans', renamed: { Trace: 'TraceId' }, retired: ['SpanAttributes'], retiredIndexes: [['Duration', 'brin']], indexes: [{ columns: ['Trace'] }], mergeTree: retention('30 days') },
);
const trimmed = Relation.make('spans', Struct.omit(fields, ['Duration']), {
    key: ['ServiceName', 'Timestamp'],
    retired: ['Duration'],
    indexes: [
        { columns: ['TraceId'], include: ['StatusCode'] },
        { columns: ['SpanAttributes'], method: 'gin' },
    ],
    mergeTree: { expires: { column: 'Timestamp', after: '90 days' }, codecs: { Timestamp: ['ZSTD(1)'] } },
});
const resorted = Relation.make('spans', { ...fields, Duration: Schema.Finite }, { key: ['TraceId', 'Timestamp'], retype: ['Duration'], mergeTree: retention('90 days') });
const revisions = Relation.make(
    'revisions',
    { id: Schema.String, Owner: Schema.String.check(Schema.isLowercased()), Revision: Schema.Natural, Score: Schema.NullOr(Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 }))), Level: Schema.Int.check(Schema.isBetween({ minimum: -5, maximum: 5 })) },
    { indexes: [{ columns: [{ casefold: 'Owner' }] }], defaults: { Level: 0 }, mergeTree: { version: 'Revision' } },
);
const accounts = Relation.make('accounts', { id: Schema.String, tenant: Schema.String, tags: Schema.NullOr(Schema.fromJsonString(Schema.Array(Schema.String))) });
const refused = Relation.make(
    'ledgers',
    { id: Schema.String, account: Schema.String, tenant: Schema.String, note: Schema.String },
    { references: [{ columns: ['account'], table: accounts }], indexes: [{ columns: ['note'], unique: true }], tenant: { column: 'tenant', setting: 'app.tenant' }, search: { columns: ['note'], tokenizer: Tokenizer.Trigram() } },
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const engine = Layer.unwrap(
    Effect.map(
        Store.use(({ clickhouse }) => clickhouse),
        (config) => Clickhouse.layer(config, Engine.names.verbatim),
    ),
);

it.layer(Store.layer(undefined, failed), { concurrent: false, timeout: '2 minutes' })('clickhouse', (methods) => {
    const isolated =
        <A, E>(body: (sql: ClickhouseClient.ClickhouseClient) => Effect.Effect<A, E, Layer.Success<typeof engine>>) =>
        () =>
            Effect.provide(Effect.flatMap(ClickhouseClient.ClickhouseClient, body), engine);
    const pending = (catalog: Catalog) => Effect.map(Catalog.plan(catalog), Struct.get('statements'));
    const seeded = (sql: ClickhouseClient.ClickhouseClient) =>
        sql.asCommand(sql`INSERT INTO spans (${sql('Timestamp')}, ${sql('TraceId')}, ${sql('ServiceName')}, ${sql('Duration')}, ${sql('StatusCode')}, ${sql('SpanAttributes')}, ${sql('Events')})
            VALUES (now64(3), 'a', 'api', 7, 'Ok', map('route', '/'), [(now64(3), 'start')]), (now64(3), 'b', 'api', 9, 'Error', map('route', '/health'), [])`);
    const layout = (sql: ClickhouseClient.ClickhouseClient, table: string) => sql`SELECT engine, partition_key, sorting_key, engine_full LIKE '%toIntervalSecond(%' AS expiring,
        (SELECT groupArray(name) FROM (SELECT name FROM system.data_skipping_indices WHERE database = currentDatabase() AND table = ${table} ORDER BY name)) AS indexes,
        (SELECT groupArray(name) FROM (SELECT name FROM system.projections WHERE database = currentDatabase() AND table = ${table} ORDER BY name)) AS projections,
        (SELECT groupArray(name) FROM (SELECT name FROM system.columns WHERE database = currentDatabase() AND table = ${table} ORDER BY position)) AS columns
        FROM system.tables WHERE database = currentDatabase() AND name = ${table}`;
    methods.effect(
        'creates MergeTree tables with the declared partition, retention, codec, projection, and skip indexes, then plans nothing',
        isolated(
            Effect.fnUntraced(function* (sql) {
                const catalog = Catalog.make({ tables: [spans] });
                assert.deepStrictEqual(yield* Catalog.reconcile(catalog), { tables: [], columns: [], indexes: [], references: [] });
                assert.deepStrictEqual(yield* layout(sql, 'spans'), [
                    {
                        engine: 'MergeTree',
                        partition_key: 'toDate(Timestamp)',
                        sorting_key: 'ServiceName, Timestamp',
                        expiring: 1,
                        indexes: ['Duration_brin', 'SpanAttributes_gin'],
                        projections: ['TraceId'],
                        columns: Struct.keys(fields),
                    },
                ]);
                assert.deepStrictEqual(yield* sql`SELECT type, compression_codec FROM system.columns WHERE database = currentDatabase() AND table = 'spans' AND name IN ('Timestamp', 'Duration', 'SpanAttributes', 'Events') ORDER BY position`, [
                    { type: 'DateTime64(3)', compression_codec: 'CODEC(Delta(8), ZSTD(1))' },
                    { type: 'UInt64', compression_codec: '' },
                    { type: 'Map(String, String)', compression_codec: '' },
                    { type: 'Array(Tuple(Timestamp DateTime64(3), Name String))', compression_codec: '' },
                ]);
                assert.deepStrictEqual(yield* pending(catalog), []);
            }),
        ),
    );
    methods.effect(
        'replaces rows by version, passes NULL through checks, fills defaults, and refuses a violating row',
        isolated(
            Effect.fnUntraced(function* (sql) {
                const catalog = Catalog.make({ tables: [revisions] });
                yield* Catalog.reconcile(catalog);
                yield* sql.asCommand(sql`INSERT INTO revisions (id, ${sql('Owner')}, ${sql('Revision')}, ${sql('Score')}) VALUES ('a', 'ann', 1, NULL)`);
                assert.strictEqual((yield* Effect.flip(sql.asCommand(sql`INSERT INTO revisions (id, ${sql('Owner')}, ${sql('Revision')}, ${sql('Score')}) VALUES ('b', 'Bob', 1, 0.5)`)))._tag, 'SqlError');
                assert.deepStrictEqual(yield* Effect.all([sql`SELECT engine FROM system.tables WHERE database = currentDatabase() AND name = 'revisions'`, sql`SELECT id, ${sql('Level')}, ${sql('Score')}, toTypeName(${sql('Level')}) AS type FROM revisions`, pending(catalog)]), [
                    [{ engine: 'ReplacingMergeTree' }],
                    [{ id: 'a', Level: 0, Score: null, type: 'Int8' }],
                    [],
                ]);
            }),
        ),
    );
    methods.effect(
        'renames a table and column, adds and drops columns and indexes, and changes retention in place keeping rows',
        isolated(
            Effect.fnUntraced(function* (sql) {
                yield* Catalog.reconcile(Catalog.make({ tables: [spans] }));
                yield* seeded(sql);
                const catalog = Catalog.make({ tables: [traces] });
                assert.deepStrictEqual(yield* Catalog.reconcile(catalog), { tables: [], columns: [], indexes: [], references: [] });
                assert.deepStrictEqual(yield* layout(sql, 'traces'), [{ engine: 'MergeTree', partition_key: 'toDate(Timestamp)', sorting_key: 'ServiceName, Timestamp', expiring: 1, indexes: [], projections: ['Trace'], columns: Struct.keys(traces.schema.fields) }]);
                assert.deepStrictEqual(yield* sql`SELECT ${sql('Trace')}, ${sql('Duration')}, ${sql('Note')} FROM traces ORDER BY ${sql('Trace')}`, [
                    { Trace: 'a', Duration: 7, Note: null },
                    { Trace: 'b', Duration: 9, Note: null },
                ]);
                assert.deepStrictEqual(yield* pending(catalog), []);
            }),
        ),
    );
    methods.effect(
        'replaces a projection whose covered columns change, drops the parts of a dropped column, and changes a codec in place',
        isolated(
            Effect.fnUntraced(function* (sql) {
                yield* Catalog.reconcile(Catalog.make({ tables: [spans] }));
                yield* seeded(sql);
                const catalog = Catalog.make({ tables: [trimmed] });
                assert.deepStrictEqual(yield* Catalog.reconcile(catalog), { tables: [], columns: [], indexes: [], references: [] });
                assert.deepStrictEqual(yield* layout(sql, 'spans'), [{ engine: 'MergeTree', partition_key: 'toDate(Timestamp)', sorting_key: 'ServiceName, Timestamp', expiring: 1, indexes: ['SpanAttributes_gin'], projections: ['TraceId'], columns: Struct.keys(trimmed.schema.fields) }]);
                assert.deepStrictEqual(
                    yield* sql`SELECT (SELECT query FROM system.projections WHERE database = currentDatabase() AND table = 'spans') AS projection, (SELECT compression_codec FROM system.columns WHERE database = currentDatabase() AND table = 'spans' AND name = 'Timestamp') AS codec, (SELECT count() FROM spans) AS kept`,
                    [{ projection: 'SELECT _part_offset, StatusCode ORDER BY TraceId', codec: 'CODEC(ZSTD(1))', kept: 2 }],
                );
                assert.deepStrictEqual(yield* pending(catalog), []);
            }),
        ),
    );
    methods.effect(
        'rebuilds a table for a new sorting key and a retyped column keeping rows, and adopts an unstamped table',
        isolated(
            Effect.fnUntraced(function* (sql) {
                yield* sql.asCommand(
                    sql`CREATE TABLE spans (${sql('Timestamp')} DateTime64(3), ${sql('TraceId')} String, ${sql('ServiceName')} String, ${sql('Duration')} UInt64, ${sql('StatusCode')} String, ${sql('SpanAttributes')} Map(String, String), ${sql('Events')} Array(Tuple(Timestamp DateTime64(3), Name String)), ${sql('Extra')} String DEFAULT 'kept') ENGINE = MergeTree ORDER BY ${sql('Timestamp')}`,
                );
                yield* seeded(sql);
                assert.deepStrictEqual(yield* Catalog.reconcile(Catalog.make({ tables: [spans] })), { tables: [], columns: [{ table: 'spans', column: 'Extra' }], indexes: [], references: [] });
                const catalog = Catalog.make({ tables: [resorted] });
                yield* Catalog.reconcile(catalog);
                assert.deepStrictEqual(yield* sql`SELECT sorting_key, (SELECT groupArray(name) FROM (SELECT name FROM system.tables WHERE database = currentDatabase() ORDER BY name)) AS tables FROM system.tables WHERE database = currentDatabase() AND name = 'spans'`, [
                    { sorting_key: 'TraceId, Timestamp', tables: ['spans'] },
                ]);
                assert.deepStrictEqual(yield* sql`SELECT ${sql('TraceId')}, ${sql('Duration')}, ${sql('Extra')}, toTypeName(${sql('Duration')}) AS type FROM spans ORDER BY ${sql('TraceId')}`, [
                    { TraceId: 'a', Duration: 7, Extra: 'kept', type: 'Float64' },
                    { TraceId: 'b', Duration: 9, Extra: 'kept', type: 'Float64' },
                ]);
                assert.deepStrictEqual(yield* pending(catalog), []);
            }),
        ),
    );
    methods.effect(
        'refuses references, unique indexes, tenant scope, search, and nullable collections',
        isolated(() =>
            Catalog.reconcile(Catalog.make({ tables: [accounts, refused] })).pipe(
                Effect.as([]),
                Effect.catchTag('ReconcileError', ({ findings }) => Effect.succeed(findings)),
                Effect.map((findings) =>
                    assert.deepStrictEqual(findings, [
                        { table: 'accounts', columns: ['tags'], reason: { _tag: 'Unsupported', refusal: 'collection' } },
                        { table: 'ledgers', columns: ['account'], reason: { _tag: 'Unsupported', refusal: 'reference' } },
                        { table: 'ledgers', columns: ['note'], reason: { _tag: 'Unsupported', refusal: 'unique' } },
                        { table: 'ledgers', columns: ['tenant'], reason: { _tag: 'Unsupported', refusal: 'tenant' } },
                        { table: 'ledgers', columns: ['note'], reason: { _tag: 'Unsupported', refusal: 'fulltext' } },
                    ]),
                ),
            ),
        ),
    );
    methods.effect(
        'refuses a declared column that another writer added with no recorded definition',
        isolated(
            Effect.fnUntraced(function* (sql) {
                yield* Catalog.reconcile(Catalog.make({ tables: [spans] }));
                yield* sql.asCommand(sql`ALTER TABLE spans ADD COLUMN ${sql('Note')} String`);
                const noted = Relation.make('spans', { ...fields, Note: Schema.NullOr(Schema.Finite) }, indexed);
                assert.deepStrictEqual(yield* Effect.flip(Catalog.reconcile(Catalog.make({ tables: [noted] }))).pipe(Effect.map(Struct.get('message'))), 'spans (Note) stored definition differs, declare retype');
            }),
        ),
    );
    methods.effect('provides reconcile without the row statement service', () => Effect.sync(() => expectTypeOf<Layer.Success<ReturnType<typeof Clickhouse.layer>>>().toEqualTypeOf<Engine | ClickhouseClient.ClickhouseClient | SqlClient.SqlClient>()));
});
