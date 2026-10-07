import { env } from 'cloudflare:workers';
import { Effect, Schema } from 'effect';
import { HttpRouter, type HttpServerRequest, HttpServerResponse } from 'effect/http';
import { renderToReadableStream } from 'react-dom/server';
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
            const [bootstrap, template] = yield* Effect.all([Effect.all({ initial: readDocument('published'), session }, { concurrency: 'unbounded' }), Effect.tryPromise(() => env.ASSETS.fetch(request.originalUrl))], { concurrency: 'unbounded' });
            const serialized = yield* Schema.encodeEffect(Schema.fromJsonString(Bootstrap))(bootstrap);
            const markup = yield* Effect.tryPromise((signal) => renderToReadableStream(<Site {...bootstrap} />, { signal }));
            yield* Effect.tryPromise(() => markup.allReady);
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
                            element.setInnerContent(markup, { html: true });
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
