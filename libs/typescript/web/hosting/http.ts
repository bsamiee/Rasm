import { Cause, Crypto, Effect } from 'effect';
import { Hex } from 'effect/encoding';
import { Etag } from 'effect/http';
import { HttpApiError } from 'effect/http-api';

// --- [OPERATIONS] ----------------------------------------------------------------------

const contentEtag = Effect.fn('contentEtag')((body: string) => Crypto.Crypto.use((crypto) => crypto.digest('SHA-256', new TextEncoder().encode(body))).pipe(Effect.map((digest) => Etag.toString({ _tag: 'Strong', value: Hex.encode(digest) }))));
const unavailable = Effect.fn('unavailable')((error: Cause.YieldableError) => Effect.andThen(Effect.logError('service unavailable', Cause.fail(error)), Effect.fail(HttpApiError.ServiceUnavailable.make())));

// --- [EXPORTS] -------------------------------------------------------------------------

export { contentEtag, unavailable };
