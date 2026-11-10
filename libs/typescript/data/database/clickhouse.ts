import { ClickhouseClient } from '@effect/sql-clickhouse';
import { Array, Config, Duration, Effect, Equal, Filter, Formatter, flow, HashSet, Layer, Match, Option, Order, Record, Result, Schema, SchemaGetter, String, Struct, UndefinedOr } from 'effect';
import { SqlClient, SqlSchema, Statement } from 'effect/sql';
import type { Catalog } from '#/catalog';
import { Column, type Kind } from '#/column';
import { type Dialect, Engine } from '#/engine';
import { constant, diff, digest, Identity, identifier, literal, render, replacement, Stamp, unmigrated, unsupported } from '#/reconcile';
import type { Relation } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

type Aligned = ReturnType<typeof aligned>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const types = { text: 'String', uuid: 'UUID', json: 'String', instant: 'DateTime64(3)', integer: 'Int64', decimal: 'String', real: 'Float64', boolean: 'Bool', blob: 'String' } as const satisfies { readonly [K in Kind]: string };
const skips: { readonly [M in Relation['indexes'][number]['method']]?: string } = { brin: 'minmax', gin: 'bloom_filter' };
const kinds = {
    INDEX: { source: 'data_skipping_indices', stored: true, definition: "concat(expr, ' TYPE ', type_full, ' GRANULARITY ', toString(granularity))" },
    PROJECTION: { source: 'projections', stored: true, definition: "concat('(', query, ')')" },
    CONSTRAINT: { source: 'constraints', stored: false, definition: "concat(toString(type), ' ', expression)" },
} as const;
const bits = { narrowest: 8n, unbounded: 64n, widest: 256n };
const edges = { '>': { lower: true, round: Math.floor, step: 1n }, '>=': { lower: true, round: Math.ceil, step: 0n }, '<': { lower: false, round: Math.ceil, step: -1n }, '<=': { lower: false, round: Math.floor, step: 0n } };
const escape = Statement.defaultEscape('"');
const quoted = flow(literal, String.replaceAll('\\', '\\\\'));

// --- [MODELS] --------------------------------------------------------------------------

const Part = Schema.Struct({ kind: Schema.Literals(Struct.keys(kinds)), name: Schema.String, digest: Schema.String, columns: Schema.Array(Schema.String) });
const Layout = Schema.Struct({ key: Schema.Array(Schema.String), version: Schema.optionalKey(Schema.String), partition: Schema.optionalKey(Schema.String) });
const Mark = Schema.fromJsonString(Identity.mapFields(Struct.assign({ layout: Layout, expires: Schema.optionalKey(Schema.Int), codecs: Schema.Record(Schema.String, Schema.NonEmptyArray(Schema.String)), parts: Schema.Array(Part) })));
const recorded = <S extends Schema.Codec<unknown, string>>(stamp: S) => Schema.String.pipe(Schema.decodeTo(Schema.Option(Schema.toType(stamp)), { decode: SchemaGetter.transform(Schema.decodeOption(stamp)), encode: SchemaGetter.forbiddenEncoding }));
const Table = Schema.Struct({
    name: Schema.String,
    identity: recorded(Mark),
    columns: Schema.Array(Schema.Struct({ name: Schema.String, identity: recorded(Stamp), comment: Schema.String, definition: Schema.String, codec: Schema.String, copied: Schema.BooleanFromBit })),
    parts: Schema.Array(Schema.Struct({ kind: Part.fields.kind, name: Schema.String, definition: Schema.String })),
});
class Clickhouse extends Schema.Class<Clickhouse, { readonly brand: unique symbol }>('Clickhouse')({ tables: Schema.Array(Table), scratch: Schema.Array(Schema.String) }) {
    // --- [DDL]

