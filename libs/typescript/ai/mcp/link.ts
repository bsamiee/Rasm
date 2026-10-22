import { Unmatched } from '@rasm/search';
import { Array, Cause, Data, Deferred, Duration, Effect, flow, Latch, Layer, Option, Schedule, type Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse, UrlParams } from 'effect/http';
import { type Rpc, RpcClient, type RpcClientError, type RpcGroup, type RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [OPERATIONS] ----------------------------------------------------------------------

const listen = Effect.fn('listen')(function* <Rpcs extends Rpc.Any, Peer, I extends Readonly<Record<string, string | undefined>>, Chosen>(
    group: RpcGroup.RpcGroup<Rpcs>,
    {
        path,
        origin,
        identity,
        serialization,
        middleware,
        serves,
    }: {
        readonly path: HttpRouter.PathInput;
        readonly origin: string;
        readonly identity: Schema.Codec<Peer, I>;
        readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization>;
        readonly middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol>;
        readonly serves: (chosen: Chosen) => (peer: Peer) => boolean;
    },
) {
    class Linked extends Data.Class<{ readonly peer: Peer; readonly query: string; readonly client: RpcClient.RpcClient.Flat<Rpcs, RpcClientError.RpcClientError> }> {}
    const [router, links] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make(Array.empty<Linked>())]);
    const linking = Effect.fnUntraced(
        function* (request: HttpServerRequest.HttpServerRequest) {
            const [found, query, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(identity), Effect.map(HttpServerRequest.ParsedSearchParams, flow(UrlParams.fromInput, UrlParams.toString)), request.upgrade, Deferred.make<Linked>(), Latch.make()]);
            const protocol = yield* RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) }).pipe(
                Effect.provideService(RpcClient.ConnectionHooks, {
                    onConnect: Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.prepend(entry))),
                    onDisconnect: Effect.andThen(
                        Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.difference([entry]))),
                        ended.open,
                    ),
                }),
                Effect.provideService(Socket.Socket, socket),
                Effect.provide(serialization),
            );
            const clients = yield* Layer.build(Layer.provideMerge(middleware, Layer.succeed(RpcClient.Protocol, protocol)));
            const client = yield* RpcClient.make(group, { flatten: true }).pipe(Effect.provideContext(clients));
            yield* Deferred.succeed(made, new Linked({ peer: found, query, client }));
            return yield* Effect.as(ended.await, HttpServerResponse.empty());
        },
        Effect.orElseSucceed(() => HttpServerResponse.empty({ status: 400 })),
    );
    yield* router.add('GET', path, (request) => (request.headers.origin === origin ? linking(request) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
    return {
        peers: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))),
        changes: Stream.map(SubscriptionRef.changes(links), Array.map(Struct.get('peer'))),
        call: (chosen: Chosen) =>
            SubscriptionRef.get(links).pipe(
                Effect.map(Array.filter(({ peer }) => serves(chosen)(peer))),
                Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new Cause.NoSuchElementError('No linked extension serves the application')),
                Effect.flatMap((serving) => Unmatched.single('linked extension serving the application', Option.none())(serving, ({ query }) => [query])),
                Effect.map(Struct.get('client')),
            ),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { listen };
