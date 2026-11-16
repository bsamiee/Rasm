import { Catalog } from '@rasm/database';
import { d1Engine } from '@rasm/database/d1';
import { r2Storage } from '@rasm/object-storage/r2';
import { serveWorker } from '@rasm/workers/serve';
import { env } from 'cloudflare:workers';
import { Layer } from 'effect';
import { app } from '#/server/app';
import { catalog } from '#/server/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serveWorker(env.HOSTING, (served) => app(Layer.merge(Catalog.layer(catalog, d1Engine({ db: served.DB })), r2Storage(served.BUCKET))));
