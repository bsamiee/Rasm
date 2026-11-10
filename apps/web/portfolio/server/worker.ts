import { env } from 'cloudflare:workers';
import { Catalog } from '@rasm/database';
import { d1Engine } from '@rasm/database/d1';
import { Sites } from '@rasm/hosting/sites';
import { Template } from '@rasm/hosting/template';
import { CloudflareAccess, r2Storage, serveWorker } from '@rasm/hosting/worker';
import { Layer } from 'effect';
import { app } from '#/server/app';
import { catalog } from '#/server/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serveWorker(env, Layer.mergeAll(app(env.OWNER_EMAIL, Layer.mergeAll(Catalog.layer(catalog, d1Engine({ db: env.DB })), r2Storage(env.BUCKET))), Template.layer(env.ASSETS, import.meta.env.PROD), CloudflareAccess.middleware(Sites.identity)));
