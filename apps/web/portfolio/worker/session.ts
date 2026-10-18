import { user } from '@rasm/hosting/sites.ts';
import { Effect, Layer, Option } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import { SqlClient } from 'effect/sql';
import { Owner } from '../model/api.ts';
import type { Session } from '../model/document.ts';
import { ownerUserId, registerOwner } from './database.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const session = Effect.fn('session')(function* (email: string) {
    return yield* Option.match(user(yield* HttpServerRequest.HttpServerRequest), {
        onNone: () => Effect.succeed<typeof Session.Type>({ kind: 'visitor', signedIn: false }),
        onSome: (signedIn) => {
            const matches = (id: string): boolean => id === signedIn.id;
            return (Option.exists(signedIn.email, (address) => address.toLowerCase() === email) ? registerOwner({ email, userId: signedIn.id }).pipe(Effect.map(matches)) : ownerUserId(email).pipe(Effect.map(Option.exists(matches)))).pipe(
                Effect.map((isOwner): typeof Session.Type => (isOwner ? { kind: 'owner' } : { kind: 'visitor', signedIn: true })),
            );
        },
    });
});
const authorize = (email: string): Effect.Effect<typeof Session.Type, HttpApiError.Forbidden | HttpApiError.ServiceUnavailable, HttpServerRequest.HttpServerRequest | SqlClient.SqlClient> =>
    Effect.filterOrFail(
        session(email),
        (current) => current.kind === 'owner',
        () => HttpApiError.Forbidden.make(),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const owner = (email: string): Layer.Layer<Owner, never, SqlClient.SqlClient> =>
    Layer.effect(
        Owner,
        Effect.map(
            SqlClient.SqlClient,
            (sql) =>
                (effect): ReturnType<typeof Owner.Service> =>
                    HttpServerRequest.HttpServerRequest.pipe(
                        Effect.filterOrFail(
                            (request) => request.method === 'GET' || Option.exists(HttpServerRequest.toURL(request), ({ origin }) => origin === request.headers.origin),
                            () => HttpApiError.Forbidden.make(),
                        ),
                        Effect.andThen(authorize(email)),
                        Effect.provideService(SqlClient.SqlClient, sql),
                        Effect.andThen(effect),
                        Effect.map(HttpServerResponse.setHeader('Cache-Control', 'private, no-store')),
                    ),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { authorize, owner, session };
