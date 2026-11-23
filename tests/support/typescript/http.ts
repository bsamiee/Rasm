import { Effect, Layer, type Scope } from 'effect';
import { FetchHttpClient, HttpEffect, type HttpServerRequest, type HttpServerResponse } from 'effect/http';

// --- [OPERATIONS] ----------------------------------------------------------------------

const transport = <E>(app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, HttpServerRequest.HttpServerRequest | Scope.Scope>) =>
    FetchHttpClient.layer.pipe(
        Layer.provide(
            Layer.effect(
                FetchHttpClient.Fetch,
                Effect.map(Effect.context<never>(), (context) => {
                    const served = HttpEffect.toWebHandlerWith<never, HttpServerRequest.HttpServerRequest | Scope.Scope>(context)(app);
                    return (input, init) => served(new Request(input, init));
                }),
            ),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { transport };
