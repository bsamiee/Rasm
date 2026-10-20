import { D1Client } from '@effect/sql-d1';
import { SqliteClient } from '@effect/sql-sqlite-do';
import { Array, type Cause, Effect, Function, Hash, Match, Option, Predicate, Record, Schema, SchemaAST, SchemaRepresentation, SchemaTransformation, String } from 'effect';
import { Hex } from 'effect/encoding';
import { SqlClient, type SqlConnection, type SqlError, Statement } from 'effect/sql';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fields extends Schema.Struct.Fields {
    readonly [field: PropertyKey]: Schema.Top & { readonly Encoded: string | number | Uint8Array | readonly number[] | null; readonly DecodingServices: never; readonly EncodingServices: never };
}
type Field<F> = keyof F & string;
type Present<F extends Fields> = string extends keyof F ? string : { readonly [K in keyof F]: null extends F[K]['Encoded'] ? never : K }[keyof F] & string;
type Key<F> = Field<F> | { readonly casefold: Field<F> };
type Rule = null | { readonly not: null } | Array.NonEmptyReadonlyArray<string | number>;
interface Index<F> {
    readonly columns: Array.NonEmptyReadonlyArray<Key<F>>;
    readonly unique?: true;
    readonly where?: { readonly [K in Field<F>]?: Rule };
    readonly include?: readonly Field<F>[];
    readonly method?: 'gin' | 'gist' | 'brin';
}
interface Options<F extends Fields> {
    readonly key?: Array.NonEmptyReadonlyArray<Present<F>>;
    readonly indexes?: readonly Index<F>[];
    readonly references?: ReadonlyArray<{ readonly columns: Array.NonEmptyReadonlyArray<Field<F>>; readonly table: Relation; readonly onDelete?: 'CASCADE' | 'RESTRICT' | 'SET NULL' | 'SET DEFAULT' }>;
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
type Settled = Required<Omit<Options<Fields>, 'search' | 'withoutRowid' | 'tenant' | 'renamedFrom'>> & Pick<Options<Fields>, 'search' | 'withoutRowid' | 'tenant' | 'renamedFrom'>;
type Kind = keyof typeof types.sqlite;
type Column = ReturnType<typeof column>;
type Values<F extends Fields, K extends Field<F>> = Array.NonEmptyReadonlyArray<F[K]['Type']>;
type Built<E> = Effect.Effect<Statement.Statement<SqlConnection.Row>, E, SqlClient.SqlClient>;
interface Relation<F extends Fields = Fields> extends Schema.Struct<F> {
    readonly table: ReturnType<typeof facts<F>>;
}
interface Table<F extends Fields> extends Relation<F> {
    readonly insert: (rows: Array.NonEmptyReadonlyArray<Schema.Struct.Type<F>>) => Built<Schema.SchemaError>;
    readonly upsert: (rows: Array.NonEmptyReadonlyArray<Schema.Struct.Type<F>>) => Built<Schema.SchemaError>;
    readonly ensure: (rows: Array.NonEmptyReadonlyArray<Schema.Struct.Type<F>>) => Effect.Effect<readonly object[], Schema.SchemaError | SqlError.SqlError, SqlClient.SqlClient>;
    readonly select: <K extends Field<F>>(field: K, values: Values<F, K>) => Built<Schema.SchemaError>;
    readonly remove: <K extends Field<F>>(field: K, values: Values<F, K>) => Built<Schema.SchemaError>;
    readonly latest: <G extends Field<F>>(group: G, order: Field<F>, groups: Values<F, G>) => Built<Schema.SchemaError>;
    readonly search: (text: string) => Built<Cause.NoSuchElementError>;
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

// --- [MODELS] --------------------------------------------------------------------------

const Bytes = Schema.Union([Schema.Uint8Array, Schema.Array(Schema.Int)]).pipe(Schema.decodeTo(Schema.Uint8Array, SchemaTransformation.transform<Uint8Array, Uint8Array | readonly number[]>({ decode: (bytes) => new Uint8Array(bytes), encode: Function.identity })));
const Uuid = Schema.String.check(Schema.isUUID(uuid.version));

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
const document = (columns: readonly string[]) => columns.map((name) => `coalesce(${quote(name)}, '')`).join(` || ' ' || `);

// --- [FACTS]
const column = (field: string, schema: Fields[string], options: Settled) => {
    const flat = (node: SchemaRepresentation.Representation): readonly SchemaRepresentation.Representation[] => (node._tag === 'Union' ? node.types.flatMap(flat) : [node]);
    const walk = (ast: SchemaAST.AST) => flat(SchemaRepresentation.toRepresentation(ast, { referencePolicy: () => undefined }).representation);
    const encoded = walk(SchemaAST.toEncoded(schema.ast));
    const values = encoded.filter(({ _tag }) => _tag !== 'Null');
    const decoded = walk(SchemaAST.toType(schema.ast)).filter(({ _tag }) => _tag !== 'Null');
    const checks = values.flatMap((node) => ('checks' in node ? node.checks.flatMap(({ representation }) => Option.toArray(Option.fromUndefinedOr(representation))) : []));
    const has = (id: string) => checks.some((check) => check.id === `effect/schema/${id}`);
    const rules: { readonly [id: string]: readonly (readonly [string, '>' | '>=' | '<' | '<=' | '%'])[] } = {
        'effect/schema/isGreaterThan': [['exclusiveMinimum', '>']],
        'effect/schema/isGreaterThanOrEqualTo': [['minimum', '>=']],
        'effect/schema/isLessThan': [['exclusiveMaximum', '<']],
        'effect/schema/isLessThanOrEqualTo': [['maximum', '<=']],
        'effect/schema/isBetween': [
            ['minimum', '>='],
            ['exclusiveMinimum', '>'],
            ['maximum', '<='],
            ['exclusiveMaximum', '<'],
        ],
        'effect/schema/isMinCodePoints': [['minCodePoints', '>=']],
        'effect/schema/isMaxCodePoints': [['maxCodePoints', '<=']],
        'effect/schema/isBetweenCodePoints': [
            ['minimum', '>='],
            ['maximum', '<='],
        ],
        ...(has('isInt') ? { 'effect/schema/isMultipleOf': [['divisor', '%'] as const] } : {}),
    };
    const boolean = decoded[0]?._tag === 'Boolean';
    const literals = values.flatMap((node) =>
        Match.value(node).pipe(
            Match.tag('Literal', ({ literal: member }) => (boolean ? [] : [member])),
            Match.tag('Enum', ({ enums }) => enums.map(([, member]) => member)),
            Match.orElse(() => []),
        ),
    );
    const numeric = Array.isReadonlyArrayNonEmpty(literals) && literals.every((member) => typeof member === 'number');
    const name = names.transformQueryNames(field);
    return {
        field,
        name,
        schema,
        kind: Match.value({ node: values[0], decoded: decoded[0], boolean, numeric, integer: has('isInt') || (numeric && literals.every(Number.isInteger)), finite: has('isFinite'), uuid: has('isUUID') }).pipe(
            Match.when({ node: { _tag: 'String', annotations: { contentMediaType: 'application/json' } } }, (): Kind => 'json'),
            Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'BigInt' } }, (): Kind => 'int64'),
            Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'Declaration', representation: { id: Match.is('effect/schema/Date', 'effect/schema/DateTimeUtc') } } }, (): Kind => 'instant'),
            Match.when({ node: { _tag: 'String' }, uuid: true }, (): Kind => 'uuid'),
            Match.when({ boolean: true }, (): Kind => 'boolean'),
            Match.whenOr({ node: { _tag: 'Number' }, integer: true }, { numeric: true, integer: true }, (): Kind => 'integer'),
            Match.whenOr({ node: { _tag: 'Number' }, finite: true }, { numeric: true }, (): Kind => 'real'),
            Match.when({ node: { _tag: 'Number' } }, (): Kind => 'number'),
            Match.whenOr({ node: { _tag: 'Declaration' } }, { node: { _tag: 'Arrays' } }, (): Kind => 'blob'),
            Match.orElse((): Kind => 'text'),
        ),
        nullable: values.length < encoded.length,
        literals,
        bounds: checks.flatMap(({ id, payload }) => {
            const given: { readonly [key: PropertyKey]: unknown } = Predicate.isObject(payload) ? payload : {};
            return Option.getOrElse(Record.get(rules, id), () => []).flatMap(([key, op]) => {
                const value = given[key];
                return typeof value === 'number' ? [{ op, value, length: id.endsWith('CodePoints') }] : [];
            });
        }),
        fallback: Option.fromNullishOr(options.defaults[field]),
        projection: Option.map(Option.fromUndefinedOr(options.projections[field]), ({ from, path }) => ({ from: names.transformQueryNames(from), path })),
        earlier: Option.map(Option.fromUndefinedOr(options.renamed[field]), names.transformQueryNames),
        incarnation: options.retired.filter((entry) => names.transformQueryNames(entry) === name).length,
        retype: options.retype.includes(field),
    };
};
const facts = <F extends Fields>(name: string, fields: F, options: Settled) => {
    const key = (entry: Key<Fields>) => (typeof entry === 'string' ? { column: names.transformQueryNames(entry), casefold: false } : { column: names.transformQueryNames(entry.casefold), casefold: true });
    const keys = Array.map(options.key, (entry) => key(entry));
    const declared = options.indexes.map((given) => {
        const entry = { unique: false, include: [], where: {}, method: 'btree' as const, ...given };
        return {
            keys: Array.map(entry.columns, key),
            unique: entry.unique,
            include: entry.include.map(names.transformQueryNames),
            where: Record.toEntries(entry.where).flatMap(([field, predicate]) => (predicate === undefined ? [] : [{ column: names.transformQueryNames(field), predicate }])),
            method: entry.method,
        };
    });
    const references = options.references.map((reference) => ({ columns: Array.map(reference.columns, names.transformQueryNames), table: reference.table, onDelete: Option.fromUndefinedOr(reference.onDelete) }));
    const leads = (columns: readonly string[], order: readonly ReturnType<typeof key>[]) => columns.every((part, position) => order[position]?.column === part && order[position]?.casefold === false);
    const covering = references.filter(({ columns }) => !(leads(columns, keys) || declared.some((index) => index.method === 'btree' && leads(columns, index.keys))));
    return {
        name,
        earlier: Option.fromUndefinedOr(options.renamedFrom),
        key: Array.map(keys, ({ column: part }) => part),
        columns: Record.map<keyof F & string, F[keyof F], Column>(fields, (schema, field) => column(field, schema, options)),
        indexes: [...declared, ...covering.map(({ columns }) => ({ keys: Array.map(columns, (part) => ({ column: part, casefold: false })), unique: false, include: [], where: [], method: 'btree' as const }))].map((index) => ({
            ...index,
            parts: [...index.keys.map(({ column: part }) => part), ...(index.method === 'btree' ? [] : [index.method])],
        })),
        references,
        search: Option.map(Option.fromUndefinedOr(options.search), (search) => ({ tokenizer: 'unicode61' as const, ...search, columns: search.columns.map(names.transformQueryNames) })),
        withoutRowid: options.withoutRowid === true,
        tenant: Option.map(Option.fromUndefinedOr(options.tenant), names.transformQueryNames),
        retired: { columns: options.retired.map(names.transformQueryNames), indexes: options.retiredIndexes.map((columns) => columns.map(names.transformQueryNames)) },
    };
};

