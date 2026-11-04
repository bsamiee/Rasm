import { unavailable } from '@rasm/hosting';
import { Template } from '@rasm/hosting/server';
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
        Effect.flatMap((bootstrap) => Template.render(shell, bootstrap)),
        Effect.map(OwnerAccount.sessionScoped),
        Effect.catchTag('HostingError', unavailable),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { page };
