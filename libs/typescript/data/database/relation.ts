import { Array, Data, Effect, Equal, flow, Option, Record, Schema, String } from 'effect';
import { SqlClient, type Statement } from 'effect/sql';
import { Column, type Field, type Fields, type Where } from '#/column';
import { statements } from '#/engine';
import type { referentialActions } from '#/reconcile';

// --- [TYPES] ---------------------------------------------------------------------------

type Present<F extends Fields> = string extends keyof F ? string : { readonly [K in keyof F]: null extends F[K]['Encoded'] ? never : K }[keyof F] & string;
interface TableOptions<F extends Fields> {
    readonly key?: Array.NonEmptyReadonlyArray<Present<F>>;
    readonly defaults?: { readonly [K in keyof F]?: Exclude<F[K]['Encoded'], Uint8Array | readonly number[]> };
    readonly projections?: { readonly [K in Field<F>]?: { readonly from: Field<F>; readonly path: Array.NonEmptyReadonlyArray<string> } | { readonly where: Where<F> } };
    readonly indexes?: ReadonlyArray<{
        readonly columns: Array.NonEmptyReadonlyArray<Field<F> | { readonly casefold: Field<F> }>;
        readonly unique?: true;
        readonly where?: Where<F>;
        readonly include?: readonly Field<F>[];
        readonly method?: 'gin' | 'gist' | 'brin';
    }>;
    readonly references?: ReadonlyArray<{ readonly columns: Array.NonEmptyReadonlyArray<Field<F>>; readonly table: Relation; readonly onDelete?: keyof typeof referentialActions }>;
    readonly search?: { readonly columns: Array.NonEmptyReadonlyArray<Field<F>>; readonly tokenizer: 'trigram' | { readonly configuration: string; readonly filters: readonly string[]; readonly tokenize: string } };
    readonly withoutRowid?: true;
    readonly tenant?: { readonly column: Present<F>; readonly setting: string };
    readonly renamed?: { readonly [K in Field<F>]?: string };
    readonly renamedFrom?: string;
    readonly retype?: readonly Field<F>[];
    readonly retired?: readonly string[];
    readonly retiredIndexes?: readonly Array.NonEmptyReadonlyArray<string>[];
}
type Written<F extends Fields, O extends TableOptions<F>> = Schema.Struct.Type<Omit<F, keyof O['projections']>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const like = /[\\%_]/gu;

// --- [MODELS] --------------------------------------------------------------------------

