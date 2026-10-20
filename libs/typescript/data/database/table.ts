import { D1Client } from '@effect/sql-d1';
import { Array, Effect, Equal, Filter, Function, flow, Match, Option, Record, Schema, SchemaAST, SchemaRepresentation, SchemaTransformation, String } from 'effect';
import { Hex } from 'effect/encoding';
import { SqlClient, Statement } from 'effect/sql';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fields extends Schema.Struct.Fields {
    readonly [field: PropertyKey]: Schema.Top & { readonly Encoded: string | number | Uint8Array | readonly number[] | null; readonly DecodingServices: never; readonly EncodingServices: never };
}
type Field<F> = keyof F & string;
type Present<F extends Fields> = string extends keyof F ? string : { readonly [K in keyof F]: null extends F[K]['Encoded'] ? never : K }[keyof F] & string;
type Key<F> = Field<F> | { readonly casefold: Field<F> };
interface Options<F extends Fields> {
    readonly key?: Array.NonEmptyReadonlyArray<Present<F>>;
    readonly indexes?: ReadonlyArray<{
        readonly columns: Array.NonEmptyReadonlyArray<Key<F>>;
        readonly unique?: true;
        readonly where?: { readonly [K in Field<F>]?: null | { readonly not: null } | Array.NonEmptyReadonlyArray<string | number> };
        readonly include?: readonly Field<F>[];
        readonly method?: 'gin' | 'gist' | 'brin';
    }>;
    readonly references?: ReadonlyArray<{ readonly columns: Array.NonEmptyReadonlyArray<Field<F>>; readonly table: Relation; readonly onDelete?: keyof typeof actions }>;
    readonly defaults?: { readonly [K in keyof F]?: Exclude<F[K]['Encoded'], Uint8Array | readonly number[]> };
    readonly projections?: { readonly [K in Field<F>]?: { readonly from: Field<F>; readonly path: Array.NonEmptyReadonlyArray<string> } };
    readonly search?: { readonly columns: Array.NonEmptyReadonlyArray<Field<F>>; readonly tokenizer?: 'unicode61' | 'trigram' };
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
    sqlite: { text: 'TEXT', uuid: 'TEXT', json: 'TEXT', instant: 'TEXT', integer: 'INTEGER', int64: 'INTEGER', real: 'REAL', number: 'REAL', boolean: 'INTEGER', blob: 'BLOB' },
    pg: { text: 'text', uuid: 'uuid', json: 'jsonb', instant: 'timestamptz(3)', integer: 'bigint', int64: 'bigint', real: 'double precision', number: 'double precision', boolean: 'boolean', blob: 'bytea' },
} as const;
const actions = { 'NO ACTION': 'a', RESTRICT: 'r', CASCADE: 'c', 'SET NULL': 'n', 'SET DEFAULT': 'd' };

// --- [MODELS] --------------------------------------------------------------------------

const Bytes = Schema.Union([Schema.Uint8Array, Schema.Array(Schema.Int)]).pipe(Schema.decodeTo(Schema.Uint8Array, SchemaTransformation.transform<Uint8Array, Uint8Array | readonly number[]>({ decode: (bytes) => new Uint8Array(bytes), encode: Function.identity })));
const Uuid = Schema.String.check(Schema.isUUID(uuid.version));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [NAMES]
const quote = Statement.defaultEscape('"');
const literal = (text: string) => `'${text.replaceAll("'", "''")}'`;
const document = (columns: readonly string[]) => columns.map((name) => `coalesce(${quote(name)}, '')`).join(` || ' ' || `);

