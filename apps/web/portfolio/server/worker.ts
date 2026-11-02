import { env } from 'cloudflare:workers';
import { BrowserCrypto } from '@effect/platform-browser';
import { D1Client } from '@effect/sql-d1';
import { Catalog } from '@rasm/database/catalog';
import { worker } from '@rasm/hosting/cloudflare';
import { Page } from '@rasm/hosting/page/render';
import { ObjectStorage } from '@rasm/hosting/storage';
import { Layer } from 'effect';
import { app } from '#/server/app';
import { catalog } from '#/server/store/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default worker(app(env.OWNER_EMAIL).pipe(Layer.provideMerge([Catalog.layer(catalog, (options) => D1Client.layer({ db: env.DB, ...options })), ObjectStorage.r2(env.BUCKET), BrowserCrypto.layer, Layer.succeed(Page, env.ASSETS)])));
