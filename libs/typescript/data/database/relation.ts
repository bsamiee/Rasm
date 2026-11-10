import { Array, Data, Duration, Effect, Equal, Filter, Function, Option, Record, Schema, Struct } from 'effect';
import { SqlClient, SqlSchema, Statement } from 'effect/sql';
import { Column, type FieldName, type Fields, type Where } from '#/column';
import { Rows } from '#/engine';
import type { methods, referentialActions } from '#/reconcile';

// --- [TYPES] ---------------------------------------------------------------------------

type Present<F extends Fields> = string extends keyof F ? string : { readonly [K in keyof F]: null extends F[K]['Encoded'] ? never : K }[keyof F] & string;
type Method = (typeof methods)[number];
type Tokenizer = Data.TaggedEnum<{ Trigram: Record<never, never>; Words: { readonly configuration: string; readonly filters: readonly string[]; readonly tokenize: string } }>;
interface RelationOptions<F extends Fields> {
    readonly key?: Array.NonEmptyReadonlyArray<Present<F>>;
    readonly defaults?: { readonly [K in keyof F]?: Exclude<F[K]['Encoded'], Uint8Array | readonly number[]> };
    readonly projections?: { readonly [K in FieldName<F>]?: { readonly from: FieldName<F>; readonly path: Array.NonEmptyReadonlyArray<string> } | { readonly where: Where<F> } };
    readonly indexes?: ReadonlyArray<{
        readonly columns: Array.NonEmptyReadonlyArray<FieldName<F> | { readonly casefold: FieldName<F> }>;
        readonly unique?: true;
        readonly where?: Where<F>;
        readonly include?: readonly FieldName<F>[];
        readonly method?: Method;
    }>;
    readonly references?: ReadonlyArray<{ readonly columns: Array.NonEmptyReadonlyArray<FieldName<F>>; readonly table: Relation; readonly onDelete?: (typeof referentialActions)[number] }>;
    readonly search?: { readonly columns: Array.NonEmptyReadonlyArray<FieldName<F>>; readonly tokenizer: Tokenizer };
    readonly withoutRowid?: true;
    readonly tenant?: { readonly column: Present<F>; readonly setting: string };
    readonly mergeTree?: { readonly expires?: { readonly column: Present<F>; readonly after: Duration.Input }; readonly version?: Present<F>; readonly codecs?: { readonly [K in FieldName<F>]?: Array.NonEmptyReadonlyArray<string> } };
    readonly renamed?: { readonly [K in FieldName<F>]?: string };
    readonly renamedFrom?: string;
    readonly retype?: readonly FieldName<F>[];
    readonly retired?: readonly string[];
    readonly retiredIndexes?: readonly Array.NonEmptyReadonlyArray<string>[];
}
type Written<F extends Fields, O extends RelationOptions<F>> = Schema.Struct.Type<Omit<F, keyof O['projections']>>;
type Selection<F extends Fields> = { readonly [K in FieldName<F>]?: Array.NonEmptyReadonlyArray<F[K]['Type']> | { readonly not: Array.NonEmptyReadonlyArray<F[K]['Type']> } };

// --- [MODELS] --------------------------------------------------------------------------

