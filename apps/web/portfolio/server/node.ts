import { NodeCrypto, NodeRuntime } from '@effect/platform-node';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { Catalog } from '@rasm/database/catalog';
import { server } from '@rasm/hosting/node/server';
import { ObjectStorage } from '@rasm/hosting/storage';
import { Layer } from 'effect';
import config from '#/cloudflare.config';
import { app } from '#/server/app';
import { catalog } from '#/server/store/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

server(config.worker({ mode: 'production', isPreview: false }), (env) => app(env.OWNER_EMAIL).pipe(Layer.provideMerge([Catalog.layer(catalog, (options) => SqliteClient.layer({ filename: env.DB, ...options })), ObjectStorage.s3(env.BUCKET, { forcePathStyle: true }), NodeCrypto.layer]))).pipe(
    Layer.launch,
    NodeRuntime.runMain,
);
