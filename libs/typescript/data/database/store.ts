import { Effect, Layer, Option, Schema } from 'effect';
import { SqlClient } from 'effect/sql';
import { reconcile } from './reconcile.ts';
import { type Catalog, names } from './table.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const tenant = <A, E, R>(id: string, effect: Effect.Effect<A, E, R>) => SqlClient.SqlClient.use((sql) => sql.withTransaction(Effect.andThen(sql`SELECT set_config('app.tenant', ${id}, true)`, effect)));
const settled = (catalog: Catalog, runtime: Option.Option<string>) => Effect.tap(reconcile(catalog, runtime), (undeclared) => (Object.values(undeclared).some((objects) => objects.length > 0) ? Effect.annotateLogs(Effect.logInfo('undeclared database objects'), undeclared) : Effect.void));

// --- [COMPOSITION] ---------------------------------------------------------------------

const store = <A, E, R>(catalog: Catalog, client: (options: typeof names) => Layer.Layer<A | SqlClient.SqlClient, E, R>) =>
    Layer.effectDiscard(
        Effect.andThen(
            settled(catalog, Option.none()),
            SqlClient.SqlClient.use((sql) => sql.onDialectOrElse({ sqlite: () => sql`PRAGMA optimize = 0x10002`, orElse: () => Effect.void })),
        ),
    ).pipe(Layer.provideMerge(client(names)));
const reconciled = <E, R, M, N>(catalog: Catalog, runtime: Layer.Layer<SqlClient.SqlClient, E, R>, migrator: Layer.Layer<SqlClient.SqlClient, M, N>) =>
    Layer.effectDiscard(
        SqlClient.SqlClient.use((sql) => sql`SELECT current_user AS role`).pipe(
            Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ role: Schema.String })]))),
            Effect.provide(runtime),
            Effect.flatMap(([{ role }]) => Effect.provide(settled(catalog, Option.some(role)), migrator)),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { reconciled, store, tenant };
