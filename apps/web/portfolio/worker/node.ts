import { BrowserCrypto } from '@effect/platform-browser';
import { NodeRuntime } from '@effect/platform-node';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { store } from '@rasm/database/store.ts';
import { serve } from '@rasm/hosting/node.ts';
import { s3 } from '@rasm/hosting/storage.ts';
import { Layer } from 'effect';
import config from '../cloudflare.config.ts';
import { app } from './app.ts';
import { catalog } from './database.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

serve(config.worker({ mode: 'production', isPreview: false }), (env, assets) => ({
    routes: app(assets, env.OWNER_EMAIL),
    services: Layer.mergeAll(
        store(catalog, (options) => SqliteClient.layer({ filename: env.DB, ...options })),
        s3(env.BUCKET),
        BrowserCrypto.layer,
    ),
})).pipe(Layer.launch, NodeRuntime.runMain);
