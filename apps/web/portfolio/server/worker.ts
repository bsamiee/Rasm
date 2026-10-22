import { BrowserCrypto } from '@effect/platform-browser';
import { D1Client } from '@effect/sql-d1';
import { store } from '@rasm/database';
import { r2, StaticAssets } from '@rasm/hosting';
import { worker } from '@rasm/hosting/cloudflare';
import { Layer } from 'effect';
import { app } from '#/server/app';
import { catalog } from '#/server/store/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default worker((bindings: Env) => app(bindings.OWNER_EMAIL).pipe(Layer.provideMerge([store(catalog, (options) => D1Client.layer({ db: bindings.DB, ...options })), r2(bindings.BUCKET), BrowserCrypto.layer, Layer.succeed(StaticAssets, bindings.ASSETS)])));