// --- [DIALECTS]
const dialects = {
    sqlite: {
        types: types.sqlite,
        read: ({ name, kind }: Column, scope: string) => (kind === 'int64' ? `CAST(${scope}${quote(name)} AS TEXT) AS ${quote(name)}` : `${scope}${quote(name)}`),
        member: (scope: string, field: string) => `${scope} ->> ${literal(`$.${quote(field)}`)}`,
        element: ({ kind }: Column, text: string) => (kind === 'blob' ? `unhex(${text})` : text),
        constant: ({ kind }: Column, value: string | number) => (['integer', 'int64', 'real', 'number', 'boolean'].includes(kind) ? `${value}` : literal(`${value}`)),
        length: 'length',
        domain: ({ name, kind }: Column) =>
            Match.value(kind).pipe(
                Match.when('boolean', () => [`${quote(name)} IN (0, 1)`]),
                Match.when('json', () => [`json_valid(${quote(name)})`]),
                Match.orElse(() => []),
            ),
        projection: ({ kind }: Column, from: string, path: readonly string[]) => `${quote(from)} ${kind === 'json' ? '->' : '->>'} ${literal(`$${path.map((part) => `.${quote(part)}`).join('')}`)}`,
        generated: (expression: string) => `AS (${expression}) VIRTUAL`,
        constraint: (_: string, text: string) => text,
        action: Option.getOrElse((): string => 'NO ACTION'),
        casefold: (name: string) => `${quote(name)} COLLATE NOCASE`,
        projected: (_: string, name: string) => quote(name),
        methods: ['btree'],
        covering: (unique: boolean, include: readonly string[]) => ({ keys: unique ? [] : include.map(quote), clause: [] }),
        suffix: (withoutRowid: boolean) => (withoutRowid ? ' STRICT, WITHOUT ROWID' : ' STRICT'),
        search: (owner: string, columns: readonly string[], tokenizer: string) => {
            const [virtual, values] = [quote(`${owner}_fts`), columns.map(quote).join(', ')];
            const row = (prefix: string) => columns.map((name) => `${prefix}.${quote(name)}`).join(', ');
            const [inserted, deleted] = [`INSERT INTO ${virtual} ("rowid", ${values}) VALUES (NEW."rowid", ${row('NEW')});`, `INSERT INTO ${virtual} (${virtual}, "rowid", ${values}) VALUES ('delete', OLD."rowid", ${row('OLD')});`];
            const trigger = (event: string, verb: string, body: string) => ({ name: `${owner}_fts_${event}`, sql: `CREATE TRIGGER ${quote(`${owner}_fts_${event}`)} AFTER ${verb} ON ${quote(owner)} BEGIN ${body} END` });
            return {
                indexes: [],
                fts: Option.some({
                    name: `${owner}_fts`,
                    sql: `CREATE VIRTUAL TABLE ${virtual} USING fts5 (${values}, content = ${literal(owner)}, content_rowid = 'rowid', tokenize = ${literal(tokenizer === 'trigram' ? 'trigram' : 'unicode61 remove_diacritics 2')})`,
                    triggers: [trigger('insert', 'INSERT', inserted), trigger('delete', 'DELETE', deleted), trigger('update', 'UPDATE', `${deleted} ${inserted}`)],
                    rebuild: `INSERT INTO ${virtual} (${virtual}) VALUES ('rebuild')`,
                }),
            };
        },
        policy: () => Option.none(),
    },
    pg: {
        types: types.pg,
        read: ({ name, kind }: Column, scope: string) => (kind === 'int64' ? `${scope}${quote(name)}::text AS ${quote(name)}` : `${scope}${quote(name)}`),
        member: (scope: string, field: string) => `${scope} ->> ${literal(field)}`,
        element: ({ kind }: Column, text: string) => (kind === 'blob' ? `decode(${text}, 'hex')` : `(${text})::${types.pg[kind]}`),
        constant: ({ kind }: Column, value: string | number) =>
            Match.value(kind).pipe(
                Match.when('boolean', () => `${value === 1}`),
                Match.whenOr('integer', 'int64', 'real', 'number', () => `${value}`),
                Match.orElse(() => literal(`${value}`)),
            ),
        length: 'char_length',
        domain: ({ name, kind }: Column) =>
            Match.value(kind).pipe(
                Match.when('real', () => [`${quote(name)} NOT IN ('NaN', 'Infinity', '-Infinity')`]),
                Match.when('instant', () => [`isfinite(${quote(name)})`]),
                Match.orElse(() => []),
            ),
        projection: ({ kind }: Column, from: string, path: readonly string[]) => `(${quote(from)} ${kind === 'json' ? '#>' : '#>>'} ARRAY[${path.map(literal).join(', ')}])${['json', 'text'].includes(kind) ? '' : `::${types.pg[kind]}`}`,
        generated: (expression: string) => `GENERATED ALWAYS AS (${expression}) VIRTUAL`,
        constraint: (name: string, text: string) => `CONSTRAINT ${quote(name)} ${text}`,
        action: Option.match({ onNone: () => 'a', onSome: (rule: Option.Option.Value<ReturnType<typeof facts>['references'][number]['onDelete']>) => ({ CASCADE: 'c', RESTRICT: 'r', 'SET NULL': 'n', 'SET DEFAULT': 'd' })[rule] }),
        casefold: (name: string) => `casefold(${quote(name)} COLLATE pg_unicode_fast)`,
        projected: (expression: string) => `(${expression})`,
        methods: ['btree', 'gin', 'gist', 'brin'],
        covering: (_: boolean, include: readonly string[]) => ({ keys: [], clause: Array.isReadonlyArrayNonEmpty(include) ? [`INCLUDE (${include.map(quote).join(', ')})`] : [] }),
        suffix: (_: boolean) => '',
        search: (owner: string, columns: readonly string[], tokenizer: string) => {
            const [name, body] = [identifier([owner, 'search']), document(columns)];
            return {
                indexes: [{ name, parts: ['search'], trigram: tokenizer === 'trigram', text: (prefix: string, on: string) => `CREATE INDEX ${prefix}${quote(name)} ON ${on} USING gin (${tokenizer === 'trigram' ? `(${body}) gin_trgm_ops` : `to_tsvector('simple', ${body})`})` }],
                fts: Option.none(),
            };
        },
        policy: (owner: string, name: string, type: string) => {
            const [policy, rule] = [identifier([owner, 'tenant']), `${quote(name)} = nullif(current_setting('app.tenant', true), '')::${type}`];
            return Option.some({ name: policy, rule, text: (on: string, role: string) => `CREATE POLICY ${quote(policy)} ON ${on} TO ${role} USING (${rule}) WITH CHECK (${rule})` });
        },
    },
};

