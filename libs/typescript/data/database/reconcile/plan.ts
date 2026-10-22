import { D1Client } from '@effect/sql-d1';
import { Cause, Effect, Option, Struct } from 'effect';
import { SqlClient } from 'effect/sql';
import type { Catalog } from '../definition.ts';
import { refuse } from './finding.ts';
import { Pg } from './pg.ts';
import { Sqlite } from './sqlite.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const converge = Effect.fnUntraced(function* <E, R>(catalog: Catalog, runtime: Option.Option<string>, after: (sql: SqlClient.SqlClient, planned: Effect.Success<ReturnType<typeof Sqlite.plan>>) => Effect.Effect<unknown, E, R>) {
    const [sql, d1] = yield* Effect.all([
        Effect.filterOrFail(
            SqlClient.SqlClient,
            (client) => client.onDialectOrElse({ sqlite: () => true, pg: () => true, orElse: () => false }),
            () => new Cause.IllegalArgumentError('reconcile runs on SQLite and PostgreSQL clients'),
        ),
        Effect.serviceOption(D1Client.D1Client),
    ]);
    yield* refuse(sql, catalog);
    const steps = sql.onDialectOrElse({ pg: () => Pg.plan(sql, catalog, runtime), orElse: () => Sqlite.plan(sql, catalog) });
    const applied = Effect.tap(steps, (planned) => after(sql, planned));
    return yield* Option.match(d1, {
        onNone: () => sql.withTransaction(applied),
        onSome: () =>
            Effect.catchTag(applied, 'SqlError', (error) =>
                Effect.filterOrFail(
                    steps,
                    ({ ledger: written }) => Option.isNone(written),
                    () => error,
                ),
            ),
    });
});
const plan = Effect.fn('plan')((catalog: Catalog, runtime: Option.Option<string>) => converge(catalog, runtime, () => Effect.void), Effect.map(Struct.pick(['preamble', 'statements', 'concurrent', 'undeclared'])));

// --- [EXPORTS] -------------------------------------------------------------------------

export { converge, plan };
