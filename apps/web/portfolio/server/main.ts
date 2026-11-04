import { runMain } from '@effect/platform-node/NodeRuntime';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { Catalog } from '@rasm/database';
import { serveNode } from '@rasm/hosting/node';
import { ObjectStorage } from '@rasm/hosting/server';
import { Layer } from 'effect';
import config from '#/cloudflare.config.ts';
import { app } from '#/server/app.ts';
import { catalog } from '#/server/catalog.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

serveNode(config.worker({ mode: 'production', isPreview: false }), (env) =>
    app(
        env.OWNER_EMAIL,
        Layer.mergeAll(
            Catalog.layer(catalog, (options) => SqliteClient.layer({ filename: env.DB, ...options })),
            ObjectStorage.s3(env.BUCKET, { forcePathStyle: true }),
        ),
    ),
).pipe(Layer.launch, runMain);
