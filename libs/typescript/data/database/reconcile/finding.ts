import { Array, Effect, Equal, Option, Record, Schema } from 'effect';
import type { SqlClient } from 'effect/sql';
import type { Catalog } from '#/definition';

// --- [CONSTANTS] -----------------------------------------------------------------------

const reasons = {
    unbounded: 'Numbers without isFinite, which each engine stores differently',
    reserved: 'rowid alias names (rowid, oid, _rowid_), which node SQLite refuses',
    projected: 'projections in a key or reference, which PostgreSQL refuses on virtual columns',
    tenant: 'a tenant scope, which needs PostgreSQL row security',
    search: 'a search key other than one Schema.Int field, which FTS5 rowids need',
    generated: 'undeclared generated columns a rebuild would lose',
};

// --- [MODELS] --------------------------------------------------------------------------

const Finding = Schema.TaggedUnion({
    Unsupported: { table: Schema.String, reason: Schema.Literals(Record.keys(reasons)), columns: Schema.NonEmptyArray(Schema.String) },
    Required: { table: Schema.String, columns: Schema.NonEmptyArray(Schema.String) },
    RetainedReference: { table: Schema.String, columns: Schema.NonEmptyArray(Schema.String), target: Schema.String },
});

// --- [ERRORS] --------------------------------------------------------------------------

class ReconcileError extends Schema.TaggedError<ReconcileError, { readonly brand: unique symbol }>()('ReconcileError', { findings: Schema.NonEmptyArray(Finding) }) {
    override get message() {
        return this.findings
            .map(
                Finding.match({
                    Unsupported: ({ table: owner, reason, columns }) => `${owner} (${columns.join(', ')}): ${reasons[reason]}`,
                    Required: ({ table: owner, columns }) => `${owner} (${columns.join(', ')}): required with no default for existing rows`,
                    RetainedReference: ({ table: owner, columns, target }) => `${owner} (${columns.join(', ')}): references ${target}, which this plan replaces`,
                }),
            )
            .join('; ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const unsupported = <C extends { readonly name: string }>(owner: string, reason: keyof typeof reasons, rows: readonly C[], refuses: (row: C) => boolean) =>
    Array.match(
        rows.filter(refuses).map(({ name }) => name),
        { onEmpty: () => [], onNonEmpty: (columns) => [Finding.cases.Unsupported.make({ table: owner, reason, columns })] },
    );
const refuse = (sql: SqlClient.SqlClient, catalog: Catalog) =>
    Array.match(
        catalog.tables.flatMap(({ definition }) => {
            const [all, linked] = [Object.values(definition.columns), [...definition.key, ...definition.references.flatMap(({ columns }) => columns)]];
            const unkeyed = Option.isSome(definition.search) && (definition.withoutRowid || !all.some(({ name, kind }) => kind === 'integer' && Equal.equals(definition.key, [name])));
            return [
                ...unsupported(definition.name, 'unbounded', all, ({ kind }) => kind === 'number'),
                ...unsupported(definition.name, 'reserved', all, ({ name }) => ['rowid', 'oid', '_rowid_'].includes(name.toLowerCase())),
                ...unsupported(definition.name, 'projected', all, ({ name, projection }) => Option.isSome(projection) && linked.includes(name)),
                ...sql.onDialectOrElse({ sqlite: () => [...unsupported(definition.name, 'tenant', all, ({ name }) => Option.contains(definition.tenant, name)), ...unsupported(definition.name, 'search', all, ({ name }) => unkeyed && definition.key.includes(name))], orElse: () => [] }),
            ];
        }),
        { onEmpty: () => Effect.void, onNonEmpty: (findings) => Effect.fail(ReconcileError.make({ findings })) },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Finding, ReconcileError, refuse, unsupported };
