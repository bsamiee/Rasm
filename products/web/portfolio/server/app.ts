import { Layer } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpRouter } from 'effect/http';
import { HttpApiBuilder } from 'effect/http-api';
import { Api } from '#/model/api.ts';
import { assetGroup } from '#/server/assets.ts';
import { documentGroup, page } from '#/server/documents.ts';
import { OwnerAccount } from '#/server/owner.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const app = <A, E, R>(services: Layer.Layer<A, E, R>) => {
    const provided = Layer.merge(services, SecureRandom.layerWebCrypto);
    return HttpApiBuilder.layer(Api).pipe(Layer.provide([documentGroup, assetGroup]), Layer.provide(OwnerAccount.middleware), Layer.merge(page), HttpRouter.provideRequest(provided), Layer.provide(provided));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { app };
