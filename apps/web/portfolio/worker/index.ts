import { BrowserCrypto } from '@effect/platform-browser';
import { D1Client } from '@effect/sql-d1';
import { store } from '@rasm/database/store.ts';
import { Assets } from '@rasm/hosting/render.ts';
import { r2 } from '@rasm/hosting/storage.ts';
import { worker } from '@rasm/hosting/worker.ts';
import { Layer } from 'effect';
import { HttpRouter } from 'effect/http';
import { app } from './app.ts';
import { catalog } from './database.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default worker((bindings: Env) => HttpRouter.toWebHandler(app(bindings.OWNER_EMAIL).pipe(Layer.provideMerge([store(catalog, (options) => D1Client.layer({ db: bindings.DB, ...options })), r2(bindings.BUCKET), BrowserCrypto.layer, Layer.succeed(Assets, bindings.ASSETS)]))).handler);
