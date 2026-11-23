import { Layer } from 'effect';
import { Digest, Mac, SecureRandom, Signature } from 'effect/crypto';
import { signatures } from '#/signatures';

// --- [COMPOSITION] ---------------------------------------------------------------------

signatures(Layer.mergeAll(Mac.layerWebCrypto, Signature.layerWebCrypto, Digest.layerWebCrypto).pipe(Layer.provide(SecureRandom.layerWebCrypto)));
