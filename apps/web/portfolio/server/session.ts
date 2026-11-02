import { User } from '@rasm/hosting/user';
import { Effect, Layer, Option } from 'effect';
import { HttpMethod, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import { SqlClient } from 'effect/sql';
import { Owner } from '#/model/api';
import type { Session } from '#/model/bootstrap';
import { ownerUserId, registerOwner } from '#/server/store/owner';

// --- [OPERATIONS] ----------------------------------------------------------------------

const enrolled = Effect.fnUntraced(function* (email: string, { id, email: address }: User) {
    const ownerId = yield* ownerUserId(email);
    return Option.isNone(ownerId) && Option.exists(address, (value) => value.toLowerCase() === email) ? Option.some(yield* registerOwner({ email, userId: id })) : ownerId;
});
const session = Effect.fn('session')((email: string) =>
    User.current.pipe(
        Effect.flatMap(
            Option.match({
                onNone: () => Effect.succeed<typeof Session.Type>('signedOut'),
                onSome: (user) => Effect.map(enrolled(email, user), (ownerId): typeof Session.Type => (Option.contains(ownerId, user.id) ? 'owner' : 'signedIn')),
            }),
        ),
    ),
);
const authorize = (email: string) =>
    Effect.filterOrFail(
        session(email),
        (current) => current === 'owner',
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
                            ({ method, headers }) => !HttpMethod.hasBody(method) || headers['sec-fetch-site'] === 'same-origin',
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
