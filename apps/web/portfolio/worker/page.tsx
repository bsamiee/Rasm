import type { D1Client } from '@effect/sql-d1';
import { unavailable } from '@rasm/hosting/http.ts';
import { renderPage } from '@rasm/hosting/render.ts';
import { type Crypto, Effect, type Layer, Option, String } from 'effect';
import { HttpRouter, type HttpServerRequest, HttpServerResponse } from 'effect/http';
import type { HttpApiError } from 'effect/http-api';
import { shell } from '../model/document.ts';
import { root } from '../site/root.tsx';
import { readDocument } from './database.ts';
import { session } from './session.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const page = (assets: Fetcher, email: string): Layer.Layer<never, never, HttpRouter.HttpRouter | HttpRouter.Request.From<'Requires', D1Client.D1Client | Crypto.Crypto> | HttpRouter.Request.From<'Error', HttpApiError.ServiceUnavailable>> =>
    HttpRouter.add(
        'GET',
        '/',
        Effect.fn('page')(
            function* (request: HttpServerRequest.HttpServerRequest) {
                const bootstrap = yield* Effect.all({ initial: readDocument('published').pipe(Effect.map(({ data }) => data)), session: session(email) }, { concurrency: 'unbounded' });
                const { name, introduction } = bootstrap.initial.portfolio;
                const response = yield* renderPage(shell, {
                    assets,
                    url: request.originalUrl,
                    value: bootstrap,
                    render: root,
                    title: Option.map(Option.liftPredicate(name, String.isNonEmpty), (text) => `${text} — Architecture`),
                    description: Option.liftPredicate(introduction, String.isNonEmpty),
                });
                return HttpServerResponse.raw(response, { headers: { 'Cache-Control': 'private, no-store' } });
            },
            Effect.catchTag(['RenderError', 'SchemaError'], unavailable),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { page };
