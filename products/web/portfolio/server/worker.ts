import { Catalog } from '@rasm/database';
import { d1Engine } from '@rasm/database/d1';
import { r2Storage } from '@rasm/object-storage/r2';
import { serveWorker } from '@rasm/workers/serve';
import { Layer } from 'effect';
import hosting from '#/hosting.config';
import { app } from '#/server/app';
import { catalog } from '#/server/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serveWorker(hosting, (env) => app(Layer.merge(Catalog.layer(catalog, d1Engine({ db: env.DB })), r2Storage(env.BUCKET))));
