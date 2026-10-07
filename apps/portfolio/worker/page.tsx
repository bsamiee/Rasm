import { env } from 'cloudflare:workers';
import { Effect, Schema } from 'effect';
import { HttpRouter, type HttpServerRequest, HttpServerResponse } from 'effect/http';
import { renderToString } from 'react-dom/server';
import { Bootstrap } from '../model/document.ts';
import { Site } from '../site/site.tsx';
import { readDocument, unavailable } from './database.ts';
import { session } from './session.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const page = HttpRouter.add(
    'GET',
    '/',
    Effect.fn('page')(
        function* (request: HttpServerRequest.HttpServerRequest) {
            const bootstrap = yield* Effect.all({ initial: readDocument('published'), session }, { concurrency: 'unbounded' });
            const template = yield* Effect.tryPromise(() => env.ASSETS.fetch(request.originalUrl));
            const serialized = yield* Schema.encodeEffect(Schema.fromJsonString(Bootstrap))(bootstrap);
            const { name, introduction } = bootstrap.initial.portfolio;
            return HttpServerResponse.raw(
                new HTMLRewriter()
                    .on('title', {
                        element: (element): void => {
                            if (name) {
                                element.setInnerContent(`${name} — Architecture`);
                            }
                        },
                    })
                    .on('meta[name="description"]', {
                        element: (element): void => {
                            if (introduction) {
                                element.setAttribute('content', introduction);
                            }
                        },
                    })
                    .on('#portfolio', {
                        element: (element): void => {
                            element.setInnerContent(renderToString(<Site {...bootstrap} />), { html: true });
                        },
                    })
                    .on('#portfolio-data', {
                        element: (element): void => {
                            element.setInnerContent(serialized.replaceAll('<', '\\u003c'), { html: true });
                        },
                    })
                    .transform(template),
                { headers: { 'Cache-Control': 'private, no-store' } },
            );
        },
        Effect.catchTag(['UnknownError', 'SchemaError'], unavailable),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { page };