const Tokenizer = Data.taggedEnum<Tokenizer>();
const Search = Schema.Struct({ text: Schema.String, pattern: Schema.String, count: Schema.Natural, marks: Schema.Option(Schema.Struct({ open: Schema.String, close: Schema.String })) });
class Relation<const Shape extends Fields = Fields, const Options extends RelationOptions<Shape> = RelationOptions<Shape>> extends Data.Class<{
    readonly schema: Schema.Struct<Shape>;
    readonly options: Option.Option<Options>;
    readonly name: string;
    readonly renamedFrom: Option.Option<string>;
    readonly key: Array.NonEmptyReadonlyArray<string>;
    readonly columns: { readonly [K in FieldName<Shape>]: Column };
    readonly indexes: readonly {
        readonly keys: Array.NonEmptyReadonlyArray<{ readonly column: string; readonly casefold: boolean }>;
        readonly unique: boolean;
        readonly include: readonly string[];
        readonly where: ReturnType<typeof Column.conditions>;
        readonly method: Method;
        readonly parts: readonly string[];
    }[];
    readonly references: readonly { readonly columns: Array.NonEmptyReadonlyArray<string>; readonly target: string; readonly key: Array.NonEmptyReadonlyArray<string>; readonly onDelete: (typeof referentialActions)[number] }[];
    readonly search: Option.Option<{ readonly columns: Array.NonEmptyReadonlyArray<Column>; readonly tokenizer: Tokenizer }>;
    readonly withoutRowid: boolean;
    readonly tenant: Option.Option<{ readonly column: Column; readonly setting: string }>;
    readonly mergeTree: { readonly expires: Option.Option<{ readonly column: string; readonly after: Duration.Duration }>; readonly version: Option.Option<string>; readonly codecs: { readonly [column: string]: Array.NonEmptyReadonlyArray<string> } };
    readonly retired: { readonly columns: readonly string[]; readonly indexes: readonly (readonly string[])[] };
    readonly paths: readonly string[];
    readonly queries: ReturnType<typeof queries<Shape>>;
}> {
    static readonly make = <const F extends Fields, const O extends RelationOptions<F> = Record<never, never>>(name: string, fields: F, ...[options]: 'id' extends Present<F> ? readonly [options?: O] : readonly [options: O & { readonly key: Array.NonEmptyReadonlyArray<Present<F>> }]): Relation<F, O> => {
        const [schema, declared] = [Schema.Struct(fields), { name, ...definition<F>(fields, options ?? {}, Function.identity) }];
        return new Relation<F, O>({ schema, options: Option.fromUndefinedOr(options), ...declared, queries: queries(schema, declared) });
    };
    static readonly insert = <F extends Fields, O extends RelationOptions<F>>(relation: Relation<F, O>, rows: Array.NonEmptyReadonlyArray<Written<F, O>>) => Effect.catchTag(relation.queries.insert(rows), 'NoSuchElementError', Effect.die);
    static readonly upsert = <F extends Fields, O extends RelationOptions<F>>(relation: Relation<F, O>, rows: Array.NonEmptyReadonlyArray<Written<F, O>>, ...kept: readonly Exclude<FieldName<F>, keyof O['projections']>[]) => relation.queries.upsert({ rows, kept });
    static readonly ensure = <F extends Fields, O extends RelationOptions<F>>(relation: Relation<F, O>, rows: Array.NonEmptyReadonlyArray<Written<F, O>>) => Effect.catchTag(relation.queries.ensure(rows), 'NoSuchElementError', Effect.die);
    static readonly select = <F extends Fields, O extends RelationOptions<F>>(
        relation: Relation<F, O>,
        where: Selection<F>,
        { order = [], limit, per }: { readonly order?: ReadonlyArray<FieldName<F> | { readonly descending: FieldName<F> }> } & ({ readonly limit?: number; readonly per?: never } | { readonly limit: number; readonly per: FieldName<F> }) = {},
    ) => relation.queries.select({ where, order, limit: Option.fromUndefinedOr(limit), per: Option.fromUndefinedOr(per) });
    static readonly remove = <F extends Fields, O extends RelationOptions<F>>(relation: Relation<F, O>, where: Selection<F>) => relation.queries.remove(where);
    static readonly search = <F extends Fields, O extends RelationOptions<F> & { readonly search: NonNullable<RelationOptions<F>['search']> }>(relation: Relation<F, O>, text: string, limit: number, marks?: { readonly open: string; readonly close: string }) =>
        Effect.flatMap(Effect.orDie(Effect.fromOption(relation.queries.search)), (find) => find({ text, pattern: `%${text.replaceAll(/[\\%_]/gu, '\\$&')}%`, count: limit, marks: Option.fromUndefinedOr(marks) }));
    static readonly withTenant = Effect.fnUntraced(function* <F extends Fields, O extends RelationOptions<F> & { readonly tenant: NonNullable<RelationOptions<F>['tenant']> }, A, E, R>(relation: Relation<F, O>, id: F[O['tenant']['column']]['Type'], effect: Effect.Effect<A, E, R>) {
        const { column, setting } = yield* Effect.orDie(Effect.fromOption(relation.tenant));
        const [sql, encoded] = yield* Effect.all([SqlClient.SqlClient, Schema.encodeEffect(column.schema)(id)]);
        return yield* sql.withTransaction(Effect.andThen(sql`SELECT set_config(${setting}, ${encoded}, true)`, effect));
    });
    named(rename: (name: string) => string) {
        return new Relation<Shape, Options>({ ...this, name: rename(this.name), ...definition(this.schema.fields, Option.getOrElse(this.options, () => ({})), rename) });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DEFINITIONS]

const definition = <F extends Fields>(fields: F, options: RelationOptions<F>, rename: (name: string) => string) => {
    const key = Array.map(options.key ?? Array.make('id'), rename);
    const retired = (options.retired ?? []).map(rename);
    const declared = (options.indexes ?? []).map((index) => {
        const { columns, unique, include, where, method } = { unique: false, include: [], method: 'btree' as const, where: {}, ...index };
        return {
            keys: Array.map(columns, (entry) => (typeof entry === 'string' ? { column: rename(entry), casefold: false } : { column: rename(entry.casefold), casefold: true })),
            unique,
            include: include.map(rename),
            where: Column.conditions(where, rename),
            method,
        };
    });
    const linked = (options.references ?? []).map(({ columns: linking, table, onDelete = 'NO ACTION' }) => ({ columns: Array.map(linking, rename), target: rename(table.name), key: Array.map(table.key, rename), onDelete }));
    const orders = [key, ...declared.filter(({ method }) => method === 'btree').map((index) => Array.takeWhile(index.keys, ({ casefold }) => !casefold).map(({ column }) => column))];
    const covering = linked.filter((reference) => !orders.some((order) => Equal.equals(Array.take(order, reference.columns.length), reference.columns)));
    const columns = Record.map<FieldName<F>, F[FieldName<F>], Column>(fields, (schema, field) => Column.make<F>(field, schema, options, retired, rename));
    return {
        renamedFrom: Option.map(Option.fromUndefinedOr(options.renamedFrom), rename),
        key,
        columns,
        indexes: [...declared, ...covering.map((reference) => ({ keys: Array.map(reference.columns, (column) => ({ column, casefold: false })), unique: false, include: [], where: [], method: 'btree' as const }))].map((index) => ({
            ...index,
            parts: [...index.keys.map(({ column }) => column), ...(index.method === 'btree' ? [] : [index.method])],
        })),
        references: linked,
        search: Option.map(Option.fromUndefinedOr(options.search), ({ columns: searched, tokenizer }) => ({ columns: Array.map(searched, (field): Column => columns[field]), tokenizer })),
        withoutRowid: options.withoutRowid === true,
        tenant: Option.map(Option.fromUndefinedOr(options.tenant), ({ column, setting }) => ({ column: columns[column], setting })),
        mergeTree: {
            expires: Option.map(Option.fromUndefinedOr(options.mergeTree?.expires), ({ column, after }) => ({ column: rename(column), after: Duration.fromInputUnsafe(after) })),
            version: Option.map(Option.fromUndefinedOr(options.mergeTree?.version), rename),
            codecs: Record.fromIterableWith(Object.entries<Array.NonEmptyReadonlyArray<string>>(options.mergeTree?.codecs ?? {}), ([column, codec]) => [rename(column), codec]),
        },
        retired: { columns: retired, indexes: (options.retiredIndexes ?? []).map((parts) => parts.map(rename)) },
        paths: Array.getSomes(Object.values<Column>(columns).map(({ projection }) => Option.filter(projection, Column.Projection.$is('Path')))).map(Struct.get('jsonPath')),
    };
};

// --- [QUERIES]

const selecting = (
    { sql, owner, prefix, unqualified, reads, keys, where: matching }: ReturnType<typeof Rows.fragments>,
    columns: Relation['columns'],
    { where, order, limit, per }: { readonly where: Parameters<ReturnType<typeof Rows.fragments>['where']>[0]; readonly order: ReadonlyArray<string | { readonly descending: string }>; readonly limit: Option.Option<number>; readonly per: Option.Option<string> },
) => {
    const ordering = (scope: Statement.Fragment) =>
        sql.csv([
            ...Array.filterMap(
                order,
                Filter.fromPredicateOption((entry) => Option.map(Record.get(columns, typeof entry === 'string' ? entry : entry.descending), ({ name }) => sql.join(' ', false)([sql`${scope}${sql(name)}`, ...(typeof entry === 'string' ? [] : ['DESC'])]))),
            ),
            keys(scope),
        ]);
    const [outer, filter] = [sql.literal('t.'), matching(where)];
    const filtered = sql`SELECT ${reads(unqualified)} FROM ${owner} WHERE ${filter} ORDER BY ${ordering(prefix)}`;
    return Option.match(Option.all({ count: limit, by: Option.flatMap(per, (field) => Record.get(columns, field)) }), {
        onNone: () => Option.match(limit, { onNone: () => filtered, onSome: (count) => sql`${filtered} LIMIT ${count}` }),
        onSome: ({ count, by }) =>
            sql`SELECT ${reads(outer)} FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY ${sql(by.name)} ORDER BY ${ordering(prefix)}) AS ${sql(Rows.flags.position)} FROM ${owner} WHERE ${filter}) t WHERE t.${sql(Rows.flags.position)} <= ${count} ORDER BY ${sql.csv([sql.join('', false)([outer, Statement.fragment([sql(by.name)])]), ordering(outer)])}`,
    });
};
const queries = <F extends Fields>(schema: Schema.Struct<F>, relation: Pick<Relation, 'name' | 'columns' | 'key' | 'search'>) => {
    const run = <A, E>(statement: (context: ReturnType<typeof Rows.fragments>) => Effect.Effect<A, E, SqlClient.SqlClient>) => Effect.flatMap(Effect.all([SqlClient.SqlClient, Rows]), ([sql, rows]) => statement(Rows.fragments(sql, rows, relation)));
    const all = Object.values<Column>(relation.columns);
    const wire = (entry: Column) => (entry.kind === 'blob' ? Schema.encodeTo(Schema.NullOr(Schema.Uint8ArrayFromHex))(entry.schema) : entry.schema);
    const written = all.filter(({ projection }) => Option.isNone(projection));
    const keyed = written.filter(({ name }) => relation.key.includes(name)).map(Struct.get('name'));
    const list = (entry: Column) => Schema.fromJsonString(Schema.NonEmptyArray(wire(entry)));
    const Batch = Schema.Array(Schema.Struct(Record.fromIterableWith(written, (entry) => [entry.name, wire(entry)])));
    const Selection = Schema.Struct(Record.fromIterableWith(all, (entry) => [entry.name, Schema.optionalKey(Schema.Union([list(entry), Schema.Struct({ not: list(entry) })]))]));
    return {
        insert: SqlSchema.findNonEmpty({ Request: Schema.fromJsonString(Batch), Result: schema, execute: (body) => run(({ unqualified, reads, inserting }) => inserting(body, unqualified, reads(unqualified))) }),
        ensure: SqlSchema.findNonEmpty({ Request: Schema.fromJsonString(Batch), Result: schema, execute: (body) => run((context) => context.rows.ensure(context, body)) }),
        upsert: SqlSchema.findAll({
            Request: Schema.Struct({
                rows: Schema.fromJsonString(Batch.check(Schema.makeFilter((rows) => Array.dedupe(rows.map(Struct.pick(keyed))).length === rows.length, { expected: 'rows with distinct keys' }))),
                kept: Schema.Array(Schema.String),
            }),
            Result: Schema.Struct({ row: schema, inserted: Schema.BooleanFromBit }),
            execute: ({ rows, kept }) =>
                run((context) =>
                    context.rows
                        .upsert(
                            context,
                            rows,
                            written.filter(({ name }) => !(relation.key.includes(name) || kept.includes(name))),
                        )
                        .pipe(Effect.map(Array.map(({ [Rows.flags.inserted]: inserted, ...row }) => ({ row, inserted })))),
                ),
        }),
        select: SqlSchema.findAll({
            Request: Schema.Struct({ where: Selection, order: Schema.Array(Schema.Union([Schema.String, Schema.Struct({ descending: Schema.String })])), limit: Schema.Option(Schema.Natural), per: Schema.Option(Schema.String) }),
            Result: schema,
            execute: (request) => run((context) => selecting(context, relation.columns, request)),
        }),
        remove: SqlSchema.findAll({ Request: Selection, Result: schema, execute: (where) => run(({ sql, owner, reads, unqualified, where: matching }) => sql`DELETE FROM ${owner} WHERE ${matching(where)} RETURNING ${reads(unqualified)}`) }),
        search: Option.map(relation.search, (searchable) =>
            SqlSchema.findAll({
                Request: Search,
                Result: Schema.Struct({ row: schema, rank: Schema.Finite, highlight: Schema.OptionFromNullOr(Schema.fromJsonString(Schema.Record(Schema.String, Schema.NullOr(Schema.String)))) }),
                execute: ({ text, pattern, count, marks }) => run((context) => context.rows.search(context, searchable, text, pattern, marks, count)).pipe(Effect.map(Array.map(({ [Rows.flags.rank]: rank, [Rows.flags.highlight]: highlight, ...row }) => ({ row, rank, highlight })))),
            }),
        ),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Relation, type RelationOptions, Tokenizer };