    static readonly #stored: (collection: Option.Option.Value<Column['collection']>) => string = Column.Collection.$match({
        Scalar: ({ kind }) => types[kind],
        Array: ({ element }) => `Array(${Clickhouse.#stored(element)})`,
        Map: ({ value }) => `Map(String, ${Clickhouse.#stored(value)})`,
        Records: ({ members }) => `Array(Tuple(${Record.collect(members, (name, member) => [escape(name), Clickhouse.#stored(member)].join(' ')).join(', ')}))`,
    });
    static readonly #integer = (bounds: Column['bounds']) => {
        const [lower, upper] = Array.partition(
            Array.filterMap(bounds.filter(Column.Bound.$is('Compare')), ({ op, value, subject }) => (subject === 'value' && !(value instanceof Date) ? Result.succeed({ lower: edges[op].lower, value: (typeof value === 'bigint' ? value : BigInt(edges[op].round(value))) + edges[op].step }) : Result.failVoid)),
            Filter.fromPredicate(({ lower: side }) => side),
        );
        const low = Array.match(lower.map(Struct.get('value')), { onEmpty: () => -(2n ** (bits.unbounded - 1n)), onNonEmpty: Array.max(Order.BigInt) });
        const signed = low < 0n ? 1n : 0n;
        const high = Array.match(upper.map(Struct.get('value')), { onEmpty: () => 2n ** (bits.unbounded - signed) - 1n, onNonEmpty: Array.min(Order.BigInt) });
        const widths = Array.unfold<bigint, bigint>(bits.narrowest, (size) => (size > bits.widest ? Option.none() : Option.some([size, size * 2n])));
        const width = Array.findFirst(widths, (candidate) => low >= -signed * 2n ** (candidate - signed) && high < 2n ** (candidate - signed));
        return `${signed === 0n ? 'UInt' : 'Int'}${Option.getOrElse(width, () => bits.widest)}`;
    };
    static readonly type: Dialect['type'] = (entry) => {
        const held = Match.value(entry).pipe(
            Match.when({ collection: Option.isSome }, ({ collection }) => Clickhouse.#stored(collection.value)),
            Match.when({ kind: 'integer' }, ({ bounds }) => Clickhouse.#integer(bounds)),
            Match.orElse(({ kind }) => types[kind]),
        );
        return entry.nullable ? `Nullable(${held})` : held;
    };
    static readonly constant: Dialect['constant'] = flow(constant, String.replaceAll('\\', '\\\\'));
    static readonly codePoints: Dialect['codePoints'] = (sql, subject) => sql`lengthUTF8(${subject})`;
    static readonly cased: Dialect['cased'] = (sql, subject, _, casing) => sql`${sql.literal(casing)}UTF8(toString(${subject})) = toString(${subject})`;
    static readonly domain: Dialect['domain'] = (sql, subject, kind) => (kind === 'real' ? [sql`isFinite(${subject})`] : []);
    static readonly projection: Dialect['projection'] = (sql, entry, { from, path }) => {
        const [extract, cast] = entry.kind === 'json' ? ['JSONExtractRaw', []] : ['JSONExtract', [Clickhouse.type(entry)]];
        return sql`${sql.literal(extract)}(${sql.csv([Statement.fragment([sql(from)]), ...[...path, ...cast].map(quoted)])})`;
    };
    static readonly generated: Dialect['generated'] = (sql, expression) => sql`ALIAS (${expression})`;
    static readonly constraint: Dialect['constraint'] = (sql) => sql.literal('');
    static readonly casefold: Dialect['casefold'] = (sql, name) => sql`lowerUTF8(${sql(name)})`;
    static readonly methods: Dialect['methods'] = ['btree', ...Struct.keys(skips)];
    static readonly opclass: Dialect['opclass'] = (method) => Option.liftPredicate(Option.none(), () => Clickhouse.methods.includes(method));
    static readonly projected: Dialect['projected'] = (sql, _, name) => sql`${sql(name)}`;
    static readonly covering: Dialect['covering'] = () => ({ keys: [], clause: [] });
    static readonly suffix: Dialect['suffix'] = (sql, { mergeTree: { version, expires } }) => {
        const settings = [...Option.toArray(version).map(() => "deduplicate_merge_projection_mode = 'rebuild'"), ...Option.toArray(expires).map(() => 'ttl_only_drop_parts = 1')];
        return sql.join(
            ' ',
            false,
        )([
            ' ENGINE =',
            Option.match(version, { onNone: () => sql.literal('MergeTree'), onSome: (column) => sql`ReplacingMergeTree(${sql(column)})` }),
            ...Option.toArray(expires).flatMap((expiry) => [sql`PARTITION BY toDate(${sql(expiry.column)})`, ttl(sql, expiry)]),
            ...Array.match(settings, { onEmpty: () => [], onNonEmpty: (all) => ['SETTINGS', all.join(', ')] }),
        ]);
    };
    static readonly rename: Dialect['rename'] = (sql, from, to) => sql`RENAME TABLE ${sql(from)} TO ${to}`;
    static readonly fulltext: Dialect['fulltext'] = () => ({ indexes: [], virtual: Option.none() });
    static readonly policy: Dialect['policy'] = () => Option.none();
    static readonly comment: Dialect['comment'] = (sql, _, identity) => ({
        inline: sql.literal(''),
        statements: (relation, column) => Option.toArray(column).map((name) => sql`ALTER TABLE ${relation} ${commented(sql, name, identity)}`),
    });

    // --- [RECONCILIATION]

    static readonly refusals: Dialect['refusals'] = ({ name: owner, columns, indexes, references, search, tenant, mergeTree: { expires, version, codecs } }) => {
        const all = Object.values(columns);
        const named = {
            reference: references.flatMap(Struct.get('columns')),
            unique: indexes.filter(({ unique, where, method }) => unique || Array.isReadonlyArrayNonEmpty(where) || !Clickhouse.methods.includes(method)).flatMap(({ keys }) => keys.map(Struct.get('column'))),
            tenant: Option.toArray(tenant).map(({ column }) => column.name),
            fulltext: Option.toArray(search).flatMap(({ columns: searched }) => searched.map(Struct.get('name'))),
        };
        const sources = Array.getSomes(all.map(({ projection }) => Option.map(Option.filter(projection, Column.Projection.$is('Path')), Struct.get('from'))));
        const expiring = Option.map(expires, Struct.get('column'));
        return [
            ...Record.collect(named, (refusal, names) => unsupported(owner, refusal, all, ({ name }) => names.includes(name))).flat(),
            ...unsupported(owner, 'collection', all, ({ name, nullable, collection, default: preset }) => Option.isSome(collection) && (nullable || Option.isSome(preset) || sources.includes(name))),
            ...unsupported(owner, 'layout', all, ({ name, kind, projection }) => (Option.contains(expiring, name) && kind !== 'instant') || (Option.contains(version, name) && kind !== 'integer' && kind !== 'instant') || (Record.has(codecs, name) && Option.isSome(projection))),
        ];
    };
    static readonly role = unmigrated;
    static readonly open: Dialect['open'] = () => Effect.void;
    static readonly execute: Dialect['execute'] = (statements) =>
        Effect.flatMap(ClickhouseClient.ClickhouseSettings, (settings) => Effect.all(statements, { discard: true }).pipe(Effect.provideService(ClickhouseClient.ClientMethod, 'command'), Effect.provideService(ClickhouseClient.ClickhouseSettings, { ...settings, alter_sync: '2', mutations_sync: '2' })));
    static readonly transactional = false;
    static readonly plan: Dialect['plan'] = Effect.fnUntraced(
        function* (sql: SqlClient.SqlClient, catalog: Catalog) {
            const rendered = yield* render(sql, Clickhouse, catalog);
            const live = yield* observed(Array.map(catalog.tables, ({ name }) => replacement(name)));
            const resolved = diff(sql, Clickhouse, catalog, rendered, live.tables.map(Struct.assign({ references: [] })));
            const matched = yield* Effect.forEach(resolved.matched, (entry) => Effect.map(parts(sql, entry), (wanted) => aligned(entry, wanted)));
            const created = yield* Effect.forEach(resolved.created, ({ declared, rendered: table }) =>
                Effect.map(parts(sql, { declared, rendered: table }), (wanted) => [table.create(), built(sql, table.relation, { declared, parts: wanted, mark: mark(declared, table.identity, wanted), stamps: table.definitions, foreign: [] })]),
            );
            const [replaced, kept] = Array.partition(
                matched,
                Filter.fromPredicate(({ rebuild }) => rebuild),
            );
            const changes = kept.map((entry) => altered(sql, entry));
            return yield* resolved.plan({
                findings: [],
                replaced: replaced.map(({ live: table }) => table.name),
                populated: yield* resolved.populated,
                session: [],
                drifted: ({ live: table }, { definition, recorded: stamp }) => Option.isSome(table.identity) && !Option.contains(Option.flatMapNullishOr(stamp, Struct.get('digest')), definition.digest),
                preamble: [],
                opening: changes.flatMap(Struct.get('opening')),
                statements: [...live.scratch.map((name) => sql`DROP TABLE ${sql(name)} SYNC`), ...created.flat(), ...changes.flatMap(Struct.get('statements')), ...replaced.flatMap((entry) => rebuilt(sql, entry))],
                concurrent: [],
                undeclared: { indexes: matched.flatMap(({ foreign }) => foreign.filter(({ kind }) => kinds[kind].stored).map(Struct.get('name'))), references: [] },
            });
        },
        (effect, sql) => Effect.provideService(effect, SqlClient.SqlClient, sql),
    );

    // --- [LAYERS]

    static readonly layer = (config: Omit<ClickhouseClient.ClickhouseClientConfig, keyof Dialect['names']>, names: Dialect['names'] = Engine.names.snake) => Layer.merge(Layer.succeed(Engine, { ...Clickhouse, names }), ClickhouseClient.layer({ ...config, ...names }));
    static readonly layerConfig = (config: Config.Wrap<Omit<ClickhouseClient.ClickhouseClientConfig, keyof Dialect['names']>>, names: Dialect['names'] = Engine.names.snake) =>
        Layer.merge(
            Layer.succeed(Engine, { ...Clickhouse, names }),
            Config.unwrap(config).pipe(
                Config.map((settings) => ({ ...settings, ...names })),
                ClickhouseClient.layerConfig,
            ),
        );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const observed = SqlSchema.findOne({
    Request: Schema.Array(Schema.String),
    Result: Clickhouse,
    execute: (scratch) =>
        SqlClient.SqlClient.use(
            (
                sql,
            ) => sql`SELECT groupArrayIf(CAST((t.name, t.comment, c.columns, p.parts), 'Tuple(name String, identity String, columns Array(Tuple(name String, identity String, comment String, definition String, codec String, copied UInt8)), parts Array(Tuple(kind String, name String, definition String)))'), NOT has(${scratch}, t.name)) AS tables,
                    groupArrayIf(t.name, has(${scratch}, t.name)) AS scratch
                FROM system.tables t
                LEFT JOIN (SELECT table, groupArray((name, comment, comment, concat(type, ' ', default_kind, ' ', default_expression), compression_codec, default_kind IN ('', 'DEFAULT'))) AS columns FROM system.columns WHERE database = currentDatabase() GROUP BY table) c ON c.table = t.name
                LEFT JOIN (SELECT table, groupArray((kind, name, definition)) AS parts FROM (${sql.join(' UNION ALL ', false)(Record.collect(kinds, (kind, { source, definition }) => sql`SELECT table, ${kind} AS kind, name, ${sql.literal(definition)} AS definition FROM system.${sql.literal(source)} WHERE database = currentDatabase()`))}) GROUP BY table) p ON p.table = t.name
                WHERE t.database = currentDatabase() AND NOT t.is_temporary`,
        ),
});
const seconds = (after: Duration.Duration) => Math.ceil(Duration.toSeconds(after));
const ttl = (sql: SqlClient.SqlClient, { column, after }: Option.Option.Value<Relation['mergeTree']['expires']>) => sql`TTL toDateTime(${sql(column)}) + INTERVAL ${sql.literal(seconds(after).toString())} SECOND`;
const commented = (sql: SqlClient.SqlClient, name: string, identity: typeof Identity.Type) => sql`COMMENT COLUMN ${sql(name)} ${sql.literal(quoted(Formatter.formatJson(identity)))}`;
const parts = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, { declared: { columns }, rendered: { definitions, indexes } }: Effect.Success<ReturnType<typeof render>>[number]) {
    const maps = new Map(
        Object.values(columns)
            .filter(({ collection }) => Option.exists(collection, Column.Collection.$is('Map')))
            .map(({ name }) => [name, sql`mapKeys(${sql(name)})`]),
    );
    const checks = Array.getSomes(definitions.map(({ name, check }) => Option.map(check, ({ parts: label, condition }) => ({ kind: 'CONSTRAINT' as const, name: identifier(label), columns: [name], body: sql`CHECK ifNull(${condition}, 1)` }))));
    const accessed = Array.getSomes(indexes.map(({ parts: label, declared }) => Option.map(declared, Struct.assign({ name: identifier(label) })))).map(({ name, keys, include, method }) => {
        const key = sql.csv(keys.map(({ column, casefold }) => (casefold ? Clickhouse.casefold(sql, column) : (maps.get(column) ?? sql`${sql(column)}`))));
        const part = { name, columns: [...keys.map(Struct.get('column')), ...include] };
        return UndefinedOr.match(skips[method], {
            onUndefined: () => ({ ...part, kind: 'PROJECTION' as const, body: sql`(SELECT ${sql.csv(['_part_offset', ...include.map((column) => Statement.fragment([sql(column)]))])} ORDER BY ${key})` }),
            onDefined: (skip) => ({ ...part, kind: 'INDEX' as const, body: sql`(${key}) TYPE ${sql.literal(skip)}` }),
        });
    });
    return yield* Effect.forEach([...checks, ...accessed], (part) => Effect.map(digest(sql`${part.body}`.compile(true)[0]), (hashed) => ({ ...part, digest: hashed })));
});
const mark = ({ key, mergeTree: { version, expires, codecs } }: Relation, { origin, generation }: typeof Identity.Type, wanted: Effect.Success<ReturnType<typeof parts>>): (typeof Mark)['Type'] => ({
    origin,
    generation,
    layout: { key, ...Option.getOrUndefined(Option.map(version, (column) => ({ version: column }))), ...Option.getOrUndefined(Option.map(expires, ({ column }) => ({ partition: column }))) },
    ...Option.getOrUndefined(Option.map(expires, ({ after }) => ({ expires: seconds(after) }))),
    codecs,
    parts: wanted.map(Struct.pick(['kind', 'name', 'digest', 'columns'])),
});
const recoded = (sql: SqlClient.SqlClient, { columns, mergeTree }: Relation, held: (typeof Mark)['Type']['codecs']) =>
    Array.filterMap(Object.values(columns), ({ name }) => {
        const codec = Record.get(mergeTree.codecs, name);
        return Equal.equals(codec, Record.get(held, name)) ? Result.failVoid : Result.succeed(sql`MODIFY COLUMN ${sql(name)} ${sql.literal(Option.match(codec, { onNone: () => 'REMOVE CODEC', onSome: (chain) => ['CODEC(', chain.join(', '), ')'].join('') }))}`);
    });
