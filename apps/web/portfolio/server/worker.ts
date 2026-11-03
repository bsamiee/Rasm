import { D1Client } from '@effect/sql-d1';
import { Catalog } from '@rasm/database/catalog';
import { worker } from '@rasm/hosting/cloudflare';
import { Access } from '@rasm/hosting/cloudflare/access';
import { ObjectStorage } from '@rasm/hosting/storage';
import { Layer } from 'effect';
import config from '#/cloudflare.config.ts';
import { sites } from '#/model/sites.ts';
import { app } from '#/server/app.ts';
import { catalog } from '#/server/store/catalog.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default worker(config.worker({ mode: 'production', isPreview: false }), (env) =>
    Layer.merge(
        app(
            env.OWNER_EMAIL,
            Layer.mergeAll(
                Catalog.layer(catalog, (options) => D1Client.layer({ db: env.DB, ...options })),
                ObjectStorage.r2(env.BUCKET),
            ),
        ),
        Access.middleware(sites),
    ),
);
