import { unavailable } from '@rasm/hosting/http.ts';
import { renderPage } from '@rasm/hosting/render.ts';
import { Effect, Option, String } from 'effect';
import { HttpRouter, HttpServerResponse } from 'effect/http';
import { shell } from '../model/document.ts';
import { root } from '../site/root.tsx';
import { readDocument } from './database.ts';
import { session } from './session.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const page = (email: string) =>
    HttpRouter.add(
        'GET',
        '/',
        Effect.fn('page')(
            function* () {
                const bootstrap = yield* Effect.all({ initial: readDocument('published').pipe(Effect.map(({ data }) => data)), session: session(email) }, { concurrency: 'unbounded' });
                const { name, introduction } = bootstrap.initial.portfolio;
                const response = yield* renderPage(shell, {
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