// --- [HOST]
const environment = Effect.gen(function* () {
    const d1 = yield* Effect.serviceOption(D1Client.D1Client);
    return { sql: (yield* SqlClient.SqlClient).withoutTransforms(), d1, cloudflare: Option.isSome(d1) || Option.isSome(yield* Effect.serviceOption(SqliteClient.SqliteClient)) };
});
const atomic = (statements: Array.NonEmptyReadonlyArray<Statement.Statement<object>>): Effect.Effect<Array.NonEmptyReadonlyArray<readonly object[]>, SqlError.SqlError, SqlClient.SqlClient> =>
    Effect.flatMap(environment, ({ sql, d1 }) =>
        Option.match(d1, {
            onSome: (client) => client.batch(statements),
            onNone: () => sql.withTransaction(Effect.all(statements)),
        }),
    );

// --- [STATEMENTS]
const encoded = <S extends Schema.Top>(schema: S) => Schema.encodeUnknownEffect(Schema.fromJsonString(Schema.Array(schema), { replacer: (_, value) => (value instanceof Uint8Array ? Hex.encode(value) : value) }));
const table = <const F extends Fields>(name: string, fields: F, options: Options<F> & ('id' extends Present<F> ? unknown : { readonly key: Array.NonEmptyReadonlyArray<Present<F>> })): Table<F> => {
    const struct = Schema.Struct(fields);
    const relation = facts(name, fields, { key: ['id'], indexes: [], references: [], defaults: {}, projections: {}, renamed: {}, retype: [], retired: [], retiredIndexes: [], ...options });
    const columns = Object.values(relation.columns);
    const written = columns.filter(({ projection }) => Option.isNone(projection));
    const engine = Effect.map(SqlClient.SqlClient, (sql) => {
        const dialect = sql.onDialectOrElse({ pg: () => dialects.pg, orElse: () => dialects.sqlite });
        const element = (entry: Column) => dialect.element(entry, dialect.member('e.value', entry.field));
        return {
            sql,
            dialect,
            owner: sql(relation.name),
            keys: sql.csv(relation.key.map(quote)),
            reads: (scope: string) => sql.csv(columns.map((entry) => dialect.read(entry, scope))),
            targets: sql.csv(written.map((entry) => quote(entry.name))),
            values: sql.csv(written.map(element)),
            order: sql.csv(relation.key.flatMap((part) => written.filter((entry) => entry.name === part).map(element))),
        };
    });
    const write = (merge: boolean) =>
        Effect.fnUntraced(function* (rows: Array.NonEmptyReadonlyArray<Schema.Struct.Type<F>>) {
            const { sql, owner, keys, reads, targets, values, order } = yield* engine;
            const body = yield* encoded(struct)(rows);
            const changed = written.filter((entry) => !relation.key.includes(entry.name)).map((entry) => sql(entry.name));
            const conflict = merge
                ? Array.match(changed, {
                      onEmpty: () => sql`ON CONFLICT (${keys}) DO NOTHING`,
                      onNonEmpty: (set) => sql`ON CONFLICT (${keys}) DO UPDATE SET ${sql.csv(set.map((part) => sql`${part} = excluded.${part}`))} WHERE (${sql.csv(set.map((part) => sql`${owner}.${part}`))}) IS DISTINCT FROM (${sql.csv(set.map((part) => sql`excluded.${part}`))})`,
                  })
                : sql.literal('');
            return sql.onDialectOrElse({
                pg: () => sql`INSERT INTO ${owner} (${targets}) SELECT ${values} FROM jsonb_array_elements(${body}) e(value) ORDER BY ${order} ${conflict} RETURNING ${reads('')}`,
                orElse: () => sql`INSERT INTO ${owner} (${targets}) SELECT ${values} FROM json_each(${body}) e WHERE true ${conflict} RETURNING ${reads('')}`,
            });
        });
    const ensure = Effect.fnUntraced(function* (rows: Array.NonEmptyReadonlyArray<Schema.Struct.Type<F>>) {
        const { sql, owner, keys, reads, targets, values, order } = yield* engine;
        const body = yield* encoded(struct)(rows);
        return yield* sql.onDialectOrElse({
            pg: () =>
                sql`WITH inserted AS (INSERT INTO ${owner} (${targets}) SELECT ${values} FROM jsonb_array_elements(${body}) e(value) ORDER BY ${order} ON CONFLICT (${keys}) DO NOTHING RETURNING *) SELECT ${reads('')} FROM inserted UNION ALL SELECT ${reads('')} FROM ${owner} WHERE (${keys}) IN (SELECT ${order} FROM jsonb_array_elements(${body}) e(value))`,
            orElse: () => Effect.map(atomic([sql`INSERT INTO ${owner} (${targets}) SELECT ${values} FROM json_each(${body}) e WHERE true ON CONFLICT (${keys}) DO NOTHING`, sql`SELECT ${reads('')} FROM ${owner} WHERE (${keys}) IN (SELECT ${order} FROM json_each(${body}) e) ORDER BY ${keys}`]), Array.lastNonEmpty),
        });
    });
    const matching = Effect.fnUntraced(function* <K extends Field<F>>(field: K, values: Values<F, K>) {
        const { sql, dialect } = yield* engine;
        const entry = relation.columns[field];
        const body = yield* encoded(entry.schema)(values);
        const element = dialect.element(entry, 'e.value');
        return sql.onDialectOrElse({
            pg: () => sql`${sql(entry.name)} IN (SELECT ${sql.literal(element)} FROM jsonb_array_elements_text(${body}) e(value))`,
            orElse: () => sql`${sql(entry.name)} IN (SELECT ${sql.literal(element)} FROM json_each(${body}) e)`,
        });
    });
    const select = Effect.fnUntraced(function* <K extends Field<F>>(field: K, values: Values<F, K>) {
        const { sql, owner, keys, reads } = yield* engine;
        const where = yield* matching(field, values);
        return sql`SELECT ${reads('')} FROM ${owner} WHERE ${where} ORDER BY ${keys}`;
    });
    const remove = Effect.fnUntraced(function* <K extends Field<F>>(field: K, values: Values<F, K>) {
        const { sql, owner, reads } = yield* engine;
        const where = yield* matching(field, values);
        return sql`DELETE FROM ${owner} WHERE ${where} RETURNING ${reads('')}`;
    });
    const latest = Effect.fnUntraced(function* <G extends Field<F>>(group: G, order: Field<F>, groups: Values<F, G>) {
        const { sql, dialect, owner, reads } = yield* engine;
        const by = relation.columns[group];
        const body = yield* encoded(by.schema)(groups);
        const matched = sql`i.${sql(by.name)} = ${sql.literal(dialect.element(by, 'e.value'))}`;
        const descending = sql.csv([relation.columns[order].name, ...relation.key].map((part) => sql`i.${sql(part)} DESC`));
        const keys = (scope: string) => sql.csv(relation.key.map((part) => `${scope}${quote(part)}`));
        return sql.onDialectOrElse({
            pg: () => sql`SELECT ${reads('l.')} FROM jsonb_array_elements_text(${body}) e(value) CROSS JOIN LATERAL (SELECT * FROM ${owner} i WHERE ${matched} ORDER BY ${descending} LIMIT 1) l`,
            orElse: () => sql`SELECT ${reads('t.')} FROM json_each(${body}) e JOIN ${owner} t ON (${keys('t.')}) = (SELECT ${keys('i.')} FROM ${owner} i WHERE ${matched} ORDER BY ${descending} LIMIT 1)`,
        });
    });
    const search = Effect.fnUntraced(function* (text: string) {
        const { sql, owner, keys, reads } = yield* engine;
        const { columns: searched, tokenizer } = yield* Effect.fromOption(relation.search);
        return sql.onDialectOrElse({
            pg: () => {
                const body = sql.literal(document(searched));
                return tokenizer === 'trigram'
                    ? sql`SELECT ${reads('')} FROM ${owner} WHERE (${body}) ILIKE ${`%${text.replaceAll(patterns.like, (character) => `\\${character}`)}%`} ORDER BY ${keys}`
                    : sql`SELECT ${reads('')} FROM ${owner} CROSS JOIN plainto_tsquery('simple', ${text}) q WHERE to_tsvector('simple', ${body}) @@ q ORDER BY ts_rank(to_tsvector('simple', ${body}), q) DESC`;
            },
            orElse: () => {
                const virtual = sql(`${relation.name}_fts`);
                const terms = (tokenizer === 'trigram' ? [text] : text.split(patterns.words).filter((word) => word !== '')).map((term) => `"${term.replaceAll('"', '""')}"`).join(' ');
                return sql`SELECT ${reads(`${quote(relation.name)}.`)} FROM ${virtual} JOIN ${owner} ON ${owner}."rowid" = ${virtual}."rowid" WHERE ${virtual} MATCH ${terms} ORDER BY ${virtual}."rank"`;
            },
        });
    });
    return Object.assign(struct, { table: relation, insert: write(false), upsert: write(true), ensure, select, remove, latest, search });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { atomic, Bytes, type Catalog, type Column, dialects, environment, identifier, literal, names, quote, type Relation, type Table, table, Uuid };
