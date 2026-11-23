import { Layer } from 'effect';
import { Digest, Mac, SecureRandom, Signature } from 'effect/crypto';
import { acceptance } from '#/acceptance';
import { delivery } from '#/delivery';
import { signatures } from '#/signatures';

// --- [COMPOSITION] ---------------------------------------------------------------------

const crypto = Layer.mergeAll(Mac.layerWebCrypto, Signature.layerWebCrypto, Digest.layerWebCrypto).pipe(Layer.provide(SecureRandom.layerWebCrypto));

signatures(crypto);
acceptance(crypto);
delivery(crypto);
