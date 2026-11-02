import { Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const reasons = {
    unbounded: 'Schema.Number without isFinite stores differently per engine',
    reserved: 'node SQLite refuses rowid alias name',
    projected: 'PostgreSQL refuses virtual column in key or reference',
    tenant: 'tenant scope needs PostgreSQL row security',
    search: 'FTS5 search needs one Schema.Int key column as rowid',
    generated: 'rebuild drops undeclared generated column',
    invalid: 'default or predicate value fails its column schema',
};

// --- [MODELS] --------------------------------------------------------------------------

const Finding = Schema.TaggedUnion({
    Unsupported: { table: Schema.String, reason: Schema.Literals(Record.keys(reasons)), columns: Schema.NonEmptyArray(Schema.String) },
    Required: { table: Schema.String, columns: Schema.NonEmptyArray(Schema.String) },
    Retype: { table: Schema.String, columns: Schema.NonEmptyArray(Schema.String) },
    RetainedReference: { table: Schema.String, columns: Schema.NonEmptyArray(Schema.String), target: Schema.String },
});

// --- [ERRORS] --------------------------------------------------------------------------

class ReconcileError extends Schema.TaggedError<ReconcileError, { readonly brand: unique symbol }>()('ReconcileError', { findings: Schema.NonEmptyArray(Finding) }) {
    override get message() {
        return this.findings
            .map(
                Finding.match({
                    Unsupported: ({ table, reason, columns }) => `${table} (${columns.join(', ')}) ${reasons[reason]}`,
                    Required: ({ table, columns }) => `${table} (${columns.join(', ')}) required, add default for existing rows`,
                    Retype: ({ table, columns }) => `${table} (${columns.join(', ')}) stored definition differs, declare retype`,
                    RetainedReference: ({ table, columns, target }) => `${table} (${columns.join(', ')}) references dropped table ${target}`,
                }),
            )
            .join('\n');
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Finding, ReconcileError };