class Relation<const S extends Fields = Fields, const T extends TableOptions<S> = TableOptions<S>> extends Data.Class<{
    readonly schema: Schema.Struct<S>;
    readonly options: T;
    readonly name: string;
    readonly earlier: Option.Option<string>;
    readonly key: Array.NonEmptyReadonlyArray<string>;
    readonly columns: { readonly [K in Field<S>]: Column };
    readonly indexes: readonly {
        readonly keys: Array.NonEmptyReadonlyArray<{ readonly column: string; readonly casefold: boolean }>;
        readonly unique: boolean;
        readonly include: readonly string[];
        readonly where: ReturnType<typeof Column.conditions>;
        readonly method: string;
        readonly parts: readonly string[];
    }[];
    readonly references: readonly { readonly columns: Array.NonEmptyReadonlyArray<string>; readonly table: Relation; readonly onDelete: keyof typeof referentialActions }[];
    readonly search: Option.Option<{ readonly columns: Array.NonEmptyReadonlyArray<Column>; readonly tokenizer: NonNullable<TableOptions<Fields>['search']>['tokenizer'] }>;
    readonly withoutRowid: boolean;
    readonly tenant: Option.Option<{ readonly column: string; readonly setting: string }>;
    readonly retired: { readonly columns: readonly string[]; readonly indexes: readonly (readonly string[])[] };
    readonly codecs: ReturnType<typeof codecs<S>>;
}> {
    static readonly make = <const F extends Fields, const O extends TableOptions<F>>(
        name: string,
        fields: F,
        options: O & ('id' extends Present<F> ? unknown : { readonly key: Array.NonEmptyReadonlyArray<Present<F>> }) & ('inserted' extends keyof F ? never : unknown) & (Extract<keyof F, 'rank' | 'highlight'> extends never ? unknown : { readonly search?: never }),
    ): Relation<F, O> => {
        const [schema, declared] = [Schema.Struct(fields), definition<F>(fields, options)];
        return new Relation<F, O>({ schema, options, name: String.camelToSnake(name), ...declared, codecs: codecs(schema, declared.columns, declared.key) });
    };
    static readonly insert = <F extends Fields, O extends TableOptions<F>>(relation: Relation<F, O>, rows: Array.NonEmptyReadonlyArray<Written<F, O>>) => run(relation, relation.codecs.one, relation.codecs.rows(rows), ({ bare, reads, inserting }, body) => inserting(body, bare, reads(bare)));
    static readonly upsert = <F extends Fields, O extends TableOptions<F>>(relation: Relation<F, O>, rows: Array.NonEmptyReadonlyArray<Written<F, O>>, ...kept: readonly Field<F>[]) => {
        const fixed = [...relation.key, ...kept.map((field) => relation.columns[field].name)];
        return run(relation, relation.codecs.changed, relation.codecs.distinct(rows), (context, body) =>
            context.picked.upsert(
                context,
                body,
                context.written.filter(({ name }) => !fixed.includes(name)),
            ),
        );
    };
    static readonly ensure = <F extends Fields, O extends TableOptions<F>>(relation: Relation<F, O>, rows: Array.NonEmptyReadonlyArray<Written<F, O>>) => run(relation, relation.codecs.one, relation.codecs.rows(rows), (context, body) => context.picked.ensure(context, body));
    static readonly select = <F extends Fields, O extends TableOptions<F>, K extends Field<F>>(relation: Relation<F, O>, field: K, values: Array.NonEmptyReadonlyArray<F[K]['Type']>, ...order: ReadonlyArray<Field<F> | { readonly descending: Field<F> }>) =>
        run(relation, relation.codecs.many, relation.codecs.lists[field](values), ({ sql, owner, prefix, bare, keys, reads, matching }, body) => {
            const ordered = order.map((entry) => (typeof entry === 'string' ? sql`${prefix}${sql(relation.columns[entry].name)}` : sql`${prefix}${sql(relation.columns[entry.descending].name)} DESC`));
            return sql`SELECT ${reads(bare)} FROM ${owner} WHERE ${matching(relation.columns[field], body)} ORDER BY ${sql.csv([...ordered, keys(prefix)])}`;
        });
    static readonly remove = <F extends Fields, O extends TableOptions<F>, K extends Field<F>>(relation: Relation<F, O>, field: K, values: Array.NonEmptyReadonlyArray<F[K]['Type']>) =>
        run(relation, relation.codecs.many, relation.codecs.lists[field](values), ({ sql, owner, bare, reads, matching }, body) => sql`DELETE FROM ${owner} WHERE ${matching(relation.columns[field], body)} RETURNING ${reads(bare)}`);
    static readonly latest = <F extends Fields, O extends TableOptions<F>, G extends Field<F>>(relation: Relation<F, O>, group: G, order: Array.NonEmptyReadonlyArray<Field<F>>, groups: Array.NonEmptyReadonlyArray<F[G]['Type']>, limit: number) =>
        run(relation, relation.codecs.many, Effect.all([relation.codecs.lists[group](groups), bound(limit)]), (context, [body, count]) => {
            const { sql, picked, owner } = context;
            const by = relation.columns[group];
            const ranked = (scope: Statement.Fragment) => sql.csv([...order.map((field) => relation.columns[field].name), ...relation.key].map((part) => sql`${scope}${sql(part)} DESC`));
            return picked.latest(context, body, ranked, (selected) => sql`SELECT ${selected} FROM ${owner} i WHERE i.${sql(by.name)} = ${picked.element(sql, by, sql.literal('e.value'))} ORDER BY ${ranked(sql.literal('i.'))} LIMIT ${count}`);
        });
    static readonly search = <F extends Fields, O extends TableOptions<F> & { readonly search: NonNullable<TableOptions<F>['search']> }>(relation: Relation<F, O>, text: string, limit: number, marks?: { readonly open: string; readonly close: string }) =>
        run(relation, relation.codecs.found, bound(limit), (context, count) =>
            context.picked.search(context, { columns: Array.map(relation.options.search.columns, (field): Column => relation.columns[field]), tokenizer: relation.options.search.tokenizer }, text, `%${text.replaceAll(like, (character) => `\\${character}`)}%`, Option.fromUndefinedOr(marks), count),
        );
    static readonly tenant = <F extends Fields, O extends TableOptions<F> & { readonly tenant: NonNullable<TableOptions<F>['tenant']> }, A, E, R>(relation: Relation<F, O>, id: string, effect: Effect.Effect<A, E, R>) =>
        SqlClient.SqlClient.use((sql) => sql.withTransaction(Effect.andThen(sql`SELECT set_config(${relation.options.tenant.setting}, ${id}, true)`, effect)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const bound = Schema.encodeEffect(Schema.Natural);
const definition = <F extends Fields>(fields: F, options: TableOptions<F>) => {
    const key = Array.map(options.key ?? Array.make('id'), String.camelToSnake);
    const retired = (options.retired ?? []).map(String.camelToSnake);
    const declared = (options.indexes ?? []).map((index) => {
        const { columns, unique, include, where, method } = { unique: false, include: [], method: 'btree', where: {}, ...index };
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
        earlier: Option.map(Option.fromUndefinedOr(options.renamedFrom), String.camelToSnake),
        key,
        columns,
        indexes: [...declared, ...covering.map((reference) => ({ keys: Array.map(reference.columns, (column) => ({ column, casefold: false })), unique: false, include: [], where: [], method: 'btree' }))].map((index) => ({
            ...index,
            parts: [...index.keys.map(({ column }) => column), ...(index.method === 'btree' ? [] : [index.method])],
        })),
        references: linked,
        search: Option.map(Option.fromUndefinedOr(options.search), ({ columns: searched, tokenizer }) => ({ columns: Array.map(searched, (field): Column => columns[field]), tokenizer })),
        withoutRowid: options.withoutRowid === true,
        tenant: Option.map(Option.fromUndefinedOr(options.tenant), ({ column, setting }) => ({ column: String.camelToSnake(column), setting })),
        retired: { columns: retired, indexes: (options.retiredIndexes ?? []).map((parts) => parts.map(String.camelToSnake)) },
    };
};
const codecs = <F extends Fields>(schema: Schema.Struct<F>, columns: { readonly [K in Field<F>]: Column }, key: readonly string[]) => {
    const encoded = flow(Schema.fromJsonString, Schema.encodeUnknownEffect);
    const wire = ({ kind, schema: stored }: Column) => (kind === 'blob' ? Schema.encodeTo(Schema.NullOr(Schema.Uint8ArrayFromHex))(stored) : stored);
    const written = Object.values(columns).filter(({ projection }) => Option.isNone(projection));
    const keyed = written.filter(({ name }) => key.includes(name));
    const writable = Schema.Array(Schema.Struct(Record.fromIterableWith(written, (entry) => [entry.field, wire(entry)])));
    return {
        rows: encoded(writable),
        distinct: encoded(writable.check(Schema.makeFilter((entries) => Array.dedupe(entries.map((row) => keyed.map(({ field }) => row[field]))).length === entries.length, { expected: 'rows with distinct keys' }))),
        lists: Record.map(columns, (entry) => encoded(Schema.Array(wire(entry)))),
        one: Schema.decodeUnknownEffect(Schema.NonEmptyArray(schema)),
        many: Schema.decodeUnknownEffect(Schema.Array(schema)),
        changed: Schema.decodeUnknownEffect(Schema.Array(Schema.Struct({ ...schema.fields, inserted: Schema.BooleanFromBit }))),
        found: Schema.decodeUnknownEffect(Schema.Array(Schema.Struct({ ...schema.fields, rank: Schema.Finite, highlight: Schema.OptionFromNullOr(Schema.fromJsonString(Schema.Record(Schema.String, Schema.NullOr(Schema.String)))) }))),
    };
};
const run = <F extends Fields, O extends TableOptions<F>, A, B, E, R>(
    relation: Relation<F, O>,
    decode: (result: unknown) => Effect.Effect<B, Schema.SchemaError>,
    request: Effect.Effect<A, Schema.SchemaError>,
    statement: (context: Effect.Success<ReturnType<typeof statements>>, body: A) => Effect.Effect<unknown, E, R>,
) =>
    Effect.flatMap(
        Effect.flatMap(Effect.zip(statements(relation), request), ([context, body]) => statement(context, body)),
        decode,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Present, Relation, type TableOptions };
