import { BrowserCrypto } from '@effect/platform-browser';
import { NodeRuntime } from '@effect/platform-node';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { store } from '@rasm/database';
import { s3 } from '@rasm/hosting';
import { server } from '@rasm/hosting/node';
import { Layer } from 'effect';
import config from '#/cloudflare.config';
import { app } from '#/server/app';
import { catalog } from '#/server/store/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

server(config.worker({ mode: 'production', isPreview: false }), (env) => ({
    routes: app(env.OWNER_EMAIL),
    services: Layer.mergeAll(
        store(catalog, (options) => SqliteClient.layer({ filename: env.DB, ...options })),
        s3(env.BUCKET),
        BrowserCrypto.layer,
    ),
})).pipe(Layer.launch, NodeRuntime.runMain);
