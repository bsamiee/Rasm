import { Catalog } from '@rasm/database';
import { serve } from '@rasm/hosting/server';
import { Layer } from 'effect';
import hosting from '#/hosting.config.ts';
import { app } from '#/server/app.ts';
import { catalog } from '#/server/catalog.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serve(hosting, ({ DB, BUCKET }) => app(Layer.merge(Catalog.layer(catalog, DB), BUCKET)));
