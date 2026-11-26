import { Catalog } from '@rasm/database/catalog';
import { serve } from '@rasm/hosting/server';
import { Layer } from 'effect';
import hosting from '#/hosting.config';
import { app } from '#/server/app';
import { catalog } from '#/server/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serve(hosting, ({ DB, BUCKET }) => app(Layer.merge(Catalog.layer(catalog, DB), BUCKET)));
