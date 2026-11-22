import { Enrollment } from '@rasm/hosting/identity';
import { Effect, Layer } from 'effect';
import type { HttpServerRequest } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import { Owner } from '#/model/api.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const authorize = Effect.filterOrFail(
    Enrollment.session,
    (current) => current === 'allowed',
    () => HttpApiError.Forbidden.make(),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const OwnerAccount = {
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
