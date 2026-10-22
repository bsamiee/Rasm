import { Array, Effect, Schema } from 'effect';
import { type SqlClient, SqlSchema } from 'effect/sql';
import { table } from '../table.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Entry = Schema.Struct({ origin: Schema.optionalKey(Schema.String), incarnation: Schema.optionalKey(Schema.Int) });
const ledger = table('reconcile_revision', { revision: Schema.Int, identity: Schema.fromJsonString(Schema.Record(Schema.String, Schema.Struct({ ...Entry.fields, columns: Schema.optionalKey(Schema.Record(Schema.String, Entry)) }))) }, { key: ['revision'] });

// --- [OPERATIONS] ----------------------------------------------------------------------

const latestRevision = (sql: SqlClient.SqlClient) =>
    Effect.flatMap(
        sql.onDialectOrElse({
            pg: () => sql`SELECT relname FROM pg_class WHERE relnamespace = current_schema()::regnamespace AND relname = ${ledger.definition.name}`,
            orElse: () => sql`SELECT name FROM sqlite_schema WHERE name = ${ledger.definition.name}`,
        }),
        (rows) => (Array.isReadonlyArrayNonEmpty(rows) ? SqlSchema.findOneOption({ Request: Schema.Void, Result: ledger, execute: () => sql`SELECT * FROM ${sql(ledger.definition.name)}` })(undefined) : Effect.succeedNone),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Entry, latestRevision, ledger };
