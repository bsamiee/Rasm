import { runMain } from '@effect/platform-node/NodeRuntime';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { Catalog } from '@rasm/database';
import { s3Storage, serveNode } from '@rasm/hosting/node';
import { Layer } from 'effect';
import config from '#/cloudflare.config.ts';
import { app } from '#/server/app.ts';
import { catalog } from '#/server/catalog.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

serveNode(config, (env) =>
    app(
        env.OWNER_EMAIL,
        Layer.mergeAll(
            Catalog.layer(catalog, (options) => SqliteClient.layer({ filename: env.DB, ...options })),
            s3Storage(env.BUCKET, { forcePathStyle: true }),
        ),
    ),
).pipe(Layer.launch, runMain);
