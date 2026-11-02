import { Page } from '@rasm/hosting/page/render';
import { unavailable } from '@rasm/hosting/response';
import { Effect, Struct } from 'effect';
import { HttpRouter, HttpServerResponse } from 'effect/http';
import { session } from '#/server/session';
import { readDocument } from '#/server/store/documents';
import { shell } from '#/site/shell';

// --- [COMPOSITION] ---------------------------------------------------------------------

const page = (email: string) =>
    HttpRouter.add(
        'GET',
        '/',
        Effect.fn('page')(
            () => Effect.all({ initial: Effect.map(readDocument('published'), Struct.get('data')), session: session(email) }, { concurrency: 'unbounded' }),
            Effect.flatMap((bootstrap) => Page.render(shell, bootstrap)),
            Effect.map(HttpServerResponse.setHeader('Cache-Control', 'private, no-store')),
            Effect.catchTag('UnknownError', unavailable),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { page };
