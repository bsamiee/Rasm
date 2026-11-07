import { Layer } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpRouter } from 'effect/http';
import { HttpApiBuilder } from 'effect/http-api';
import { Api } from '#/model/api';
import { assetGroup } from '#/server/assets';
import { documentGroup } from '#/server/documents';
import { OwnerAccount } from '#/server/owner';
import { page } from '#/server/page';

// --- [COMPOSITION] ---------------------------------------------------------------------

const app = <A, E, R>(ownerEmail: string, services: Layer.Layer<A, E, R>) => {
    const provided = Layer.mergeAll(services, Layer.succeed(OwnerAccount, { email: ownerEmail }), SecureRandom.layerWebCrypto);
    return HttpApiBuilder.layer(Api).pipe(Layer.provide([documentGroup, assetGroup]), Layer.provide(OwnerAccount.middleware), Layer.merge(page), HttpRouter.provideRequest(provided), Layer.provide(provided));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { app };
