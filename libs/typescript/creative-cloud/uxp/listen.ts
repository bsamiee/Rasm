import { Unmatched } from '@rasm/search';
import { Array, Cause, Data, Deferred, Duration, Effect, Latch, Layer, Option, type Predicate, Schedule, Stream, Struct, SubscriptionRef } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { type Rpc, RpcClient, type RpcClientError, type RpcGroup, type RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { ORIGIN, Peer } from '#/link';

// --- [OPERATIONS] ----------------------------------------------------------------------

const listen = Effect.fn('listen')(function* <Rpcs extends Rpc.Any>(
    group: RpcGroup.RpcGroup<Rpcs>,
    {
        path,
        serialization,
        middleware,
    }: {
        readonly path: HttpRouter.PathInput;
        readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization>;
        readonly middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol>;
    },
) {
    class Linked extends Data.Class<{ readonly peer: typeof Peer.Type; readonly client: RpcClient.RpcClient.Flat<Rpcs, RpcClientError.RpcClientError> }> {}
    const [router, links] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make(Array.empty<Linked>())]);
    const linking = Effect.fnUntraced(
        function* (request: HttpServerRequest.HttpServerRequest) {
            const [found, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(Peer), request.upgrade, Deferred.make<Linked>(), Latch.make()]);
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
            yield* Deferred.succeed(made, new Linked({ peer: found, client }));
            return yield* Effect.as(ended.await, HttpServerResponse.empty());
        },
        Effect.orElseSucceed(() => HttpServerResponse.empty({ status: 400 })),
    );
    yield* router.add('GET', path, (request) => (request.headers.origin === ORIGIN ? linking(request) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
    return {
        peers: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))),
        changes: Stream.map(SubscriptionRef.changes(links), Array.map(Struct.get('peer'))),
        call: (serves: Predicate.Predicate<typeof Peer.Type>) =>
            SubscriptionRef.get(links).pipe(
                Effect.map(Array.filter(({ peer }) => serves(peer))),
                Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new Cause.NoSuchElementError('No linked extension serves the application')),
                Effect.flatMap((serving) => Unmatched.single('linked extension serving the application', Option.none())(serving, ({ peer }) => [`${peer.extension} in ${peer.application.href}`])),
                Effect.map(Struct.get('client')),
            ),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { listen };
