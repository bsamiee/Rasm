import { Enrollment } from '@rasm/hosting/identity';
import { Effect, Equal, Layer } from 'effect';
import type { HttpServerRequest } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import { Owner } from '#/model/api';

// --- [OPERATIONS] ----------------------------------------------------------------------

const authorize = Effect.filterOrFail(Enrollment.session, Equal.equals('allowed'), () => HttpApiError.Forbidden.make());

// --- [COMPOSITION] ---------------------------------------------------------------------

const OwnerAccount = {
    authorize,
    middleware: Layer.effect(
        Owner,
        Effect.gen(function* () {
            const context = yield* Effect.context<Exclude<Effect.Services<typeof authorize>, HttpServerRequest.HttpServerRequest>>();
            return (effect): ReturnType<typeof Owner.Service> => Enrollment.sameOrigin.pipe(Effect.andThen(authorize), Effect.provideContext(context), Effect.andThen(effect), Effect.map(Enrollment.unshared));
        }),
    ),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { OwnerAccount };
