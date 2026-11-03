import { Page } from '@rasm/hosting/page/render';
import { unavailable } from '@rasm/hosting/unavailable';
import { Effect, Struct } from 'effect';
import { HttpRouter } from 'effect/http';
import { readDocument } from '#/server/documents.ts';
import { OwnerAccount } from '#/server/owner.ts';
import { shell } from '#/site/shell.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const page = HttpRouter.add(
    'GET',
    '/',
    Effect.fn('page')(
        () => Effect.all({ published: Effect.map(readDocument('published'), Struct.get('data')), session: OwnerAccount.session }, { concurrency: 'unbounded' }),
        Effect.flatMap((bootstrap) => Page.render(shell, bootstrap)),
        Effect.map(OwnerAccount.sessionScoped),
        Effect.catchTag('UnknownError', unavailable),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { page };