const built = (sql: SqlClient.SqlClient, target: Statement.Fragment, { declared, parts: wanted, mark: stamp, stamps, foreign }: Pick<Aligned, 'declared' | 'parts' | 'mark' | 'stamps' | 'foreign'>) =>
    sql`ALTER TABLE ${target} ${sql.csv([
        ...recoded(sql, declared, {}),
        ...[...wanted, ...foreign.map(({ kind, name, definition }) => ({ kind, name, body: sql.literal(definition) }))].map(({ kind, name, body }) => sql`ADD ${sql.literal(kind)} ${sql(name)} ${body}`),
        ...stamps.map(({ name, stamp: identity }) => commented(sql, name, identity)),
        sql`MODIFY COMMENT ${sql.literal(quoted(Formatter.formatJson(stamp)))}`,
    ])}`;
const aligned = (entry: ReturnType<typeof diff<typeof Table.Type & { readonly references: never[] }>>['matched'][number], wanted: Effect.Success<ReturnType<typeof parts>>) => {
    const keyed = ({ kind, name }: Pick<(typeof Part)['Type'], 'kind' | 'name'>) => ({ kind, name });
    const [stamp, present, declared] = [mark(entry.declared, entry.identity, wanted), HashSet.fromIterable(entry.live.parts.map(keyed)), HashSet.fromIterable(wanted.map(keyed))];
    const managed = Option.filter(entry.live.identity, () => entry.owned);
    const moved = new Set([...entry.dropping.map(Struct.get('name')), ...entry.renames.map(Struct.get('from'))]);
    const retired = new Set(entry.declared.retired.indexes.map(identifier));
    const stale = Option.toArray(managed).flatMap(({ parts: registered }) => {
        const current = HashSet.fromIterable(registered);
        return [...registered.filter(({ columns }) => !moved.isDisjointFrom(new Set(columns))), ...wanted.filter((part) => !HashSet.has(current, Struct.pick(part, ['kind', 'name', 'digest', 'columns']))), ...entry.live.parts.filter(({ kind, name }) => kinds[kind].stored && retired.has(name))];
    });
    const gone = HashSet.intersection(present, HashSet.fromIterable(stale.map(keyed)));
    const retyped = entry.fields.some(({ definition, found, recorded: held }) => definition.retype && Option.isSome(found) && !Option.contains(Option.flatMapNullishOr(held, Struct.get('digest')), definition.digest));
    return {
        ...entry,
        parts: wanted,
        mark: stamp,
        managed,
        gone,
        stamps: entry.fields.map(({ definition, identity }) => ({ name: definition.name, stamp: { ...identity, digest: definition.digest } })),
        missing: wanted.filter((part) => !HashSet.has(HashSet.difference(present, gone), keyed(part))),
        foreign: entry.live.parts.filter((part) => !(HashSet.has(gone, keyed(part)) || HashSet.has(declared, keyed(part)))),
        rebuild: entry.owned && (retyped || !Option.exists(entry.live.identity, ({ layout }) => Equal.equals(layout, stamp.layout))),
    };
};
const altered = (sql: SqlClient.SqlClient, entry: Aligned) => {
    const marks = entry.fields.filter(({ definition, found, recorded: held, identity }) => Option.isSome(found) && !Option.contains(held, { ...identity, digest: definition.digest }));
    const expiry = Option.filter(entry.declared.mergeTree.expires, () => Option.exists(entry.managed, ({ expires }) => !Equal.equals(expires, entry.mark.expires)));
    const actions = [
        ...entry.dropping.map(({ name }) => sql`DROP COLUMN ${sql(name)}`),
        ...entry.added.map(({ text }) => sql`ADD COLUMN ${text}`),
        ...Option.toArray(entry.managed).flatMap(({ codecs }) => recoded(sql, entry.declared, Record.mapKeys(codecs, entry.renamed))),
        ...entry.missing.flatMap(({ kind, name, body }) => [sql`ADD ${sql.literal(kind)} ${sql(name)} ${body}`, ...(kinds[kind].stored ? [sql`MATERIALIZE ${sql.literal(kind)} ${sql(name)}`] : [])]),
        ...[...entry.added.map((definition) => ({ definition, identity: definition.identity })), ...marks].map(({ definition, identity }) => commented(sql, definition.name, { ...identity, digest: definition.digest })),
        ...Option.toArray(Option.filter(entry.managed, (held) => !Equal.equals(held, entry.mark))).map(() => sql`MODIFY COMMENT ${sql.literal(quoted(Formatter.formatJson(entry.mark)))}`),
        ...Option.toArray(expiry).map((expires) => sql`MODIFY ${ttl(sql, expires)}`),
    ];
    const stale = Array.fromIterable(entry.gone).map(({ kind, name }) => sql`DROP ${sql.literal(kind)} ${sql(name)}`);
    return {
        opening: Array.match(stale, { onEmpty: () => [], onNonEmpty: (all) => [sql`ALTER TABLE ${sql(entry.live.name)} ${sql.csv(all)}`] }),
        statements: Array.match(actions, { onEmpty: () => [], onNonEmpty: (all) => [sql`ALTER TABLE ${entry.rendered.relation} ${sql.csv(all)}`] }),
    };
};
const rebuilt = (sql: SqlClient.SqlClient, entry: Aligned) => {
    const target = sql`${sql(replacement(entry.declared.name))}`;
    const elements = entry.retained.map(({ name, comment, definition, codec }) => sql.join(' ', false)([Statement.fragment([sql(name)]), definition, 'COMMENT', quoted(comment), codec]));
    const copied = [
        ...Array.filterMap(entry.fields, ({ definition, found }) => (Option.isNone(definition.projection) && Option.isSome(found) ? Result.succeed({ from: found.value.name, to: definition.name }) : Result.failVoid)),
        ...entry.retained.filter(Struct.get('copied')).map(({ name }) => ({ from: name, to: name })),
    ];
    const listed = (side: 'from' | 'to') => sql.csv(copied.map((pair) => sql`${sql(pair[side])}`));
    return [
        entry.rendered.create({ target: () => target, elements, claimed: Option.some(entry) }),
        built(sql, target, entry),
        sql`INSERT INTO ${target} (${listed('to')}) SELECT ${listed('from')} FROM ${entry.rendered.relation}`,
        sql`EXCHANGE TABLES ${entry.rendered.relation} AND ${target}`,
        sql`DROP TABLE ${target} SYNC`,
    ];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Clickhouse };
