import { unavailable } from '@rasm/hosting/http.ts';
import { renderPage } from '@rasm/hosting/render.ts';
import { type Crypto, Effect, type Layer, Option, String } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import type { HttpApiError } from 'effect/http-api';
import type { SqlClient } from 'effect/sql';
import { shell } from '../model/document.ts';
import { root } from '../site/root.tsx';
import { readDocument } from './database.ts';
import { session } from './session.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const page = (assets: { readonly fetch: (request: Request) => Promise<Response> }, email: string): Layer.Layer<never, never, HttpRouter.HttpRouter | HttpRouter.Request.From<'Requires', SqlClient.SqlClient | Crypto.Crypto> | HttpRouter.Request.From<'Error', HttpApiError.ServiceUnavailable>> =>
    HttpRouter.add(
        'GET',
        '/',
        Effect.fn('page')(
            function* (request: HttpServerRequest.HttpServerRequest) {
                const bootstrap = yield* Effect.all({ initial: readDocument('published').pipe(Effect.map(({ data }) => data)), session: session(email) }, { concurrency: 'unbounded' });
                const { name, introduction } = bootstrap.initial.portfolio;
                const response = yield* renderPage(shell, {
                    assets,
                    request: yield* HttpServerRequest.toWeb(request),
                    value: bootstrap,
                    render: root,
                    title: Option.map(Option.liftPredicate(name, String.isNonEmpty), (text) => `${text} — Architecture`),
                    description: Option.liftPredicate(introduction, String.isNonEmpty),
                });
                return HttpServerResponse.setHeader(response, 'Cache-Control', 'private, no-store');
            },
            Effect.catchTag(['UnknownError', 'SchemaError', 'RequestParseError', 'InternalError', 'RouteNotFound'], unavailable),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { page };
