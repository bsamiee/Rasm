import { D1Client } from '@effect/sql-d1';
import { Catalog } from '@rasm/database';
import { CloudflareAccess } from '@rasm/hosting';
import { serveWorker } from '@rasm/hosting/cloudflare';
import { ObjectStorage } from '@rasm/hosting/server';
import { Layer } from 'effect';
import config from '#/cloudflare.config.ts';
import { sites } from '#/model/sites.ts';
import { app } from '#/server/app.ts';
import { catalog } from '#/server/catalog.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serveWorker(config.worker({ mode: 'production', isPreview: false }), (env) =>
    app(
        env.OWNER_EMAIL,
        Layer.mergeAll(
            Catalog.layer(catalog, (options) => D1Client.layer({ db: env.DB, ...options })),
            ObjectStorage.r2(env.BUCKET),
        ),
    ).pipe(Layer.merge(CloudflareAccess.middleware(sites))),
);
