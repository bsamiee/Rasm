import { Array, Effect, Option, Record, Schema } from 'effect';
import { Hex } from 'effect/encoding';
import { SqlClient, type Statement } from 'effect/sql';
import { type Column, dataTypes, type Field, type Fields } from '#/column';
import { definition, document, ftsTable, type Present, type TableOptions } from '#/definition';
import { literal, quote } from '#/lexical';
import { atomic } from '#/transaction';

// --- [TYPES] ---------------------------------------------------------------------------

type Values<F extends Fields, K extends Field<F>> = Array.NonEmptyReadonlyArray<F[K]['Type']>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const patterns = { words: /\s+/u, like: /[\\%_]/gu };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DIALECTS]
const dialects = {
    sqlite: {
        read: ({ name, kind }: Column, scope: string) => (kind === 'int64' ? `CAST(${scope}${quote(name)} AS TEXT) AS ${quote(name)}` : `${scope}${quote(name)}`),
        member: (scope: string, field: string) => `${scope} ->> ${literal(`$.${quote(field)}`)}`,
        element: ({ kind }: Column, text: string) => (kind === 'blob' ? `unhex(${text})` : text),
    },
    pg: {
        read: ({ name, kind }: Column, scope: string) => (kind === 'int64' ? `${scope}${quote(name)}::text AS ${quote(name)}` : `${scope}${quote(name)}`),
        member: (scope: string, field: string) => `${scope} ->> ${literal(field)}`,
        element: ({ kind }: Column, text: string) => (kind === 'blob' ? `decode(${text}, 'hex')` : `(${text})::${dataTypes.pg[kind]}`),
    },
};

