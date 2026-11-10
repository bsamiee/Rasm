import { Effect, Layer, Queue, type Schema, type Scope } from 'effect';
import { FetchHttpClient, HttpEffect, type HttpServerRequest, type HttpServerResponse } from 'effect/http';
import type { Delivery, Webhook } from '#/webhook';

// --- [OPERATIONS] ----------------------------------------------------------------------

const client = <E>(app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, HttpServerRequest.HttpServerRequest | Scope.Scope>) =>
    FetchHttpClient.layer.pipe(
        Layer.provide(
            Layer.effect(
                FetchHttpClient.Fetch,
                Effect.map(Effect.context<never>(), (context): typeof globalThis.fetch => {
                    const served = HttpEffect.toWebHandlerWith<never, HttpServerRequest.HttpServerRequest | Scope.Scope>(context)(app);
                    return (input, init) => served(new Request(input, init));
                }),
            ),
        ),
    );

const capture = Effect.fnUntraced(function* <S extends Schema.Decoder<unknown>>(webhook: Webhook<S>) {
    const deliveries = yield* Queue.unbounded<Delivery<S['Type']>>();
    return { deliveries, client: client(webhook.receive((delivery) => Queue.offer(deliveries, delivery))) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const WebhookTest = Object.assign(client, { capture });

// --- [EXPORTS] -------------------------------------------------------------------------

export { WebhookTest };
