import { Array, Data, Effect, Equal, Exit, Option, Record, Schema, String, Struct } from 'effect';
import { SqlClient, type SqlError, SqlSchema, type Statement } from 'effect/sql';
import { Column, type Field, type Fields, type Where } from '#/column';
import { Engine } from '#/engine';
import type { Reconcile } from '#/reconcile';

// --- [TYPES] ---------------------------------------------------------------------------

type Present<F extends Fields> = string extends keyof F ? string : { readonly [K in keyof F]: null extends F[K]['Encoded'] ? never : K }[keyof F] & string;
type Method = (typeof Reconcile.methods)[number];
type Tokenizer = 'trigram' | { readonly configuration: string; readonly filters: readonly string[]; readonly tokenize: string };
interface RelationOptions<F extends Fields> {
    readonly key?: Array.NonEmptyReadonlyArray<Present<F>>;
    readonly defaults?: { readonly [K in keyof F]?: Exclude<F[K]['Encoded'], Uint8Array | readonly number[]> };
    readonly projections?: { readonly [K in Field<F>]?: { readonly from: Field<F>; readonly path: Array.NonEmptyReadonlyArray<string> } | { readonly where: Where<F> } };
    readonly indexes?: ReadonlyArray<{
        readonly columns: Array.NonEmptyReadonlyArray<Field<F> | { readonly casefold: Field<F> }>;
        readonly unique?: true;
        readonly where?: Where<F>;
        readonly include?: readonly Field<F>[];
        readonly method?: Method;
    }>;
    readonly references?: ReadonlyArray<{ readonly columns: Array.NonEmptyReadonlyArray<Field<F>>; readonly table: Relation; readonly onDelete?: (typeof Reconcile.referentialActions)[number] }>;
    readonly search?: { readonly columns: Array.NonEmptyReadonlyArray<Field<F>>; readonly tokenizer: Tokenizer };
    readonly withoutRowid?: true;
    readonly tenant?: { readonly column: Present<F>; readonly setting: string };
    readonly renamed?: { readonly [K in Field<F>]?: string };
    readonly renamedFrom?: string;
    readonly retype?: readonly Field<F>[];
    readonly retired?: readonly string[];
    readonly retiredIndexes?: readonly Array.NonEmptyReadonlyArray<string>[];
}
type Written<F extends Fields, O extends RelationOptions<F>> = Schema.Struct.Type<Omit<F, keyof O['projections']>>;
type Selection<F extends Fields> = { readonly [K in Field<F>]?: Array.NonEmptyReadonlyArray<F[K]['Type']> | { readonly not: Array.NonEmptyReadonlyArray<F[K]['Type']> } };
type Fragments = ReturnType<typeof Engine.fragments>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const like = /[\\%_]/gu;

// --- [MODELS] --------------------------------------------------------------------------

