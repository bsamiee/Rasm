import { Effect, Layer, Option, Schema } from 'effect';
import { SqlClient } from 'effect/sql';
import { reconcile } from './reconcile.ts';
import { type Catalog, environment, names } from './table.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const tenant = Effect.fnUntraced(function* <A, E, R>(id: string, effect: Effect.Effect<A, E, R>) {
    const sql = yield* SqlClient.SqlClient;
    return yield* sql.withTransaction(Effect.andThen(sql`SELECT set_config('app.tenant', ${id}, true)`, effect));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const store = <A, E, R, M = never, N = never>(catalog: Catalog, client: (options: typeof names) => Layer.Layer<A | SqlClient.SqlClient, E, R>, migrator?: (options: typeof names) => Layer.Layer<SqlClient.SqlClient, M, N>) =>
    Layer.effectDiscard(
        Effect.gen(function* () {
            const { sql, cloudflare } = yield* environment;
            const runtime = sql.onDialectOrElse({
                pg: () => Effect.map(Effect.flatMap(sql`SELECT current_user AS role`, Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ role: Schema.String })]))), ([{ role }]) => Option.some(role)),
                orElse: () => Effect.succeedNone,
            });
            const undeclared = yield* Option.match(Option.fromUndefinedOr(migrator), {
                onNone: () => reconcile(catalog, Option.none()),
                onSome: (layer) => Effect.flatMap(runtime, (role) => Effect.provide(reconcile(catalog, role), layer(names))),
            });
            yield* Object.values(undeclared).some((objects) => objects.length > 0) ? Effect.logInfo('undeclared', undeclared) : Effect.void;
            yield* sql.onDialectOrElse({ sqlite: () => !cloudflare, orElse: () => false })
                ? Effect.andThen(
                      sql`PRAGMA optimize = 0x10002`,
                      Effect.addFinalizer(() => Effect.orDie(sql`PRAGMA optimize`)),
                  )
                : Effect.void;
        }),
    ).pipe(Layer.provideMerge(client(names)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { store, tenant };
