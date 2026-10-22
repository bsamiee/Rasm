import { D1Client } from '@effect/sql-d1';
import { Array, Effect, Equal, Filter, Function, flow, Hash, Match, Option, Record, Schema, SchemaAST, SchemaRepresentation, SchemaTransformation, String, Struct } from 'effect';
import { Hex } from 'effect/encoding';
import { SqlClient, Statement } from 'effect/sql';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fields extends Schema.Struct.Fields {
    readonly [field: PropertyKey]: Schema.ConstraintCodec<unknown, string | number | Uint8Array | readonly number[] | null>;
}
type Field<F> = keyof F & string;
type Present<F extends Fields> = string extends keyof F ? string : { readonly [K in keyof F]: null extends F[K]['Encoded'] ? never : K }[keyof F] & string;
type Where<F> = { readonly [K in Field<F>]?: null | Array.NonEmptyReadonlyArray<string | number> | { readonly not: null | Array.NonEmptyReadonlyArray<string | number> } };
interface Options<F extends Fields> {
    readonly key?: Array.NonEmptyReadonlyArray<Present<F>>;
    readonly indexes?: ReadonlyArray<{
        readonly columns: Array.NonEmptyReadonlyArray<Field<F> | { readonly casefold: Field<F> }>;
        readonly unique?: true;
        readonly where?: Where<F>;
        readonly include?: readonly Field<F>[];
        readonly method?: 'gin' | 'gist' | 'brin';
    }>;
    readonly references?: ReadonlyArray<{ readonly columns: Array.NonEmptyReadonlyArray<Field<F>>; readonly table: Relation; readonly onDelete?: keyof typeof actions }>;
    readonly defaults?: { readonly [K in keyof F]?: Exclude<F[K]['Encoded'], Uint8Array | readonly number[]> };
    readonly projections?: { readonly [K in Field<F>]?: { readonly from: Field<F>; readonly path: Array.NonEmptyReadonlyArray<string> } | { readonly where: Where<F> } };
    readonly search?: { readonly columns: Array.NonEmptyReadonlyArray<Field<F>>; readonly tokenizer: 'trigram' | { readonly configuration: string; readonly filters?: readonly string[] } };
    readonly withoutRowid?: true;
    readonly tenant?: Present<F>;
    readonly renamed?: { readonly [K in Field<F>]?: string };
    readonly renamedFrom?: string;
    readonly retype?: readonly Field<F>[];
    readonly retired?: readonly string[];
    readonly retiredIndexes?: readonly Array.NonEmptyReadonlyArray<string>[];
}
type Column = ReturnType<typeof column>;
type Values<F extends Fields, K extends Field<F>> = Array.NonEmptyReadonlyArray<F[K]['Type']>;
interface Relation<F extends Fields = Fields> extends Schema.Struct<F> {
    readonly table: ReturnType<typeof facts<F>>;
}
interface Catalog {
    readonly tables: Array.NonEmptyReadonlyArray<Relation>;
    readonly retired: readonly string[];
    readonly reader?: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const names = { transformQueryNames: String.camelToSnake, transformResultNames: String.snakeToCamel, transformJson: false };
const uuid = { version: 7 } as const;
const patterns = { words: /\s+/u, like: /[\\%_]/gu };
const types = {
    sqlite: { text: 'TEXT', uuid: 'TEXT', json: 'TEXT', instant: 'TEXT', integer: 'INTEGER', int64: 'INTEGER', decimal: 'TEXT', real: 'REAL', number: 'REAL', boolean: 'INTEGER', blob: 'BLOB' },
    pg: { text: 'text', uuid: 'uuid', json: 'jsonb', instant: 'timestamptz(3)', integer: 'bigint', int64: 'bigint', decimal: 'numeric', real: 'double precision', number: 'double precision', boolean: 'boolean', blob: 'bytea' },
} as const;
const actions = { 'NO ACTION': 'a', RESTRICT: 'r', CASCADE: 'c', 'SET NULL': 'n', 'SET DEFAULT': 'd' };

// --- [MODELS] --------------------------------------------------------------------------

const Bytes = Schema.Union([Schema.Uint8Array, Schema.Array(Schema.Int)]).pipe(Schema.decodeTo(Schema.Uint8Array, SchemaTransformation.transform<Uint8Array, Uint8Array | readonly number[]>({ decode: (bytes) => new Uint8Array(bytes), encode: Function.identity })));
const Uuid = Schema.String.check(Schema.isUUID(uuid.version), Schema.isLowercased());

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [NAMES]
const quote = Statement.defaultEscape('"');
const literal = (text: string) => `'${text.replaceAll("'", "''")}'`;
const identifier = (parts: readonly string[]) => {
    const name = parts.map(names.transformQueryNames).join('_');
    const limit = 63;
    const radix = 36;
    const digest = Math.abs(Hash.string(name)).toString(radix);
    const encoder = new TextEncoder();
    return encoder.encode(name).length <= limit ? name : `${name.slice(0, encoder.encodeInto(name, new Uint8Array(limit - digest.length - 1)).read)}_${digest}`;
};
const document = ({ columns, tokenizer }: Option.Option.Value<Relation['table']['search']>) => {
    const joined = columns.map(({ name }) => `coalesce(${quote(name)}, '')`).join(` || ' ' || `);
    return tokenizer === 'trigram' ? joined : `to_tsvector(${literal(tokenizer.name)}, ${joined})`;
};

// --- [FACTS]
const conditions = <F extends Fields>(where: Where<F>) => Object.entries(where).flatMap(([field, predicate]) => (predicate === undefined ? [] : [{ column: names.transformQueryNames(field), predicate }]));
const column = <F extends Fields>(field: Field<F>, schema: F[Field<F>], { defaults, projections, renamed, retype }: Options<F>, retired: readonly string[]) => {
    const flat = (node: SchemaRepresentation.Representation): readonly SchemaRepresentation.Representation[] => (node._tag === 'Union' ? node.types.flatMap(flat) : [node]);
    const walk = (ast: SchemaAST.AST) => Array.partition(flat(SchemaRepresentation.toRepresentation(ast, { referencePolicy: () => undefined }).representation), Filter.tagged<SchemaRepresentation.Representation>()('Null'));
    const [nulls, values] = walk(schema.ast);
    const [, decoded] = walk(SchemaAST.toType(schema.ast));
    const leaves = (check: SchemaRepresentation.Check): readonly SchemaRepresentation.Check[] => (check._tag === 'FilterGroup' ? check.checks.flatMap(leaves) : [check]);
    const payloads =
        (nodes: readonly SchemaRepresentation.Representation[]) =>
        <P>({ id, payloadSchema }: SchemaRepresentation.FilterReviver<P>) =>
            Array.flatMapNullishOr(
                nodes.flatMap((node) => ('checks' in node ? node.checks.flatMap(leaves) : [])),
                Struct.get('representation'),
            ).flatMap((check) => (check.id === id ? Option.toArray(Schema.decodeOption(Schema.toCodecJson(payloadSchema))(check.payload)) : []));
    const [stored, typed] = [payloads(values), payloads(decoded)];
    const has = flow(stored, Array.isReadonlyArrayNonEmpty);
    const limits = <P>(read: typeof stored, reviver: SchemaRepresentation.FilterReviver<P>, length: boolean, ...bounds: ReadonlyArray<(payload: P) => readonly [string, number | bigint]>) => read(reviver).flatMap((payload) => bounds.map((bound) => bound(payload)).map(([op, value]) => ({ op, value, length })));
    const lower = ({ minimum, exclusiveMinimum }: Pick<typeof SchemaRepresentation.isBetweenReviver.payloadSchema.Type | typeof SchemaRepresentation.isBetweenBigIntReviver.payloadSchema.Type, 'minimum' | 'exclusiveMinimum'>) => [exclusiveMinimum === true ? '>' : '>=', minimum] as const;
    const upper = ({ maximum, exclusiveMaximum }: Pick<typeof SchemaRepresentation.isBetweenReviver.payloadSchema.Type | typeof SchemaRepresentation.isBetweenBigIntReviver.payloadSchema.Type, 'maximum' | 'exclusiveMaximum'>) => [exclusiveMaximum === true ? '<' : '<=', maximum] as const;
    const boolean = decoded.some(({ _tag }) => _tag === 'Boolean');
    const literals = values.flatMap((node) =>
        Match.value(node).pipe(
            Match.tag('Literal', ({ literal: member }) => (boolean ? [] : [member])),
            Match.tag('Enum', ({ enums }) => enums.map(([, member]) => member)),
            Match.orElse(() => []),
        ),
    );
    const numeric = Array.isReadonlyArrayNonEmpty(literals) && literals.every((member) => typeof member === 'number');
    const integer = has(SchemaRepresentation.isIntReviver);
    const name = names.transformQueryNames(field);
    return {
        field,
        name,
        schema,
        kind: Match.value({ node: values[0], decoded: decoded[0], boolean, numeric, integer: integer || (numeric && literals.every(Number.isInteger)), finite: has(SchemaRepresentation.isFiniteReviver), uuid: has(SchemaRepresentation.isUUIDReviver) }).pipe(
            Match.withReturnType<keyof typeof types.sqlite>(),
            Match.when({ node: { _tag: 'String', annotations: { contentMediaType: 'application/json' } } }, () => 'json'),
            Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'BigInt' } }, () => 'int64'),
            Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'Declaration', representation: { id: Match.is(SchemaRepresentation.DateReviver.id, SchemaRepresentation.DateTimeUtcReviver.id) } } }, () => 'instant'),
            Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'Declaration', representation: { id: SchemaRepresentation.BigDecimalReviver.id } } }, () => 'decimal'),
            Match.when({ node: { _tag: 'String' }, uuid: true }, () => 'uuid'),
            Match.when({ boolean: true }, () => 'boolean'),
            Match.whenOr({ node: { _tag: 'Number' }, integer: true }, { numeric: true, integer: true }, () => 'integer'),
            Match.whenOr({ node: { _tag: 'Number' }, finite: true }, { numeric: true }, () => 'real'),
            Match.when({ node: { _tag: 'Number' } }, () => 'number'),
            Match.whenOr({ node: { _tag: 'Declaration' } }, { node: { _tag: 'Arrays' } }, () => 'blob'),
            Match.orElse(() => 'text'),
        ),
        nullable: Array.isReadonlyArrayNonEmpty(nulls),
        literals,
        bounds: [
            limits(stored, SchemaRepresentation.isGreaterThanReviver, false, ({ exclusiveMinimum }) => ['>', exclusiveMinimum]),
            limits(stored, SchemaRepresentation.isGreaterThanOrEqualToReviver, false, lower),
            limits(stored, SchemaRepresentation.isLessThanReviver, false, ({ exclusiveMaximum }) => ['<', exclusiveMaximum]),
            limits(stored, SchemaRepresentation.isLessThanOrEqualToReviver, false, upper),
            limits(stored, SchemaRepresentation.isBetweenReviver, false, lower, upper),
            integer ? limits(stored, SchemaRepresentation.isMultipleOfReviver, false, ({ divisor }) => ['%', divisor]) : [],
            limits(stored, SchemaRepresentation.isMinCodePointsReviver, true, ({ minCodePoints }) => ['>=', minCodePoints]),
            limits(stored, SchemaRepresentation.isMaxCodePointsReviver, true, ({ maxCodePoints }) => ['<=', maxCodePoints]),
            limits(stored, SchemaRepresentation.isBetweenCodePointsReviver, true, lower, upper),
            limits(typed, SchemaRepresentation.isGreaterThanBigIntReviver, false, ({ exclusiveMinimum }) => ['>', exclusiveMinimum]),
            limits(typed, SchemaRepresentation.isGreaterThanOrEqualToBigIntReviver, false, lower),
            limits(typed, SchemaRepresentation.isLessThanBigIntReviver, false, ({ exclusiveMaximum }) => ['<', exclusiveMaximum]),
            limits(typed, SchemaRepresentation.isLessThanOrEqualToBigIntReviver, false, upper),
            limits(typed, SchemaRepresentation.isBetweenBigIntReviver, false, lower, upper),
        ].flat(),
        fallback: Option.fromNullishOr(defaults?.[field]),
        projection: Option.map(Option.fromUndefinedOr(projections?.[field]), (projection) => ('where' in projection ? { _tag: 'Where' as const, where: conditions(projection.where) } : { _tag: 'Path' as const, from: names.transformQueryNames(projection.from), path: projection.path })),
        earlier: Option.map(Option.fromUndefinedOr(renamed?.[field]), names.transformQueryNames),
        incarnation: Array.countBy(retired, Equal.equals(name)),
        retype: Option.exists(Option.fromUndefinedOr(retype), Array.contains(field)),
    };
};
const facts = <F extends Fields>(name: string, fields: F, options: Options<F>) => {
    const settled = { key: ['id'], indexes: [], references: [], retired: [], retiredIndexes: [], ...options };
    const key = Array.map(settled.key, names.transformQueryNames);
    const retired = settled.retired.map(names.transformQueryNames);
    const declared = settled.indexes.map((index) => {
        const { columns, unique, include, where, method } = { unique: false, include: [], method: 'btree', where: {}, ...index };
        return {
            keys: Array.map(columns, (entry) => (typeof entry === 'string' ? { column: names.transformQueryNames(entry), casefold: false } : { column: names.transformQueryNames(entry.casefold), casefold: true })),
            unique,
            include: include.map(names.transformQueryNames),
            where: conditions(where),
            method,
        };
    });
    const linked = settled.references.map((reference) => ({ onDelete: 'NO ACTION' as const, ...reference, columns: Array.map(reference.columns, names.transformQueryNames) }));
    const orders = [key, ...declared.filter(({ method }) => method === 'btree').map((index) => Array.takeWhile(index.keys, ({ casefold }) => !casefold).map(({ column: entry }) => entry))];
    const covering = linked.filter(({ columns }) => !orders.some((order) => Equal.equals(Array.take(order, columns.length), columns)));
    const described = Record.map<Field<F>, F[Field<F>], Column>(fields, (schema, field) => column(field, schema, options, retired));
    return {
        name: names.transformQueryNames(name),
        earlier: Option.map(Option.fromUndefinedOr(options.renamedFrom), names.transformQueryNames),
        key,
        columns: described,
        indexes: [...declared, ...covering.map(({ columns }) => ({ keys: Array.map(columns, (entry) => ({ column: entry, casefold: false })), unique: false, include: [], where: [], method: 'btree' }))].map((index) => ({
            ...index,
            parts: [...index.keys.map(({ column: entry }) => entry), ...(index.method === 'btree' ? [] : [index.method])],
        })),
        references: linked,
        search: Option.map(Option.fromUndefinedOr(options.search), ({ columns, tokenizer }) => ({
            columns: Array.map(columns, (field): Column => described[field]),
            tokenizer: Match.value(tokenizer).pipe(
                Match.when('trigram', () => 'trigram' as const),
                Match.orElse(({ configuration, filters = [] }) => ({ name: identifier([configuration, ...filters]), configuration, filters })),
            ),
        })),
        withoutRowid: options.withoutRowid === true,
        tenant: Option.map(Option.fromUndefinedOr(options.tenant), names.transformQueryNames),
        retired: { columns: retired, indexes: settled.retiredIndexes.map((columns) => columns.map(names.transformQueryNames)) },
    };
};

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
        element: ({ kind }: Column, text: string) => (kind === 'blob' ? `decode(${text}, 'hex')` : `(${text})::${types.pg[kind]}`),
    },
};

