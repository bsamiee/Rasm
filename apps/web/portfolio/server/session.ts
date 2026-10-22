import { user } from '@rasm/hosting/authentication/sites.ts';
import { Effect, Layer, Option } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import { SqlClient } from 'effect/sql';
import { Owner } from '../model/api.ts';
import type { Session } from '../model/shell.ts';
import { ownerUserId, registerOwner } from './store/owner.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const session = Effect.fn('session')(function* (email: string) {
    return yield* Option.match(user(yield* HttpServerRequest.HttpServerRequest), {
        onNone: () => Effect.succeed<typeof Session.Type>({ kind: 'visitor', signedIn: false }),
        onSome: ({ id, email: address }) =>
            (Option.exists(address, (value) => value.toLowerCase() === email) ? Effect.asSome(registerOwner({ email, userId: id })) : ownerUserId(email)).pipe(Effect.map((ownerId): typeof Session.Type => (Option.contains(ownerId, id) ? { kind: 'owner' } : { kind: 'visitor', signedIn: true }))),
    });
});
const authorize = (email: string) =>
    Effect.filterOrFail(
        session(email),
        (current) => current.kind === 'owner',
        () => HttpApiError.Forbidden.make(),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const owner = (email: string) =>
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
