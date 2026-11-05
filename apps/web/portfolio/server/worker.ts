import { env } from 'cloudflare:workers';
import { D1Client } from '@effect/sql-d1';
import { Catalog } from '@rasm/database';
import { CloudflareAccess } from '@rasm/hosting/cloudflare-access';
import { ObjectStorage, Template } from '@rasm/hosting/server';
import { serveWorker } from '@rasm/hosting/worker';
import { Layer } from 'effect';
import { app } from '#/server/app.ts';
import { catalog } from '#/server/catalog.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serveWorker(
    env,
    Layer.mergeAll(
        app(
            env.OWNER_EMAIL,
            Layer.mergeAll(
                Catalog.layer(catalog, (options) => D1Client.layer({ db: env.DB, ...options })),
                ObjectStorage.r2(env.BUCKET),
            ),
        ),
        Template.layer(env.ASSETS),
        CloudflareAccess.middleware,
    ),
);
