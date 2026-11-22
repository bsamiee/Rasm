import { Layer } from 'effect';
import { Digest, Mac, SecureRandom, Signature } from 'effect/crypto';
import { acceptance } from '#/acceptance.ts';
import { delivery } from '#/delivery.ts';
import { signatures } from '#/signatures.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const crypto = Layer.mergeAll(Mac.layerWebCrypto, Signature.layerWebCrypto, Digest.layerWebCrypto).pipe(Layer.provide(SecureRandom.layerWebCrypto));

signatures(crypto);
acceptance(crypto);
delivery(crypto);
