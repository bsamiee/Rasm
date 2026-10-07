import { env } from 'cloudflare:workers';
import { D1Client } from '@effect/sql-d1';
import { Effect, Layer, Option } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import { Owner } from '../model/api.ts';
import type { Session } from '../model/document.ts';
import { ownerUserId, registerOwner } from './database.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const session = Effect.gen(function* () {
    const { headers } = yield* HttpServerRequest.HttpServerRequest;
    const email = env.OWNER_EMAIL.toLowerCase();
    return yield* Option.match(Option.fromNullishOr(headers['oai-authenticated-user-id']), {
        onNone: () => Effect.succeed<typeof Session.Type>({ kind: 'visitor', signedIn: false }),
        onSome: (userId) =>
            ownerUserId(email).pipe(
                Effect.flatMap((registered) => (Option.isNone(registered) && headers['oai-authenticated-user-email']?.toLowerCase() === email ? registerOwner(email, userId) : Effect.succeed(registered))),
                Effect.map(Option.exists((registered) => registered.userId === userId)),
                Effect.map((owned): typeof Session.Type => (owned ? { kind: 'owner' } : { kind: 'visitor', signedIn: true })),
            ),
    });
}).pipe(Effect.withSpan('session'));
const authorize = Effect.filterOrFail(
    session,
    (current) => current.kind === 'owner',
    () => new HttpApiError.Forbidden(),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const owner = Layer.effect(
    Owner,
    Effect.map(
        D1Client.D1Client,
        (sql) =>
            (effect): ReturnType<typeof Owner.Service> =>
                HttpServerRequest.HttpServerRequest.pipe(
                    Effect.filterOrFail(
                        (request) => request.method === 'GET' || request.headers.origin === new URL(request.originalUrl).origin,
                        () => new HttpApiError.Forbidden(),
                    ),
                    Effect.andThen(authorize),
                    Effect.provideService(D1Client.D1Client, sql),
                    Effect.andThen(effect),
                    Effect.map(HttpServerResponse.setHeader('Cache-Control', 'private, no-store')),
                ),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { authorize, owner, session };