class Relation<const Shape extends Fields = Fields, const Options extends RelationOptions<Shape> = RelationOptions<Shape>> extends Data.Class<{
    readonly schema: Schema.Struct<Shape>;
    readonly options: Option.Option<Options>;
    readonly name: string;
    readonly renamedFrom: Option.Option<string>;
    readonly key: Array.NonEmptyReadonlyArray<string>;
    readonly columns: { readonly [K in Field<Shape>]: Column };
    readonly indexes: readonly {
        readonly keys: Array.NonEmptyReadonlyArray<{ readonly column: string; readonly casefold: boolean }>;
        readonly unique: boolean;
        readonly include: readonly string[];
        readonly where: ReturnType<typeof Column.conditions>;
        readonly method: Method;
        readonly parts: readonly string[];
    }[];
    readonly references: readonly { readonly columns: Array.NonEmptyReadonlyArray<string>; readonly table: Relation; readonly onDelete: (typeof Reconcile.referentialActions)[number] }[];
    readonly search: Option.Option<{ readonly columns: Array.NonEmptyReadonlyArray<Column>; readonly tokenizer: Tokenizer }>;
    readonly withoutRowid: boolean;
    readonly tenant: Option.Option<{ readonly column: Column; readonly setting: string }>;
    readonly retired: { readonly columns: readonly string[]; readonly indexes: readonly (readonly string[])[] };
    readonly paths: readonly string[];
    readonly queries: ReturnType<typeof queries<Shape>>;
}> {
    static readonly make = <const F extends Fields, const O extends RelationOptions<F> = Record<never, never>>(name: string, fields: F, ...[options]: 'id' extends Present<F> ? readonly [options?: O] : readonly [options: O & { readonly key: Array.NonEmptyReadonlyArray<Present<F>> }]): Relation<F, O> => {
        const [schema, declared] = [Schema.Struct(fields), { name: String.camelToSnake(name), ...definition<F>(fields, options ?? {}) }];
        return new Relation<F, O>({ schema, options: Option.fromUndefinedOr(options), ...declared, queries: queries(schema, declared) });
    };
    static readonly insert = <F extends Fields, O extends RelationOptions<F>>(relation: Relation<F, O>, rows: Array.NonEmptyReadonlyArray<Written<F, O>>) => Effect.catchTag(relation.queries.insert(rows), 'NoSuchElementError', Effect.die);
    static readonly upsert = <F extends Fields, O extends RelationOptions<F>>(relation: Relation<F, O>, rows: Array.NonEmptyReadonlyArray<Written<F, O>>, ...kept: readonly Exclude<Field<F>, keyof O['projections']>[]) => relation.queries.upsert({ rows, kept });
    static readonly ensure = <F extends Fields, O extends RelationOptions<F>>(relation: Relation<F, O>, rows: Array.NonEmptyReadonlyArray<Written<F, O>>) => Effect.catchTag(relation.queries.ensure(rows), 'NoSuchElementError', Effect.die);
    static readonly select = <F extends Fields, O extends RelationOptions<F>>(
        relation: Relation<F, O>,
        where: Selection<F>,
        { order = [], limit, per }: { readonly order?: ReadonlyArray<Field<F> | { readonly descending: Field<F> }> } & ({ readonly limit?: number; readonly per?: never } | { readonly limit: number; readonly per: Field<F> }) = {},
    ) => relation.queries.select({ where, order, limit: Option.fromUndefinedOr(limit), per: Option.fromUndefinedOr(per) });
    static readonly remove = <F extends Fields, O extends RelationOptions<F>>(relation: Relation<F, O>, where: Selection<F>) => relation.queries.remove(where);
    static readonly search = <F extends Fields, O extends RelationOptions<F> & { readonly search: NonNullable<RelationOptions<F>['search']> }>(relation: Relation<F, O>, text: string, limit: number, marks?: { readonly open: string; readonly close: string }) =>
        Option.match(relation.queries.search, {
            onNone: () => Effect.succeed([]),
            onSome: (find) => find({ text, pattern: `%${text.replaceAll(like, (character) => `\\${character}`)}%`, count: limit, marks: Option.fromUndefinedOr(marks) }),
        });
    static readonly withTenant = <F extends Fields, O extends RelationOptions<F> & { readonly tenant: NonNullable<RelationOptions<F>['tenant']> }, A, E, R>(relation: Relation<F, O>, id: F[O['tenant']['column']]['Type'], effect: Effect.Effect<A, E, R>) =>
        Option.match(relation.tenant, {
            onNone: () => effect,
            onSome: Effect.fnUntraced(function* ({ column, setting }: Option.Option.Value<Relation['tenant']>) {
                const [sql, encoded] = yield* Effect.all([SqlClient.SqlClient, Schema.encodeEffect(column.schema)(id)]);
                const scope = SqlSchema.findOne({ Request: Schema.String, Result: Schema.Struct({ prior: Schema.String }), execute: (value) => sql`WITH p AS MATERIALIZED (SELECT coalesce(current_setting(${setting}, true), '') AS prior) SELECT p.prior, set_config(${setting}, ${value}, true) FROM p` });
                return yield* sql.withTransaction(
                    Effect.acquireUseRelease(
                        scope(`${encoded}`),
                        () => effect,
                        ({ prior }, exit) => (Exit.isSuccess(exit) ? Effect.orDie(scope(prior)) : Effect.void),
                    ),
                );
            }),
        });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DEFINITION]
