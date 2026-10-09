import { RegistryProvider } from '@effect/atom-react';
import type { D1Client } from '@effect/sql-d1';
import { Effect, type Layer, Schema } from 'effect';
import { HttpRouter, type HttpServerRequest, HttpServerResponse } from 'effect/http';
import type { HttpApiError } from 'effect/http-api';
import { renderToReadableStream } from 'react-dom/server';
import { Bootstrap } from '../model/document.ts';
import { Site } from '../site/site.tsx';
import { readDocument, unavailable } from './database.ts';
import { session } from './session.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const page = (assets: Fetcher, email: string): Layer.Layer<never, never, HttpRouter.HttpRouter | HttpRouter.Request.From<'Requires', D1Client.D1Client> | HttpRouter.Request.From<'Error', HttpApiError.ServiceUnavailable>> =>
    HttpRouter.add(
        'GET',
        '/',
        Effect.fn('page')(
            function* (request: HttpServerRequest.HttpServerRequest) {
                const [bootstrap, template] = yield* Effect.all([Effect.all({ initial: readDocument('published').pipe(Effect.map(({ data }) => data)), session: session(email) }, { concurrency: 'unbounded' }), Effect.tryPromise(() => assets.fetch(request.originalUrl))], { concurrency: 'unbounded' });
                const serialized = yield* Schema.encodeEffect(Schema.fromJsonString(Bootstrap))(bootstrap);
                const markup = yield* Effect.tryPromise((signal) =>
                    renderToReadableStream(
                        <RegistryProvider>
                            <Site {...bootstrap} />
                        </RegistryProvider>,
                        { signal },
                    ),
                );
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
