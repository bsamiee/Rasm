import { Layer } from 'effect';
import { HttpServer } from 'effect/http';
import { HttpApiBuilder } from 'effect/http-api';
import { Api } from '#/model/api';
import { content } from '#/server/content';
import { download, media } from '#/server/media';
import { page } from '#/server/page';
import { owner } from '#/server/session';

// --- [COMPOSITION] ---------------------------------------------------------------------

const app = (ownerEmail: string) => {
    const email = ownerEmail.toLowerCase();
    return Layer.mergeAll(page(email), download(email), HttpApiBuilder.layer(Api).pipe(Layer.provide([Layer.merge(content, media).pipe(Layer.provide(owner(email))), HttpServer.layerServices])));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { app };
