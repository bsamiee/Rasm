import { Crypto, Effect, type PlatformError } from 'effect';
import { Hex } from 'effect/encoding';
import { Etag } from 'effect/http';

// --- [OPERATIONS] ----------------------------------------------------------------------

const contentEtag = (body: string): Effect.Effect<string, PlatformError.PlatformError, Crypto.Crypto> => Crypto.Crypto.use((crypto) => crypto.digest('SHA-256', new TextEncoder().encode(body))).pipe(Effect.map((digest) => Etag.toString({ _tag: 'Strong', value: Hex.encode(digest) })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { contentEtag };
