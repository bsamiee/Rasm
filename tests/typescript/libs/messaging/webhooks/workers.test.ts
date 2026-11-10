import { Layer } from 'effect';
import { Digest, Mac, SecureRandom, Signature } from 'effect/crypto';
import { cases } from '#/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(Layer.mergeAll(Mac.layerWebCrypto, Signature.layerWebCrypto, Digest.layerWebCrypto).pipe(Layer.provide(SecureRandom.layerWebCrypto)));
