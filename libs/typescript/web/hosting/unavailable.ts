import { Cause, Effect } from 'effect';
import { HttpApiError } from 'effect/http-api';

// --- [OPERATIONS] ----------------------------------------------------------------------

const unavailable = Effect.fn('unavailable')((error: Cause.YieldableError) => Effect.andThen(Effect.logError(Cause.fail(error)), Effect.fail(HttpApiError.ServiceUnavailable.make())));

// --- [EXPORTS] -------------------------------------------------------------------------

export { unavailable };
