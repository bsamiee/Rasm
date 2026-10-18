import { Effect, Layer } from 'effect';
import { HttpServer } from 'effect/http';
import { HttpApiBuilder, HttpApiSchema } from 'effect/http-api';
import { Api } from '../model/api.ts';
import { readDocument, writeDocument } from './database.ts';
import { download, media } from './media.ts';
import { page } from './page.tsx';
import { owner } from './session.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const content = HttpApiBuilder.group(Api, 'content', (handlers) => {
    const withEtag = Effect.map((revision: number) => HttpApiSchema.withHeaders({ body: undefined, headers: { 'draft-etag': revision } }));
    return handlers.handleAll({
        draft: () => readDocument('draft').pipe(Effect.map(({ data, revision }) => HttpApiSchema.withHeaders({ body: data, headers: { 'draft-etag': revision } }))),
        save: ({ payload, headers }) => withEtag(writeDocument(payload, false, headers['if-match'])),
        publish: ({ payload, headers }) => withEtag(writeDocument(payload, true, headers['if-match'])),
    });
});
const app = (ownerEmail: string) => {
    const email = ownerEmail.toLowerCase();
    return Layer.mergeAll(page(email), download(email), HttpApiBuilder.layer(Api).pipe(Layer.provide([Layer.merge(content, media).pipe(Layer.provide(owner(email))), HttpServer.layerServices])));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { app };
