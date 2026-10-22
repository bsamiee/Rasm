import { Array, Equal, Option, Record, type Schema } from 'effect';
import { type Column, type ColumnOptions, column, conditions, type Field, type Fields, type Where } from '#/column';
import { identifier, literal, quote, transforms } from '#/lexical';

// --- [TYPES] ---------------------------------------------------------------------------

type Present<F extends Fields> = string extends keyof F ? string : { readonly [K in keyof F]: null extends F[K]['Encoded'] ? never : K }[keyof F] & string;
interface TableOptions<F extends Fields> extends ColumnOptions<F> {
    readonly key?: Array.NonEmptyReadonlyArray<Present<F>>;
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
    readonly tenant?: Present<F>;
    readonly renamedFrom?: string;
    readonly retired?: readonly string[];
    readonly retiredIndexes?: readonly Array.NonEmptyReadonlyArray<string>[];
}
interface Relation<F extends Fields = Fields> extends Schema.Struct<F> {
    readonly definition: ReturnType<typeof definition<F>>;
}
interface Catalog {
    readonly tables: Array.NonEmptyReadonlyArray<Relation>;
    readonly retired: readonly string[];
    readonly reader?: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const referentialActions = { 'NO ACTION': 'a', RESTRICT: 'r', CASCADE: 'c', 'SET NULL': 'n', 'SET DEFAULT': 'd' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const ftsTable = (owner: string) => `${owner}_fts`;
const document = ({ columns, tokenizer }: Option.Option.Value<Relation['definition']['search']>) => {
    const joined = columns.map(({ name }) => `coalesce(${quote(name)}, '')`).join(` || ' ' || `);
    return tokenizer === 'trigram' ? joined : `to_tsvector(${literal(tokenizer.name)}, ${joined})`;
};
const definition = <F extends Fields>(name: string, fields: F, options: TableOptions<F>) => {
    const defaulted = { key: ['id'], indexes: [], references: [], retired: [], retiredIndexes: [], ...options };
    const key = Array.map(defaulted.key, transforms.transformQueryNames);
    const retired = defaulted.retired.map(transforms.transformQueryNames);
    const declared = defaulted.indexes.map((index) => {
        const { columns, unique, include, where, method } = { unique: false, include: [], method: 'btree', where: {}, ...index };
        return {
            keys: Array.map(columns, (entry) => (typeof entry === 'string' ? { column: transforms.transformQueryNames(entry), casefold: false } : { column: transforms.transformQueryNames(entry.casefold), casefold: true })),
            unique,
            include: include.map(transforms.transformQueryNames),
            where: conditions(where),
            method,
        };
    });
    const linked = defaulted.references.map((reference) => ({ onDelete: 'NO ACTION' as const, ...reference, columns: Array.map(reference.columns, transforms.transformQueryNames) }));
    const orders = [key, ...declared.filter(({ method }) => method === 'btree').map((index) => Array.takeWhile(index.keys, ({ casefold }) => !casefold).map(({ column: entry }) => entry))];
    const covering = linked.filter(({ columns }) => !orders.some((order) => Equal.equals(Array.take(order, columns.length), columns)));
    const described = Record.map<Field<F>, F[Field<F>], Column>(fields, (schema, field) => column(field, schema, options, retired));
    return {
        name: transforms.transformQueryNames(name),
        earlier: Option.map(Option.fromUndefinedOr(options.renamedFrom), transforms.transformQueryNames),
        key,
        columns: described,
        indexes: [...declared, ...covering.map(({ columns }) => ({ keys: Array.map(columns, (entry) => ({ column: entry, casefold: false })), unique: false, include: [], where: [], method: 'btree' }))].map((index) => ({
            ...index,
            parts: [...index.keys.map(({ column: entry }) => entry), ...(index.method === 'btree' ? [] : [index.method])],
        })),
        references: linked,
        search: Option.map(Option.fromUndefinedOr(options.search), ({ columns, tokenizer }) => ({
            columns: Array.map(columns, (field): Column => described[field]),
            tokenizer: tokenizer === 'trigram' ? tokenizer : { ...tokenizer, name: identifier([tokenizer.configuration, ...tokenizer.filters]) },
        })),
        withoutRowid: options.withoutRowid === true,
        tenant: Option.map(Option.fromUndefinedOr(options.tenant), transforms.transformQueryNames),
        retired: { columns: retired, indexes: defaulted.retiredIndexes.map((columns) => columns.map(transforms.transformQueryNames)) },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Catalog, definition, document, ftsTable, type Present, type Relation, referentialActions, type TableOptions };
