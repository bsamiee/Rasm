// biome-ignore-all lint/style/noNamespace lint/style/noDefaultExport: Cloudflare declares bindings on the Cloudflare.Env namespace and loads the default Worker export
import { Layer } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';
import { HttpApiBuilder } from 'effect/http-api';
import { Api } from '../model/api.ts';
import { content } from './content.ts';
import { database } from './database.ts';
import { download, media } from './media.ts';
import { page } from './page.tsx';
import { owner } from './session.ts';

// --- [TYPES] ---------------------------------------------------------------------------

declare global {
    namespace Cloudflare {
        interface Env {
            DB: D1Database;
            BUCKET: R2Bucket;
            ASSETS: Fetcher;
            OWNER_EMAIL: string;
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const { handler } = HttpRouter.toWebHandler(Layer.mergeAll(page, download, HttpApiBuilder.layer(Api).pipe(Layer.provide([Layer.merge(content, media).pipe(Layer.provide(owner)), HttpServer.layerServices]))).pipe(Layer.provideMerge(database)));

// --- [EXPORTS] -------------------------------------------------------------------------

export default { fetch: handler };
