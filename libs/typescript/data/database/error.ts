import { Array, Effect, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const refusals = {
    unbounded: 'Schema.Number without isFinite stores differently per engine',
    mixed: 'union mixes encoded kinds one column type cannot store',
    reserved: 'names a SQLite rowid alias',
    projected: 'PostgreSQL refuses virtual column in key or reference',
    tenant: 'tenant scope needs PostgreSQL row security',
    search: 'FTS5 search needs one Schema.Int key column as rowid',
    casefold: 'SQLite NOCASE folds ASCII alone, declare isAscii',
    opclass: 'index method has no operator class for column kind',
    generated: 'rebuild drops undeclared generated column',
    retained: 'rebuild drops undeclared column constraints',
    invalid: 'default or predicate value fails its column schema',
    mergeTree: 'MergeTree options need ClickHouse',
    reference: 'ClickHouse has no foreign key',
    unique: 'ClickHouse has no unique, partial, or gist index',
    fulltext: 'ClickHouse has no declared text search',
    collection: 'ClickHouse collection column takes no Nullable, default, or JSON path projection',
    layout: 'ClickHouse expires needs an instant column, version an integer or instant column, and a codec a stored column',
};

// --- [MODELS] --------------------------------------------------------------------------

const Reason = Schema.TaggedUnion({ Unsupported: { refusal: Schema.Literals(Record.keys(refusals)) }, Required: {}, Retype: {}, RetainedReference: { target: Schema.String } });
const Finding = Schema.Struct({ table: Schema.String, columns: Schema.NonEmptyArray(Schema.String), reason: Reason });
const described = Reason.match({
    Unsupported: ({ refusal }) => refusals[refusal],
    Required: () => 'required, add default for existing rows',
    Retype: () => 'stored definition differs, declare retype',
    RetainedReference: ({ target }) => `references dropped table ${target}`,
});

// --- [ERRORS] --------------------------------------------------------------------------

class ReconcileError extends Schema.TaggedError<ReconcileError, { readonly brand: unique symbol }>()('ReconcileError', { findings: Schema.NonEmptyArray(Finding) }) {
    static readonly Finding = Finding;
    static readonly raise = (findings: readonly (typeof Finding.Type)[]) => (Array.isReadonlyArrayNonEmpty(findings) ? ReconcileError.make({ findings }) : Effect.void);
    override get message() {
        return this.findings.map(({ table, columns, reason }) => `${table} (${columns.join(', ')}) ${described(reason)}`).join('\n');
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ReconcileError };
