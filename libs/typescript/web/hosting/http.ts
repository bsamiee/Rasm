import { type Cause, Crypto, Effect, type PlatformError } from 'effect';
import { Hex } from 'effect/encoding';
import { Etag } from 'effect/http';
import { HttpApiError } from 'effect/http-api';

// --- [OPERATIONS] ----------------------------------------------------------------------

const contentEtag = (body: string): Effect.Effect<string, PlatformError.PlatformError, Crypto.Crypto> => Crypto.Crypto.use((crypto) => crypto.digest('SHA-256', new TextEncoder().encode(body))).pipe(Effect.map((digest) => Etag.toString({ _tag: 'Strong', value: Hex.encode(digest) })));
const unavailable = (error: Cause.YieldableError): Effect.Effect<never, HttpApiError.ServiceUnavailable> => Effect.andThen(Effect.logError(error), Effect.fail(HttpApiError.ServiceUnavailable.make()));

// --- [EXPORTS] -------------------------------------------------------------------------

export { contentEtag, unavailable };
