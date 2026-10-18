import { Array, Function, Hash, Match, Option, Predicate, Record, Schema, SchemaAST, SchemaRepresentation, SchemaTransformation, String } from 'effect';
import { Statement } from 'effect/sql';

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
    readonly references?: ReadonlyArray<{ readonly columns: Array.NonEmptyReadonlyArray<Field<F>>; readonly table: Table; readonly onDelete?: 'CASCADE' | 'RESTRICT' | 'SET NULL' | 'SET DEFAULT' }>;
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
interface Table<F extends Fields = Fields> extends Schema.Struct<F> {
    readonly table: ReturnType<typeof facts<F>>;
}
interface Catalog {
    readonly tables: Array.NonEmptyReadonlyArray<Table>;
    readonly retired: readonly string[];
    readonly reader?: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const names = { transformQueryNames: String.camelToSnake, transformResultNames: String.snakeToCamel, transformJson: false };
const limits = { identifier: 63, digest: 36, columns: 100 };
const uuid = { version: 7 } as const;
const types = {
    sqlite: { text: 'TEXT', uuid: 'TEXT', json: 'TEXT', instant: 'TEXT', integer: 'INTEGER', int64: 'INTEGER', real: 'REAL', number: 'REAL', boolean: 'INTEGER', blob: 'BLOB' },
    pg: { text: 'text', uuid: 'uuid', json: 'jsonb', instant: 'timestamptz(3)', integer: 'bigint', int64: 'bigint', real: 'double precision', number: 'double precision', boolean: 'boolean', blob: 'bytea' },
} as const;
const operators = {
    minimum: { inclusive: '>=', exclusive: '>', flag: 'exclusiveMinimum' },
    exclusiveMinimum: { inclusive: '>', exclusive: '>', flag: 'exclusiveMinimum' },
    maximum: { inclusive: '<=', exclusive: '<', flag: 'exclusiveMaximum' },
    exclusiveMaximum: { inclusive: '<', exclusive: '<', flag: 'exclusiveMaximum' },
    minCodePoints: { inclusive: '>=', exclusive: '>=', flag: 'minCodePoints' },
    maxCodePoints: { inclusive: '<=', exclusive: '<=', flag: 'maxCodePoints' },
    divisor: { inclusive: '%', exclusive: '%', flag: 'divisor' },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Bytes = Schema.Union([Schema.Uint8Array, Schema.Array(Schema.Int)]).pipe(Schema.decodeTo(Schema.Uint8Array, SchemaTransformation.transform<Uint8Array, Uint8Array | readonly number[]>({ decode: (bytes) => new Uint8Array(bytes), encode: Function.identity })));
const Uuid = Schema.String.check(Schema.isUUID(uuid.version));

// --- [OPERATIONS] ----------------------------------------------------------------------

const quote = Statement.defaultEscape('"');
const literal = (text: string) => `'${text.replaceAll("'", "''")}'`;
const identifier = (parts: readonly string[]) => {
    const name = parts.map(names.transformQueryNames).join('_');
    const bytes = (text: string) => new TextEncoder().encode(text).length;
    const digest = Math.abs(Hash.string(name)).toString(limits.digest);
    return bytes(name) <= limits.identifier ? name : `${[...name].filter((_, index, characters) => bytes(characters.slice(0, index + 1).join('')) < limits.identifier - digest.length).join('')}_${digest}`;
};
const column = (field: string, schema: Fields[string], options: Settled) => {
    const flat = (node: SchemaRepresentation.Representation): readonly SchemaRepresentation.Representation[] => (node._tag === 'Union' ? node.types.flatMap(flat) : [node]);
    const walk = (ast: SchemaAST.AST) => flat(SchemaRepresentation.toRepresentation(ast, { referencePolicy: () => undefined }).representation);
    const encoded = walk(SchemaAST.toEncoded(schema.ast));
    const values = encoded.filter(({ _tag }) => _tag !== 'Null');
    const decoded = walk(SchemaAST.toType(schema.ast)).filter(({ _tag }) => _tag !== 'Null');
    const checks = values.flatMap((node) => ('checks' in node ? node.checks.flatMap(({ representation }) => Option.toArray(Option.fromUndefinedOr(representation))) : []));
    const has = (id: string) => checks.some((check) => check.id === `effect/schema/${id}`);
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
            return Record.toEntries(operators).flatMap(([key, op]) => {
                const value = given[key];
                return typeof value === 'number' && (key !== 'divisor' || has('isInt')) ? [{ op: op[given[op.flag] === true ? 'exclusive' : 'inclusive'], value, length: id.endsWith('CodePoints') }] : [];
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
const table = <const F extends Fields>(name: string, fields: F, options: Options<F> & ('id' extends Present<F> ? unknown : { readonly key: Array.NonEmptyReadonlyArray<Present<F>> })): Table<F> =>
    Object.assign(Schema.Struct(fields), { table: facts(name, fields, { key: ['id'], indexes: [], references: [], defaults: {}, projections: {}, renamed: {}, retype: [], retired: [], retiredIndexes: [], ...options }) });
const dialects = {
    sqlite: {
        types: types.sqlite,
        read: ({ name, kind }: Column, scope: string) => (kind === 'int64' ? `CAST(${scope}${quote(name)} AS TEXT) AS ${quote(name)}` : `${scope}${quote(name)}`),
        member: (scope: string, field: string) => `${scope} ->> ${literal(`$.${quote(field)}`)}`,
        element: ({ kind }: Column, text: string) => (kind === 'blob' ? `unhex(${text})` : text),
    },
    pg: {
        types: types.pg,
        read: ({ name, kind }: Column, scope: string) => (kind === 'int64' ? `${scope}${quote(name)}::text AS ${quote(name)}` : `${scope}${quote(name)}`),
        member: (scope: string, field: string) => `${scope} ->> ${literal(field)}`,
        element: ({ kind }: Column, text: string) => (kind === 'blob' ? `decode(${text}, 'hex')` : `(${text})::${types.pg[kind]}`),
        document: (columns: readonly string[]) => columns.map((name) => `coalesce(${quote(name)}, '')`).join(` || ' ' || `),
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Catalog, Column, Fields, Table };
export { Bytes, dialects, identifier, limits, literal, names, quote, table, Uuid };
