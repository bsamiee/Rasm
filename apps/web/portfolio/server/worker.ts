import { env } from 'cloudflare:workers';
import { Catalog } from '@rasm/database';
import { D1 } from '@rasm/database/d1';
import { Sites } from '@rasm/hosting/sites';
import { Template } from '@rasm/hosting/template';
import { CloudflareAccess, r2Storage, serveWorker } from '@rasm/hosting/worker';
import { Effect, Layer, Logger, Option, Record } from 'effect';
import { app } from '#/server/app';
import { catalog } from '#/server/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serveWorker(
    env,
    Effect.fromOption(Option.all({ owner: Record.get(env, 'OWNER_EMAIL'), db: Record.get(env, 'DB'), bucket: Record.get(env, 'BUCKET'), assets: Record.get(env, 'ASSETS') })).pipe(
        Effect.map(({ owner, db, bucket, assets }) => Layer.mergeAll(app(owner, Layer.mergeAll(Catalog.layer(catalog, D1.layer({ db })), r2Storage(bucket))), Template.layer(assets, import.meta.env.PROD), CloudflareAccess.middleware(Sites.identity))),
        Layer.unwrap,
        Layer.provideMerge(Logger.layer([Logger.withLeveledConsole(Logger.formatStructured)])),
    ),
);
