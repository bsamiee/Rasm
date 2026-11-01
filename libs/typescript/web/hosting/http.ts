import { Cause, Effect } from 'effect';
import { Digest } from 'effect/crypto';
import { Etag } from 'effect/http';
import { HttpApiError } from 'effect/http-api';

// --- [OPERATIONS] ----------------------------------------------------------------------

const contentEtag = Effect.fn('contentEtag')((body: string) => Digest.Digest.use((digest) => digest.digestHex(Digest.sha256, new TextEncoder().encode(body))).pipe(Effect.map((value) => Etag.toString({ _tag: 'Strong', value }))));
const unavailable = Effect.fn('unavailable')((error: Cause.YieldableError) => Effect.andThen(Effect.logError('service unavailable', Cause.fail(error)), Effect.fail(HttpApiError.ServiceUnavailable.make())));

// --- [EXPORTS] -------------------------------------------------------------------------

export { contentEtag, unavailable };
