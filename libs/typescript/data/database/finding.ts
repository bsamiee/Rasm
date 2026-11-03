import { Record, Schema } from 'effect';

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
    override get message() {
        return this.findings.map(({ table, columns, reason }) => `${table} (${columns.join(', ')}) ${described(reason)}`).join('\n');
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ReconcileError };
