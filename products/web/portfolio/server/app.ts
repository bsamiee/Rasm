import { Layer } from 'effect';
import { HttpRouter } from 'effect/http';
import { HttpApiBuilder } from 'effect/http-api';
import { Api } from '#/model/api';
import { assetGroup } from '#/server/assets';
import { documentGroup, page } from '#/server/documents';
import { OwnerAccount } from '#/server/owner';

// --- [COMPOSITION] ---------------------------------------------------------------------

const app = <A, E, R>(services: Layer.Layer<A, E, R>) => HttpApiBuilder.layer(Api).pipe(Layer.provide([documentGroup, assetGroup]), Layer.provide(OwnerAccount.middleware), Layer.merge(page), HttpRouter.provideRequest(services), Layer.provide(services));

// --- [EXPORTS] -------------------------------------------------------------------------

export { app };