const definition = <F extends Fields>(fields: F, options: RelationOptions<F>) => {
    const key = Array.map(options.key ?? Array.make('id'), String.camelToSnake);
    const retired = (options.retired ?? []).map(String.camelToSnake);
    const declared = (options.indexes ?? []).map((index) => {
        const { columns, unique, include, where, method } = { unique: false, include: [], method: 'btree' as const, where: {}, ...index };
        return {
            keys: Array.map(columns, (entry) => (typeof entry === 'string' ? { column: String.camelToSnake(entry), casefold: false } : { column: String.camelToSnake(entry.casefold), casefold: true })),
            unique,
            include: include.map(String.camelToSnake),
            where: Column.conditions(where),
            method,
        };
    });
    const linked = (options.references ?? []).map((reference) => ({ onDelete: 'NO ACTION' as const, ...reference, columns: Array.map(reference.columns, String.camelToSnake) }));
    const orders = [key, ...declared.filter(({ method }) => method === 'btree').map((index) => Array.takeWhile(index.keys, ({ casefold }) => !casefold).map(({ column }) => column))];
    const covering = linked.filter((reference) => !orders.some((order) => Equal.equals(Array.take(order, reference.columns.length), reference.columns)));
    const columns = Record.map<Field<F>, F[Field<F>], Column>(fields, (schema, field) => Column.make<F>(field, schema, options, retired));
    return {
        renamedFrom: Option.map(Option.fromUndefinedOr(options.renamedFrom), String.camelToSnake),
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
        retired: { columns: retired, indexes: (options.retiredIndexes ?? []).map((parts) => parts.map(String.camelToSnake)) },
        paths: Array.getSomes(Object.values<Column>(columns).map(({ projection }) => Option.filter(projection, Column.Projection.$is('Path')))).map(Struct.get('jsonPath')),
    };
};

// --- [QUERIES]
const perform = (relation: Pick<Relation, 'name' | 'columns' | 'key'>) => (statement: (context: Fragments) => Effect.Effect<readonly unknown[], SqlError.SqlError, SqlClient.SqlClient>) => Effect.flatMap(Effect.all([SqlClient.SqlClient, Engine]), ([sql, engine]) => statement(Engine.fragments(sql, engine, relation)));
const conditions = (context: Fragments, all: readonly Column[], where: { readonly [field: string]: string | { readonly not: string } | undefined }) =>
    context.sql.and(all.flatMap((entry) => Array.fromNullishOr(where[entry.field]).map((rule) => (typeof rule === 'string' ? context.matching(entry, rule) : context.sql`(${context.matching(entry, rule.not)}) IS NOT TRUE`))));
