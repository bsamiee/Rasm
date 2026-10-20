import { Effect, Layer, Option, Schema } from 'effect';
import { SqlClient } from 'effect/sql';
import { reconcile } from './reconcile.ts';
import { type Catalog, names } from './table.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const tenant = <A, E, R>(id: string, effect: Effect.Effect<A, E, R>) => SqlClient.SqlClient.use((sql) => sql.withTransaction(Effect.andThen(sql`SELECT set_config('app.tenant', ${id}, true)`, effect)));

// --- [COMPOSITION] ---------------------------------------------------------------------

const store = <A, E, R, M = never, N = never>(catalog: Catalog, client: (options: typeof names) => Layer.Layer<A | SqlClient.SqlClient, E, R>, migrator?: (options: typeof names) => Layer.Layer<SqlClient.SqlClient, M, N>) =>
    Layer.effectDiscard(
        Effect.gen(function* () {
            const sql = yield* SqlClient.SqlClient;
            const undeclared = yield* Option.match(Option.fromUndefinedOr(migrator), {
                onNone: () => reconcile(catalog, Option.none()),
                onSome: (layer) =>
                    sql`SELECT current_user AS role`.pipe(
                        Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ role: Schema.String })]))),
                        Effect.flatMap(([{ role }]) => Effect.provide(reconcile(catalog, Option.some(role)), layer(names))),
                    ),
            });
            yield* Object.values(undeclared).some((objects) => objects.length > 0) ? Effect.logInfo('undeclared', undeclared) : Effect.void;
            yield* sql.onDialectOrElse({ sqlite: () => Effect.acquireRelease(sql`PRAGMA optimize = 0x10002`, () => Effect.orDie(sql`PRAGMA optimize`)), orElse: () => Effect.void });
        }),
    ).pipe(Layer.provideMerge(client(names)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { store, tenant };
