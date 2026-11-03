import { Layer } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpRouter } from 'effect/http';
import { HttpApiBuilder } from 'effect/http-api';
import { Api } from '#/model/api.ts';
import { assetGroup } from '#/server/assets.ts';
import { documentGroup } from '#/server/documents.ts';
import { OwnerAccount } from '#/server/owner.ts';
import { page } from '#/server/page.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const app = <A, E, R>(ownerEmail: string, services: Layer.Layer<A, E, R>) => {
    const provided = Layer.mergeAll(services, OwnerAccount.layer(ownerEmail), SecureRandom.layerWebCrypto);
    return Layer.merge(page, HttpApiBuilder.layer(Api).pipe(Layer.provide(Layer.merge(documentGroup, assetGroup).pipe(Layer.provide(OwnerAccount.middleware))))).pipe(HttpRouter.provideRequest(provided), Layer.provide(provided));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { app };
