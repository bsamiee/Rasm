import { Array, Effect, Option, Schema, Struct } from 'effect';
import { SqlClient } from 'effect/sql';
import type { Catalog } from '../definition.ts';
import { atomic } from '../transaction.ts';
import { ledger } from './ledger.ts';
import { converge } from './plan.ts';
import type { Sqlite } from './sqlite.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const write = (sql: SqlClient.SqlClient, { statements, ledger: written }: Effect.Success<ReturnType<typeof Sqlite.plan>>) =>
    Effect.transposeOption(
        Option.map(written, ({ revision, document }) =>
            Effect.flatMap(Schema.encodeEffect(ledger)({ revision: revision + 1, identity: document }), (row) =>
                atomic(
                    Array.prependAll(
                        [sql`INSERT INTO ${sql(ledger.definition.name)} ${sql.insert(row)}`, sql`DELETE FROM ${sql(ledger.definition.name)} WHERE ${sql('revision')} = ${revision}`],
                        statements.map((text) => sql.unsafe(text)),
                    ),
                ),
            ),
        ),
    );
const apply = Effect.fn('apply')(
    (catalog: Catalog, runtime: Option.Option<string>) => converge(catalog, runtime, write),
    Effect.tap(({ concurrent }) => SqlClient.SqlClient.use((sql) => Effect.forEach(concurrent, (text) => sql.unsafe(text), { discard: true }))),
    Effect.map(Struct.get('undeclared')),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { apply };