const selecting = (context: Fragments, filter: Statement.Fragment, ordering: (scope: Statement.Fragment) => Statement.Fragment, limit: Option.Option<number>, group: Option.Option<Column>) => {
    const { sql, owner, prefix, unqualified, reads, flags } = context;
    const outer = sql.literal('t.');
    const filtered = sql`SELECT ${reads(unqualified)} FROM ${owner} WHERE ${filter} ORDER BY ${ordering(prefix)}`;
    const limited = Option.match(limit, { onNone: () => filtered, onSome: (count) => sql`${filtered} LIMIT ${count}` });
    return Option.match(Option.all({ count: limit, by: group }), {
        onNone: () => limited,
        onSome: ({ count, by }) =>
            sql`SELECT ${reads(outer)} FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY ${sql(by.name)} ORDER BY ${ordering(prefix)}) AS ${sql(flags.position)} FROM ${owner} WHERE ${filter}) t WHERE t.${sql(flags.position)} <= ${count} ORDER BY ${sql.csv([sql`t.${sql(by.name)}`, ordering(outer)])}`,
    });
};
const queries = <F extends Fields>(schema: Schema.Struct<F>, relation: Pick<Relation, 'name' | 'columns' | 'key' | 'search'>) => {
    const run = perform(relation);
    const all = Object.values<Column>(relation.columns);
    const named = (field: string) => Array.findFirst(all, (entry) => entry.field === field);
    const wire = (entry: Column) => (entry.kind === 'blob' ? Schema.encodeTo(Schema.NullOr(Schema.Uint8ArrayFromHex))(entry.schema) : entry.schema);
    const written = all.filter(({ projection }) => Option.isNone(projection));
    const keyed = written.filter(({ name }) => relation.key.includes(name));
    const list = (entry: Column) => Schema.fromJsonString(Schema.NonEmptyArray(wire(entry)));
    const Rows = Schema.Array(Schema.Struct(Record.fromIterableWith(written, (entry) => [entry.field, wire(entry)])));
    const Selection = Schema.Struct(Record.fromIterableWith(all, (entry) => [entry.field, Schema.optionalKey(Schema.Union([list(entry), Schema.Struct({ not: list(entry) })]))]));
    const ordering = (context: Fragments, order: ReadonlyArray<string | { readonly descending: string }>) => (scope: Statement.Fragment) =>
        context.sql.csv([...order.flatMap((entry) => Option.toArray(named(typeof entry === 'string' ? entry : entry.descending)).map(({ name }) => context.sql`${scope}${context.sql(name)}${context.sql.literal(typeof entry === 'string' ? '' : ' DESC')}`)), context.keys(scope)]);
    return {
        insert: SqlSchema.findNonEmpty({ Request: Schema.fromJsonString(Rows), Result: schema, execute: (body) => run(({ unqualified, reads, inserting }) => inserting(body, unqualified, reads(unqualified))) }),
        ensure: SqlSchema.findNonEmpty({ Request: Schema.fromJsonString(Rows), Result: schema, execute: (body) => run((context) => context.engine.ensure(context, body)) }),
        upsert: SqlSchema.findAll({
            Request: Schema.Struct({
                rows: Schema.fromJsonString(Rows.check(Schema.makeFilter((rows) => Array.dedupe(rows.map((row) => keyed.map(({ field }) => row[field]))).length === rows.length, { expected: 'rows with distinct keys' }))),
                kept: Schema.Array(Schema.String),
            }),
            Result: Schema.Struct({ row: schema, inserted: Schema.BooleanFromBit }),
            execute: ({ rows, kept }) =>
                run((context) =>
                    context.engine
                        .upsert(
                            context,
                            rows,
                            written.filter(({ name, field }) => !(relation.key.includes(name) || kept.includes(field))),
                        )
                        .pipe(Effect.map(Array.map(({ [context.flags.inserted]: inserted, ...row }) => ({ row, inserted })))),
                ),
        }),
        select: SqlSchema.findAll({
            Request: Schema.Struct({ where: Selection, order: Schema.Array(Schema.Union([Schema.String, Schema.Struct({ descending: Schema.String })])), limit: Schema.Option(Schema.Natural), per: Schema.Option(Schema.String) }),
            Result: schema,
            execute: ({ where, order, limit, per }) => run((context) => selecting(context, conditions(context, all, where), ordering(context, order), limit, Option.flatMap(per, named))),
        }),
        remove: SqlSchema.findAll({ Request: Selection, Result: schema, execute: (where) => run((context) => context.sql`DELETE FROM ${context.owner} WHERE ${conditions(context, all, where)} RETURNING ${context.reads(context.unqualified)}`) }),
        search: Option.map(relation.search, (searchable) =>
            SqlSchema.findAll({
                Request: Schema.Struct({ text: Schema.String, pattern: Schema.String, count: Schema.Natural, marks: Schema.Option(Schema.Struct({ open: Schema.String, close: Schema.String })) }),
                Result: Schema.Struct({ row: schema, rank: Schema.Finite, highlight: Schema.OptionFromNullOr(Schema.fromJsonString(Schema.Record(Schema.String, Schema.NullOr(Schema.String)))) }),
                execute: ({ text, pattern, count, marks }) => run((context) => context.engine.search(context, searchable, text, pattern, marks, count).pipe(Effect.map(Array.map(({ [context.flags.rank]: rank, [context.flags.highlight]: highlight, ...row }) => ({ row, rank, highlight }))))),
            }),
        ),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Present, Relation, type RelationOptions };
