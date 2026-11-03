import { Effect } from 'effect';
import { Digest } from 'effect/crypto';
import { Etag } from 'effect/http';

// --- [OPERATIONS] ----------------------------------------------------------------------

const contentEtag = Effect.fn('contentEtag')((content: Uint8Array) => Digest.Digest.use((digest) => digest.digestHex(Digest.sha256, content)).pipe(Effect.map((value) => Etag.toString({ _tag: 'Strong', value }))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { contentEtag };
