import { Relation } from '@rasm/database';
import { Enrollment } from '@rasm/hosting/identity';
import { Effect, Layer } from 'effect';
import type { HttpServerRequest } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import { Owner } from '#/model/api';
import { owner, unavailable } from '#/server/catalog';

// --- [OPERATIONS] ----------------------------------------------------------------------

const session = Enrollment.session({ enrolled: (addresses) => Relation.select(owner, { email: addresses }), enroll: (entry) => Relation.ensure(owner, [entry]) }).pipe(Effect.catchTag(['SqlError', 'SchemaError'], unavailable));
const authorize = Effect.filterOrFail(
    session,
    (current) => current === 'allowed',
    () => HttpApiError.Forbidden.make(),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const OwnerAccount = {
    session,
    authorize,
    middleware: Layer.effect(
        Owner,
        Effect.map(
            Effect.context<Exclude<Effect.Services<typeof authorize>, HttpServerRequest.HttpServerRequest>>(),
            (context) =>
                (effect): ReturnType<typeof Owner.Service> =>
                    Enrollment.sameOrigin.pipe(Effect.andThen(authorize), Effect.provideContext(context), Effect.andThen(effect), Effect.map(Enrollment.unshared)),
        ),
    ),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { OwnerAccount };
