import { runMain } from '@effect/platform-node/NodeRuntime';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { Catalog } from '@rasm/database';
import { Sqlite } from '@rasm/database/sqlite';
import { serveNode } from '@rasm/hosting/node';
import { s3Storage } from '@rasm/hosting/s3';
import { Layer, Logger } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { Otlp, OtlpSerialization } from 'effect/observability';
import hosting from '#/hosting.config';
import { app } from '#/server/app';
import { catalog } from '#/server/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

serveNode(hosting, (env) =>
    app(
        env.OWNER_EMAIL,
        Layer.mergeAll(
            Catalog.layer(
                catalog,
                Sqlite.layer((transforms) => SqliteClient.layer({ filename: env.DB, ...transforms })),
            ),
            s3Storage(env.BUCKET, { forcePathStyle: true }),
        ),
    ),
).pipe(
    Layer.provide(Otlp.layerFromConfig({ resource: { serviceName: hosting.name } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(Logger.layer([Logger.consoleJson, Logger.tracerLogger])))),
    Layer.launch,
    runMain,
);
