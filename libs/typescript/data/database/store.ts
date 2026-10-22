import { Effect, Layer, Option, Schema } from 'effect';
import { SqlClient } from 'effect/sql';
import type { Catalog } from './definition.ts';
import { transforms } from './lexical.ts';
import { apply } from './reconcile/apply.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const applyLogged = (catalog: Catalog, runtime: Option.Option<string>) => Effect.tap(apply(catalog, runtime), (undeclared) => (Object.values(undeclared).some((objects) => objects.length > 0) ? Effect.annotateLogs(Effect.logInfo('undeclared database objects'), undeclared) : Effect.void));

// --- [COMPOSITION] ---------------------------------------------------------------------

const store = <A, E, R>(catalog: Catalog, client: (options: typeof transforms) => Layer.Layer<A | SqlClient.SqlClient, E, R>) =>
    Layer.effectDiscard(
        Effect.andThen(
            applyLogged(catalog, Option.none()),
            SqlClient.SqlClient.use((sql) => sql.onDialectOrElse({ sqlite: () => sql`PRAGMA optimize = 0x10002`, orElse: () => Effect.void })),
        ),
    ).pipe(Layer.provideMerge(client(transforms)));
const reconciled = <E, R, M, N>(catalog: Catalog, runtime: Layer.Layer<SqlClient.SqlClient, E, R>, migrator: Layer.Layer<SqlClient.SqlClient, M, N>) =>
    Layer.effectDiscard(
        SqlClient.SqlClient.use((sql) => sql`SELECT current_user AS role`).pipe(
            Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ role: Schema.String })]))),
            Effect.provide(runtime),
            Effect.flatMap(([{ role }]) => Effect.provide(applyLogged(catalog, Option.some(role)), migrator)),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { reconciled, store };
