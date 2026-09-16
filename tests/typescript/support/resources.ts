// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeHttpServer } from '@effect/platform-node';
import { PgliteClient } from '@effect/sql-pglite';
import { Effect, Layer, type Scope } from 'effect';
import { HttpServer, type HttpServerError, type HttpServerRequest, type HttpServerResponse } from 'effect/unstable/http';
import { SqlClient, type SqlError } from 'effect/unstable/sql';

// --- [LAYERS] --------------------------------------------------------------------------

const pglite = (...seed: readonly string[]): Layer.Layer<PgliteClient.PgliteClient | SqlClient.SqlClient, SqlError.SqlError> =>
    Layer.provideMerge(
        Layer.effectDiscard(SqlClient.SqlClient.use((sql) => Effect.forEach(seed, (statement) => sql.unsafe(statement), { discard: true }))),
        PgliteClient.layer({ dataDir: 'memory://', relaxedDurability: true }),
    );

const loopback = <E>(
    app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, HttpServerRequest.HttpServerRequest | Scope.Scope>,
): Layer.Layer<Layer.Success<typeof NodeHttpServer.layerTest>, HttpServerError.ServeError> => Layer.provideMerge(HttpServer.serve(app), NodeHttpServer.layerTest);

// --- [EXPORTS] -------------------------------------------------------------------------

export { loopback, pglite };