// --- [STATEMENTS]
const atomic = <A extends object>(statements: Array.NonEmptyReadonlyArray<Statement.Statement<A>>) =>
    Effect.flatMap(
        Effect.serviceOption(D1Client.D1Client),
        Option.match({
            onSome: (client) => client.batch(statements),
            onNone: () => SqlClient.SqlClient.use((sql) => sql.withTransaction(Effect.all(statements))),
        }),
    );
const encoded = <S extends Schema.Constraint>(schema: S) => Schema.encodeUnknownEffect(Schema.fromJsonString(schema, { replacer: (_, value) => (value instanceof Uint8Array ? Hex.encode(value) : value) }));
const table = <const F extends Fields, const O extends Options<F>>(
    name: string,
    fields: F,
    options: O & ('id' extends Present<F> ? unknown : { readonly key: Array.NonEmptyReadonlyArray<Present<F>> }) & ('inserted' extends keyof F ? never : unknown) & (Extract<keyof F, 'rank' | 'highlight'> extends never ? unknown : { readonly search?: never }),
) => {
    const struct = Schema.Struct(fields);
    const relation = facts(name, fields, options);
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
                        const virtual = `${relation.name}_fts`;
                        const terms = (tokenizer === 'trigram' ? [text] : text.trim().split(patterns.words)).map((term) => `"${term.replaceAll('"', '""')}"`).join(' ');
                        const marked = Option.match(highlight, { onNone: () => sql.literal('NULL'), onSome: ({ open, close }) => sql`json_object(${sql.csv(searched.flatMap((entry, index) => [literal(entry.field), sql`highlight(${sql(virtual)}, ${sql.literal(`${index}`)}, ${open}, ${close})`]))})` });
                        const gram = 3;
                        const filter = tokenizer === 'trigram' && [...text].length < gram ? sql.or(searched.map(({ name: part }) => sql`${sql(`${virtual}.${part}`)} LIKE ${pattern} ESCAPE '\\'`)) : sql`${sql(virtual)} MATCH ${terms}`;
                        return sql`SELECT ${reads(prefix)}, -${sql(virtual)}."rank" AS "rank", ${marked} AS "highlight" FROM ${sql(virtual)} JOIN ${owner} ON ${owner}."rowid" = ${sql(virtual)}."rowid" WHERE ${filter} ORDER BY ${sql(virtual)}."rank", ${keys(prefix)} LIMIT ${count}`;
                    },
                });
            }),
        );
    return Object.assign(struct, { table: relation, insert, upsert, ensure, select, remove, latest, search });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { actions, atomic, Bytes, type Catalog, type Column, document, identifier, literal, names, quote, type Relation, table, types, Uuid };