// --- [FACTS]
const column = <F extends Fields>(field: Field<F>, schema: F[Field<F>], { defaults, projections, renamed, retype }: Required<Pick<Options<F>, 'defaults' | 'projections' | 'renamed' | 'retype'>>, retired: readonly string[]) => {
    const flat = (node: SchemaRepresentation.Representation): readonly SchemaRepresentation.Representation[] => (node._tag === 'Union' ? node.types.flatMap(flat) : [node]);
    const walk = (ast: SchemaAST.AST) => Array.partition(flat(SchemaRepresentation.toRepresentation(ast, { referencePolicy: () => undefined }).representation), Filter.tagged<SchemaRepresentation.Representation>()('Null'));
    const [nulls, values] = walk(schema.ast);
    const [, decoded] = walk(SchemaAST.toType(schema.ast));
    const checks = values.flatMap((node) => ('checks' in node ? node.checks.flatMap(({ representation }) => Option.toArray(Option.fromUndefinedOr(representation))) : []));
    const payloads = <P>({ id, payloadSchema }: SchemaRepresentation.FilterReviver<P>) => checks.filter((check) => check.id === id).flatMap(({ payload }) => Option.toArray(Schema.decodeOption(payloadSchema)(payload)));
    const has = flow(payloads, Array.isReadonlyArrayNonEmpty);
    const limits = <P>(reviver: SchemaRepresentation.FilterReviver<P>, length: boolean, ...bounds: ReadonlyArray<(payload: P) => readonly [string, number]>) => payloads(reviver).flatMap((payload) => bounds.map((bound) => bound(payload)).map(([op, value]) => ({ op, value, length })));
    const lower = ({ minimum, exclusiveMinimum }: Pick<typeof SchemaRepresentation.isBetweenReviver.payloadSchema.Type, 'minimum' | 'exclusiveMinimum'>) => [exclusiveMinimum === true ? '>' : '>=', minimum] as const;
    const upper = ({ maximum, exclusiveMaximum }: Pick<typeof SchemaRepresentation.isBetweenReviver.payloadSchema.Type, 'maximum' | 'exclusiveMaximum'>) => [exclusiveMaximum === true ? '<' : '<=', maximum] as const;
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
            limits(SchemaRepresentation.isGreaterThanReviver, false, ({ exclusiveMinimum }) => ['>', exclusiveMinimum]),
            limits(SchemaRepresentation.isGreaterThanOrEqualToReviver, false, lower),
            limits(SchemaRepresentation.isLessThanReviver, false, ({ exclusiveMaximum }) => ['<', exclusiveMaximum]),
            limits(SchemaRepresentation.isLessThanOrEqualToReviver, false, upper),
            limits(SchemaRepresentation.isBetweenReviver, false, lower, upper),
            integer ? limits(SchemaRepresentation.isMultipleOfReviver, false, ({ divisor }) => ['%', divisor]) : [],
            limits(SchemaRepresentation.isMinCodePointsReviver, true, ({ minCodePoints }) => ['>=', minCodePoints]),
            limits(SchemaRepresentation.isMaxCodePointsReviver, true, ({ maxCodePoints }) => ['<=', maxCodePoints]),
            limits(SchemaRepresentation.isBetweenCodePointsReviver, true, lower, upper),
        ].flat(),
        fallback: Option.fromNullishOr(defaults[field]),
        projection: Option.map(Option.fromUndefinedOr(projections[field]), ({ from, path }) => ({ from: names.transformQueryNames(from), path })),
        earlier: Option.map(Option.fromUndefinedOr(renamed[field]), names.transformQueryNames),
        incarnation: retired.filter((entry) => entry === name).length,
        retype: retype.includes(field),
    };
};
const facts = <F extends Fields>(name: string, fields: F, options: Options<F>) => {
    const settled = { key: ['id'], indexes: [], references: [], defaults: {}, projections: {}, renamed: {}, retype: [], retired: [], retiredIndexes: [], ...options };
    const key = Array.map(settled.key, names.transformQueryNames);
    const retired = settled.retired.map(names.transformQueryNames);
    const declared = settled.indexes.map((index) => {
        const { columns, unique, include, where, method } = { unique: false, include: [], method: 'btree', ...index, where: { ...index.where } };
        return {
            keys: Array.map(columns, (entry) => (typeof entry === 'string' ? { column: names.transformQueryNames(entry), casefold: false } : { column: names.transformQueryNames(entry.casefold), casefold: true })),
            unique,
            include: include.map(names.transformQueryNames),
            where: Object.entries(where).flatMap(([field, predicate]) => (predicate === undefined ? [] : [{ column: names.transformQueryNames(field), predicate }])),
            method,
        };
    });
    const linked = settled.references.map((reference) => ({ onDelete: 'NO ACTION' as const, ...reference, columns: Array.map(reference.columns, names.transformQueryNames) }));
    const orders = [key, ...declared.filter(({ method }) => method === 'btree').map((index) => Array.takeWhile(index.keys, ({ casefold }) => !casefold).map(({ column: entry }) => entry))];
    const covering = linked.filter(({ columns }) => !orders.some((order) => Equal.equals(Array.take(order, columns.length), columns)));
    return {
        name: names.transformQueryNames(name),
        earlier: Option.map(Option.fromUndefinedOr(options.renamedFrom), names.transformQueryNames),
        key,
        columns: Record.map<Field<F>, F[Field<F>], Column>(fields, (schema, field) => column(field, schema, settled, retired)),
        indexes: [...declared, ...covering.map(({ columns }) => ({ keys: Array.map(columns, (entry) => ({ column: entry, casefold: false })), unique: false, include: [], where: [], method: 'btree' }))].map((index) => ({
            ...index,
            parts: [...index.keys.map(({ column: entry }) => entry), ...(index.method === 'btree' ? [] : [index.method])],
        })),
        references: linked,
        search: Option.map(Option.fromUndefinedOr(options.search), ({ columns, tokenizer }) => ({ columns: columns.map(names.transformQueryNames), trigram: tokenizer === 'trigram' })),
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
const atomic = (statements: Array.NonEmptyReadonlyArray<Statement.Statement<object>>) =>
    Effect.flatMap(
        Effect.serviceOption(D1Client.D1Client),
        Option.match({
            onSome: (client) => client.batch(statements),
            onNone: () => SqlClient.SqlClient.use((sql) => sql.withTransaction(Effect.all(statements))),
        }),
    );
const encoded = <S extends Schema.Top>(schema: S) => Schema.encodeUnknownEffect(Schema.fromJsonString(Schema.Array(schema), { replacer: (_, value) => (value instanceof Uint8Array ? Hex.encode(value) : value) }));
const table = <const F extends Fields>(name: string, fields: F, options: Options<F> & ('id' extends Present<F> ? unknown : { readonly key: Array.NonEmptyReadonlyArray<Present<F>> })) => {
    const struct = Schema.Struct(fields);
    const relation = facts(name, fields, options);
    const columns = Object.values(relation.columns);
    const written = columns.filter(({ projection }) => Option.isNone(projection));
    const engine = Effect.map(SqlClient.SqlClient, (sql) => {
        const dialect = sql.onDialectOrElse({ pg: () => dialects.pg, orElse: () => dialects.sqlite });
        const element = (entry: Column) => dialect.element(entry, dialect.member('e.value', entry.field));
        const keys = (scope: string) => sql.csv(relation.key.map((part) => `${scope}${quote(part)}`));
        const reads = (scope: string) => sql.csv(columns.map((entry) => dialect.read(entry, scope)));
        const [owner, targets, values, order] = [sql(relation.name), sql.csv(written.map((entry) => quote(entry.name))), sql.csv(written.map(element)), sql.csv(relation.key.flatMap((part) => written.filter((entry) => entry.name === part).map(element)))];
        const ignore = sql`ON CONFLICT (${keys('')}) DO NOTHING`;
        const changed = written.filter((entry) => !relation.key.includes(entry.name)).map((entry) => sql(entry.name));
        const merge = Array.match(changed, {
            onEmpty: () => ignore,
            onNonEmpty: (set) => sql`ON CONFLICT (${keys('')}) DO UPDATE SET ${sql.csv(set.map((part) => sql`${part} = excluded.${part}`))} WHERE (${sql.csv(set.map((part) => sql`${owner}.${part}`))}) IS DISTINCT FROM (${sql.csv(set.map((part) => sql`excluded.${part}`))})`,
        });
        const inserting = (body: string, conflict: Statement.Fragment) =>
            sql.onDialectOrElse({
                pg: () => sql`INSERT INTO ${owner} (${targets}) SELECT ${values} FROM jsonb_array_elements(${body}) e(value) ORDER BY ${order} ${conflict} RETURNING ${reads('')}`,
                orElse: () => sql`INSERT INTO ${owner} (${targets}) SELECT ${values} FROM json_each(${body}) e WHERE true ${conflict} RETURNING ${reads('')}`,
            });
        return { sql, dialect, owner, order, keys, reads, ignore, merge, inserting };
    });
    const write =
        <A>(statement: (context: Effect.Success<typeof engine>, body: string) => A) =>
        (rows: Array.NonEmptyReadonlyArray<Schema.Struct.Type<F>>) =>
            Effect.zipWith(engine, encoded(struct)(rows), statement);
    const insert = write(({ sql, inserting }, body) => inserting(body, sql.literal('')));
    const upsert = write(({ inserting, merge }, body) => inserting(body, merge));
    const ensure = flow(
        write(({ sql, owner, order, keys, reads, inserting, ignore }, body) =>
            sql.onDialectOrElse({
                pg: () => sql`WITH inserted AS (${inserting(body, ignore)}) SELECT * FROM inserted UNION ALL SELECT ${reads('')} FROM ${owner} WHERE (${keys('')}) IN (SELECT ${order} FROM jsonb_array_elements(${body}) e(value))`,
                orElse: () => Effect.map(atomic([inserting(body, ignore), sql`SELECT ${reads('')} FROM ${owner} WHERE (${keys('')}) IN (SELECT ${order} FROM json_each(${body}) e) ORDER BY ${keys('')}`]), Array.lastNonEmpty),
            }),
        ),
        Effect.flatten,
    );
    const matching = Effect.fnUntraced(function* <K extends Field<F>>(field: K, values: Values<F, K>) {
        const { sql, dialect } = yield* engine;
        const entry = relation.columns[field];
        const body = yield* encoded(entry.schema)(values);
        const element = sql.literal(dialect.element(entry, 'e.value'));
        return sql.onDialectOrElse({
            pg: () => sql`${sql(entry.name)} IN (SELECT ${element} FROM jsonb_array_elements_text(${body}) e(value))`,
            orElse: () => sql`${sql(entry.name)} IN (SELECT ${element} FROM json_each(${body}) e)`,
        });
    });
    const select = Effect.fnUntraced(function* <K extends Field<F>>(field: K, values: Values<F, K>) {
        const { sql, owner, keys, reads } = yield* engine;
        const where = yield* matching(field, values);
        return sql`SELECT ${reads('')} FROM ${owner} WHERE ${where} ORDER BY ${keys('')}`;
    });
    const remove = Effect.fnUntraced(function* <K extends Field<F>>(field: K, values: Values<F, K>) {
        const { sql, owner, reads } = yield* engine;
        const where = yield* matching(field, values);
        return sql`DELETE FROM ${owner} WHERE ${where} RETURNING ${reads('')}`;
    });
    const latest = Effect.fnUntraced(function* <G extends Field<F>>(group: G, order: Field<F>, groups: Values<F, G>) {
        const { sql, dialect, owner, keys, reads } = yield* engine;
        const by = relation.columns[group];
        const body = yield* encoded(by.schema)(groups);
        const newest = (selected: Statement.Fragment) => sql`SELECT ${selected} FROM ${owner} i WHERE i.${sql(by.name)} = ${sql.literal(dialect.element(by, 'e.value'))} ORDER BY ${sql.csv([relation.columns[order].name, ...relation.key].map((part) => sql`i.${sql(part)} DESC`))} LIMIT 1`;
        return sql.onDialectOrElse({
            pg: () => sql`SELECT ${reads('l.')} FROM jsonb_array_elements_text(${body}) e(value) CROSS JOIN LATERAL (${newest(sql.literal('*'))}) l`,
            orElse: () => sql`SELECT ${reads('t.')} FROM json_each(${body}) e JOIN ${owner} t ON (${keys('t.')}) = (${newest(keys('i.'))})`,
        });
    });
    const search = Effect.fnUntraced(function* (text: string) {
        const { sql, owner, keys, reads } = yield* engine;
        const { columns: searched, trigram } = yield* Effect.fromOption(relation.search);
        return sql.onDialectOrElse({
            pg: () => {
                const body = sql.literal(document(searched));
                return trigram
                    ? sql`SELECT ${reads('')} FROM ${owner} WHERE (${body}) ILIKE ${`%${text.replaceAll(patterns.like, (character) => `\\${character}`)}%`} ORDER BY ${keys('')}`
                    : sql`SELECT ${reads('')} FROM ${owner} CROSS JOIN plainto_tsquery('simple', ${text}) q WHERE to_tsvector('simple', ${body}) @@ q ORDER BY ts_rank(to_tsvector('simple', ${body}), q) DESC`;
            },
            orElse: () => {
                const virtual = sql(`${relation.name}_fts`);
                const terms = (trigram ? [text] : text.split(patterns.words).filter((word) => word !== '')).map((term) => `"${term.replaceAll('"', '""')}"`).join(' ');
                return sql`SELECT ${reads(`${quote(relation.name)}.`)} FROM ${virtual} JOIN ${owner} ON ${owner}."rowid" = ${virtual}."rowid" WHERE ${virtual} MATCH ${terms} ORDER BY ${virtual}."rank"`;
            },
        });
    });
    return Object.assign(struct, { table: relation, insert, upsert, ensure, select, remove, latest, search });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { actions, atomic, Bytes, type Catalog, type Column, document, literal, names, quote, type Relation, table, types, Uuid };
