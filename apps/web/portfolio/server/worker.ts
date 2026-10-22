import { BrowserCrypto } from '@effect/platform-browser';
import { D1Client } from '@effect/sql-d1';
import { store } from '@rasm/database/store.ts';
import { worker } from '@rasm/hosting/cloudflare/worker.ts';
import { StaticAssets } from '@rasm/hosting/rendering/server.ts';
import { r2 } from '@rasm/hosting/storage.ts';
import { Layer } from 'effect';
import { app } from './app.ts';
import { catalog } from './store/catalog.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default worker((bindings: Env) => app(bindings.OWNER_EMAIL).pipe(Layer.provideMerge([store(catalog, (options) => D1Client.layer({ db: bindings.DB, ...options })), r2(bindings.BUCKET), BrowserCrypto.layer, Layer.succeed(StaticAssets, bindings.ASSETS)])));