// --- [STATEMENTS]
const encoded = <S extends Schema.Constraint>(schema: S) => Schema.encodeUnknownEffect(Schema.fromJsonString(schema, { replacer: (_, value) => (value instanceof Uint8Array ? Hex.encode(value) : value) }));
const table = <const F extends Fields, const O extends TableOptions<F>>(
    name: string,
    fields: F,
    options: O & ('id' extends Present<F> ? unknown : { readonly key: Array.NonEmptyReadonlyArray<Present<F>> }) & ('inserted' extends keyof F ? never : unknown) & (Extract<keyof F, 'rank' | 'highlight'> extends never ? unknown : { readonly search?: never }),
) => {
    const struct = Schema.Struct(fields);
    const relation = definition(name, fields, options);
    const columns = Object.values(relation.columns);
    const written = columns.filter(({ projection }) => Option.isNone(projection));
    const engine = Effect.map(SqlClient.SqlClient, (sql) => {
        const dialect = sql.onDialectOrElse({ pg: () => dialects.pg, orElse: () => dialects.sqlite });
        const element = (entry: Column) => dialect.element(entry, dialect.member('e.value', entry.field));
        const keys = (scope: string) => sql.csv(relation.key.map((part) => `${scope}${quote(part)}`));
        const reads = (scope: string, ...extra: readonly string[]) => sql.csv([...columns.map((entry) => dialect.read(entry, scope)), ...extra]);
        const [owner, prefix, targets, values, order] = [sql(relation.name), `${quote(relation.name)}.`, sql.csv(written.map((entry) => quote(entry.name))), sql.csv(written.map(element)), sql.csv(relation.key.flatMap((part) => written.filter((entry) => entry.name === part).map(element)))];
        const ignore = sql`ON CONFLICT (${keys('')}) DO NOTHING`;
        const merge = (set: Array.NonEmptyReadonlyArray<Column>) => {
            const parts = set.map((entry) => sql(entry.name));
            return sql`ON CONFLICT (${keys('')}) DO UPDATE SET ${sql.csv(parts.map((part) => sql`${part} = excluded.${part}`))} WHERE (${sql.csv(parts.map((part) => sql`${owner}.${part}`))}) IS DISTINCT FROM (${sql.csv(parts.map((part) => sql`excluded.${part}`))})`;
        };
        const inserting = (body: string, conflict: Statement.Fragment, returning: Statement.Fragment) =>
            sql.onDialectOrElse({
                pg: () => sql`INSERT INTO ${owner} (${targets}) SELECT ${values} FROM jsonb_array_elements(${body}) e(value) ORDER BY ${order} ${conflict} RETURNING ${returning}`,
                orElse: () => sql`INSERT INTO ${owner} (${targets}) SELECT ${values} FROM json_each(${body}) e WHERE true ${conflict} RETURNING ${returning}`,
            });
        const updating = (body: string, set: Array.NonEmptyReadonlyArray<Column>) => {
            const [targeted, qualified] = [set.map(({ name: part }) => quote(part)), set.map(({ name: part }) => `${prefix}${quote(part)}`)];
            return sql`UPDATE ${owner} SET (${sql.csv(targeted)}) = (${sql.csv(set.map(element))}) FROM json_each(${body}) e WHERE (${keys(prefix)}) = (${order}) AND (${sql.csv(qualified)}) IS DISTINCT FROM (${sql.csv(set.map(element))}) RETURNING ${reads('', '0 AS inserted')}`;
        };
        const matching = (entry: Column, body: string) => {
            const value = sql.literal(dialect.element(entry, 'e.value'));
            return sql.onDialectOrElse({
                pg: () => sql`${sql(entry.name)} IN (SELECT ${value} FROM jsonb_array_elements_text(${body}) e(value))`,
                orElse: () => sql`${sql(entry.name)} IN (SELECT ${value} FROM json_each(${body}) e)`,
            });
        };
        return { sql, dialect, owner, prefix, order, keys, reads, ignore, merge, inserting, updating, matching };
    });
    const [writable, keyed] = [Schema.Array(Schema.Struct(Record.fromIterableWith(written, ({ field, schema }) => [field, schema]))), written.filter(({ name: part }) => relation.key.includes(part))];
    const [rows, lists, bound] = [encoded(writable), Record.map(relation.columns, ({ schema }) => encoded(Schema.Array(schema))), Schema.encodeEffect(Schema.Natural)];
    const distinct = encoded(writable.check(Schema.makeFilter((entries) => Array.dedupe(entries.map((row) => keyed.map(({ field }) => row[field]))).length === entries.length, { expected: 'rows with distinct keys' })));
    const [one, many, changed, found] = [
        Schema.decodeUnknownEffect(Schema.NonEmptyArray(struct)),
        Schema.decodeUnknownEffect(Schema.Array(struct)),
        Schema.decodeUnknownEffect(Schema.Array(Schema.Struct({ ...fields, inserted: Schema.BooleanFromBit }))),
        Schema.decodeUnknownEffect(Schema.Array(Schema.Struct({ ...fields, rank: Schema.Finite, highlight: Schema.OptionFromNullOr(Schema.fromJsonString(Schema.Record(Schema.String, Schema.NullOr(Schema.String)))) }))),
    ];
    const run = <A, B, E, R>(decode: (result: unknown) => Effect.Effect<B, Schema.SchemaError>, request: Effect.Effect<A, Schema.SchemaError>, statement: (context: Effect.Success<typeof engine>, body: A) => Effect.Effect<unknown, E, R>) =>
        Effect.flatMap(
            Effect.flatMap(Effect.zip(engine, request), ([context, body]) => statement(context, body)),
            decode,
        );
    const write =
        <B, E, R>(decode: (result: unknown) => Effect.Effect<B, Schema.SchemaError>, encode: typeof rows, statement: (context: Effect.Success<typeof engine>, body: string) => Effect.Effect<unknown, E, R>) =>
        (values: Array.NonEmptyReadonlyArray<Schema.Struct.Type<Omit<F, keyof O['projections']>>>) =>
            run(decode, encode(values), statement);
    const insert = write(one, rows, ({ sql, reads, inserting }, body) => inserting(body, sql.literal(''), reads('')));
    const upsert = (values: Array.NonEmptyReadonlyArray<Schema.Struct.Type<Omit<F, keyof O['projections']>>>, ...kept: readonly Field<F>[]) => {
        const set = written.filter(({ field, name: part }) => !(relation.key.includes(part) || kept.includes(field)));
        return write(changed, distinct, ({ sql, reads, inserting, updating, ignore, merge }, body) =>
            sql.onDialectOrElse({
                pg: () => inserting(body, Array.match(set, { onEmpty: () => ignore, onNonEmpty: merge }), sql.csv('WITH (OLD AS inserted)', [reads(''), 'inserted IS NULL AS inserted'])),
                orElse: () => Effect.map(atomic([inserting(body, ignore, reads('', '1 AS inserted')), ...(Array.isReadonlyArrayNonEmpty(set) ? [updating(body, set)] : [])]), Array.flatten),
            }),
        )(values);
    };
    const ensure = write(one, rows, ({ sql, owner, prefix, order, keys, reads, inserting, ignore }, body) =>
        sql.onDialectOrElse({
            pg: () => sql`WITH inserted AS (${inserting(body, ignore, sql.literal('*'))}) SELECT ${reads('t.')} FROM (SELECT * FROM inserted UNION ALL SELECT * FROM ${owner} WHERE (${keys('')}) IN (SELECT ${order} FROM jsonb_array_elements(${body}) e(value))) t ORDER BY ${keys('t.')}`,
            orElse: () => Effect.map(atomic([inserting(body, ignore, reads('')), sql`SELECT ${reads('')} FROM ${owner} WHERE (${keys('')}) IN (SELECT ${order} FROM json_each(${body}) e) ORDER BY ${keys(prefix)}`]), Array.lastNonEmpty),
        }),
    );
    const select = <K extends Field<F>>(field: K, values: Values<F, K>, ...order: ReadonlyArray<Field<F> | { readonly descending: Field<F> }>) =>
        run(many, lists[field](values), ({ sql, owner, prefix, keys, reads, matching }, body) => {
            const ordered = order.map((entry) => (typeof entry === 'string' ? `${prefix}${quote(relation.columns[entry].name)}` : `${prefix}${quote(relation.columns[entry.descending].name)} DESC`));
            return sql`SELECT ${reads('')} FROM ${owner} WHERE ${matching(relation.columns[field], body)} ORDER BY ${sql.csv([...ordered, keys(prefix)])}`;
        });
    const remove = <K extends Field<F>>(field: K, values: Values<F, K>) => run(many, lists[field](values), ({ sql, owner, reads, matching }, body) => sql`DELETE FROM ${owner} WHERE ${matching(relation.columns[field], body)} RETURNING ${reads('')}`);
    const latest = <G extends Field<F>>(group: G, order: Array.NonEmptyReadonlyArray<Field<F>>, groups: Values<F, G>, limit: number) =>
        run(many, Effect.all([lists[group](groups), bound(limit)]), ({ sql, dialect, owner, keys, reads }, [body, count]) => {
            const by = relation.columns[group];
            const ranked = (scope: string) => sql.csv([...order.map((field) => relation.columns[field].name), ...relation.key].map((part) => `${scope}${quote(part)} DESC`));
            const newest = (selected: Statement.Fragment) => sql`SELECT ${selected} FROM ${owner} i WHERE i.${sql(by.name)} = ${sql.literal(dialect.element(by, 'e.value'))} ORDER BY ${ranked('i.')} LIMIT ${count}`;
            return sql.onDialectOrElse({
                pg: () => sql`SELECT ${reads('l.')} FROM jsonb_array_elements_text(${body}) WITH ORDINALITY e(value, position) CROSS JOIN LATERAL (${newest(sql.literal('*'))}) l ORDER BY e.position, ${ranked('l.')}`,
                orElse: () => sql`SELECT ${reads('t.')} FROM json_each(${body}) e JOIN ${owner} t ON (${keys('t.')}) IN (${newest(keys('i.'))}) ORDER BY e.key, ${ranked('t.')}`,
            });
        });
    const search = (text: string, limit: number, marks?: { readonly open: string; readonly close: string }) =>
        Effect.flatMap(Effect.fromOption(relation.search), (searchable) =>
            run(found, bound(limit), ({ sql, owner, prefix, keys, reads }, count) => {
                const { columns: searched, tokenizer } = searchable;
                const [highlight, pattern] = [Option.fromUndefinedOr(marks), `%${text.replaceAll(patterns.like, (character) => `\\${character}`)}%`];
                return sql.onDialectOrElse({
                    pg: () => {
                        const body = sql.literal(document(searchable));
                        const marked = (mark: (name: string, open: string, close: string) => Statement.Fragment) =>
                            Option.match(highlight, { onNone: () => sql.literal('NULL'), onSome: ({ open, close }) => sql`jsonb_build_object(${sql.csv(searched.flatMap((entry) => [literal(entry.field), mark(entry.name, open, close)]))})` });
                        return tokenizer === 'trigram'
                            ? sql`SELECT ${reads('')}, word_similarity(${text}, ${body}) AS rank, ${marked((part, open, close) => sql`regexp_replace(${sql(part)}, ${`***=${text}`}, ${`${open.replaceAll('\\', '\\\\')}\\&${close.replaceAll('\\', '\\\\')}`}, 'gi')`)} AS highlight FROM ${owner} WHERE (${body}) ILIKE ${pattern} ORDER BY rank DESC, ${keys(prefix)} LIMIT ${count}`
                            : sql`SELECT ${reads('')}, ts_rank(${body}, q) AS rank, ${marked((part, open, close) => sql`ts_headline(${sql.literal(literal(tokenizer.name))}, ${sql(part)}, q, ${`StartSel="${open.replaceAll('"', '""')}", StopSel="${close.replaceAll('"', '""')}", HighlightAll=true`})`)} AS highlight FROM ${owner} CROSS JOIN plainto_tsquery(${sql.literal(literal(tokenizer.name))}, ${text}) q WHERE ${body} @@ q ORDER BY rank DESC, ${keys(prefix)} LIMIT ${count}`;
                    },
                    orElse: () => {
                        const virtual = ftsTable(relation.name);
                        const terms = (tokenizer === 'trigram' ? [text] : text.trim().split(patterns.words)).map((term) => `"${term.replaceAll('"', '""')}"`).join(' ');
                        const marked = Option.match(highlight, { onNone: () => sql.literal('NULL'), onSome: ({ open, close }) => sql`json_object(${sql.csv(searched.flatMap((entry, index) => [literal(entry.field), sql`highlight(${sql(virtual)}, ${sql.literal(`${index}`)}, ${open}, ${close})`]))})` });
                        const gram = 3;
                        const filter = tokenizer === 'trigram' && [...text].length < gram ? sql.or(searched.map(({ name: part }) => sql`${sql(`${virtual}.${part}`)} LIKE ${pattern} ESCAPE '\\'`)) : sql`${sql(virtual)} MATCH ${terms}`;
                        return sql`SELECT ${reads(prefix)}, -${sql(virtual)}."rank" AS "rank", ${marked} AS "highlight" FROM ${sql(virtual)} JOIN ${owner} ON ${owner}."rowid" = ${sql(virtual)}."rowid" WHERE ${filter} ORDER BY ${sql(virtual)}."rank", ${keys(prefix)} LIMIT ${count}`;
                    },
                });
            }),
        );
    return Object.assign(struct, { definition: relation, insert, upsert, ensure, select, remove, latest, search });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { table };
