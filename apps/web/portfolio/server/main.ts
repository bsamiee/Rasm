import { createServer } from 'node:http';
import { layerConfig } from '@effect/platform-node/NodeHttpServer';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { Catalog } from '@rasm/database';
import { Sqlite } from '@rasm/database/sqlite';
import { App } from '@rasm/hosting/app';
import { serveNode } from '@rasm/hosting/node';
import { Sites } from '@rasm/hosting/openai-sites';
import { s3Storage } from '@rasm/object-storage/s3';
import { layerTelemetry } from '@rasm/process';
import { Config, Layer } from 'effect';
import hosting from '#/hosting.config';
import { app } from '#/server/app';
import { catalog } from '#/server/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

serveNode(hosting, Sites.identity, (env) =>
    app(
        env.OWNER_EMAIL,
        Layer.mergeAll(
            Catalog.layer(
                catalog,
                Sqlite.layer((names) => SqliteClient.layer({ filename: env.DB, ...names })),
            ),
            s3Storage({ bucket: env.BUCKET, uploadPrefix: App.uploads }, { forcePathStyle: true }),
        ),
    ),
).pipe(Layer.provide(layerConfig(createServer, { port: Config.Port(App.bundle.portVariable) })), Layer.provide(layerTelemetry({ resource: { serviceName: hosting.name }, target: 'stderr' })), Layer.launch, runMain);
