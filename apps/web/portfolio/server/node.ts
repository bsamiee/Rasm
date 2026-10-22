import { BrowserCrypto } from '@effect/platform-browser';
import { NodeRuntime } from '@effect/platform-node';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { store } from '@rasm/database/store.ts';
import { server } from '@rasm/hosting/node/server.ts';
import { s3 } from '@rasm/hosting/storage.ts';
import { Layer } from 'effect';
import config from '../cloudflare.config.ts';
import { app } from './app.ts';
import { catalog } from './store/catalog.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

server(config.worker({ mode: 'production', isPreview: false }), (env) => ({
    routes: app(env.OWNER_EMAIL),
    services: Layer.mergeAll(
        store(catalog, (options) => SqliteClient.layer({ filename: env.DB, ...options })),
        s3(env.BUCKET),
        BrowserCrypto.layer,
    ),
})).pipe(Layer.launch, NodeRuntime.runMain);
