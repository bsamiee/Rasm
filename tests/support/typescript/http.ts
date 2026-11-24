import { Effect, Layer, type Scope } from 'effect';
import { FetchHttpClient, HttpEffect, type HttpServerRequest, type HttpServerResponse } from 'effect/http';

// --- [OPERATIONS] ----------------------------------------------------------------------

const transport = <E>(app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, HttpServerRequest.HttpServerRequest | Scope.Scope>) => {
    const fetching = Effect.map(Effect.context<never>(), (context) => {
        const served = HttpEffect.toWebHandlerWith<never, HttpServerRequest.HttpServerRequest | Scope.Scope>(context)(app);
        return (...request: Parameters<typeof fetch>) => served(new Request(...request));
    });
    return Layer.provide(FetchHttpClient.layer, Layer.effect(FetchHttpClient.Fetch, fetching));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { transport };
