import { NodeRuntime } from '@effect/platform-node';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { Catalog } from '@rasm/database/catalog';
import { server } from '@rasm/hosting/node/server';
import { ObjectStorage } from '@rasm/hosting/storage';
import { Layer } from 'effect';
import config from '#/cloudflare.config.ts';
import { app } from '#/server/app.ts';
import { catalog } from '#/server/store/catalog.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

server(config.worker({ mode: 'production', isPreview: false }), (env) =>
    app(
        env.OWNER_EMAIL,
        Layer.mergeAll(
            Catalog.layer(catalog, (options) => SqliteClient.layer({ filename: env.DB, ...options })),
            ObjectStorage.s3(env.BUCKET, { forcePathStyle: true }),
        ),
    ),
).pipe(Layer.launch, NodeRuntime.runMain);
