import { BrowserCrypto } from '@effect/platform-browser';
import { D1Client } from '@effect/sql-d1';
import { Function, Layer, String } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';
import { HttpApiBuilder } from 'effect/http-api';
import { Api } from '../model/api.ts';
import { content } from './content.ts';
import { database } from './database.ts';
import { download, media } from './media.ts';
import { page } from './page.tsx';
import { owner } from './session.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Bindings {
    DB: D1Database;
    BUCKET: R2Bucket;
    ASSETS: Fetcher;
    OWNER_EMAIL: string;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const handler = Function.memoize((bindings: Bindings) => {
    const email = bindings.OWNER_EMAIL.toLowerCase();
    return HttpRouter.toWebHandler(
        Layer.mergeAll(page(bindings.ASSETS, email), download(bindings.BUCKET, email), HttpApiBuilder.layer(Api).pipe(Layer.provide([Layer.merge(content, media(bindings.BUCKET)).pipe(Layer.provide(owner(email))), HttpServer.layerServices]))).pipe(
            Layer.provideMerge(database.pipe(Layer.provideMerge(D1Client.layer({ db: bindings.DB, transformResultNames: String.snakeToCamel })))),
            Layer.provideMerge(BrowserCrypto.layer),
        ),
    ).handler;
});

// --- [EXPORTS] -------------------------------------------------------------------------

// biome-ignore lint/style/noDefaultExport: Cloudflare invokes the Worker module's default-exported fetch handler
export default {
    fetch(request: Request, bindings: Bindings): Promise<Response> {
        return handler(bindings)(request);
    },
};
