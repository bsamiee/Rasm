import { RegistryProvider } from '@effect/atom-react';
import type { D1Client } from '@effect/sql-d1';
import { unavailable } from '@rasm/hosting/http.ts';
import { fetchTemplate, renderPage } from '@rasm/hosting/render.ts';
import { type Crypto, Effect, type Layer, Option } from 'effect';
import { HttpRouter, type HttpServerRequest, HttpServerResponse } from 'effect/http';
import type { HttpApiError } from 'effect/http-api';
import { shell } from '../model/document.ts';
import { Site } from '../site/site.tsx';
import { readDocument } from './database.ts';
import { session } from './session.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const page = (assets: Fetcher, email: string): Layer.Layer<never, never, HttpRouter.HttpRouter | HttpRouter.Request.From<'Requires', D1Client.D1Client | Crypto.Crypto> | HttpRouter.Request.From<'Error', HttpApiError.ServiceUnavailable>> =>
    HttpRouter.add(
        'GET',
        '/',
        Effect.fn('page')(
            function* (request: HttpServerRequest.HttpServerRequest) {
                const [bootstrap, template] = yield* Effect.all([Effect.all({ initial: readDocument('published').pipe(Effect.map(({ data }) => data)), session: session(email) }, { concurrency: 'unbounded' }), fetchTemplate(assets, request.originalUrl)], { concurrency: 'unbounded' });
                const { name, introduction } = bootstrap.initial.portfolio;
                const response = yield* renderPage(shell, {
                    template,
                    value: bootstrap,
                    element: (
                        <RegistryProvider>
                            <Site {...bootstrap} />
                        </RegistryProvider>
                    ),
                    title: Option.map(
                        Option.liftPredicate(name, (text) => text !== ''),
                        (text) => `${text} — Architecture`,
                    ),
                    description: Option.liftPredicate(introduction, (text) => text !== ''),
                });
                return HttpServerResponse.raw(response, { headers: { 'Cache-Control': 'private, no-store' } });
            },
            Effect.catchTag(['RenderError', 'SchemaError'], unavailable),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { page };
