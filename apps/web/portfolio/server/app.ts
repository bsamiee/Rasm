import { Layer } from 'effect';
import { HttpServer } from 'effect/http';
import { HttpApiBuilder } from 'effect/http-api';
import { Api } from '../model/api.ts';
import { content } from './content.ts';
import { download, media } from './media.ts';
import { page } from './page.ts';
import { owner } from './session.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const app = (ownerEmail: string) => {
    const email = ownerEmail.toLowerCase();
    return Layer.mergeAll(page(email), download(email), HttpApiBuilder.layer(Api).pipe(Layer.provide([Layer.merge(content, media).pipe(Layer.provide(owner(email))), HttpServer.layerServices])));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { app };
