import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Data, Deferred, Duration, Effect, Latch, Layer, Option, type Predicate, Schedule, Schema, SchemaGetter, Stream, Struct, SubscriptionRef } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse, Url } from 'effect/http';
import { NetAddress } from 'effect/net';
import { type Rpc, RpcClient, type RpcClientError, type RpcGroup, type RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';
import { Plugin } from '#/runtime';

// --- [MODELS] --------------------------------------------------------------------------

class Link extends Schema.Class<Link, { readonly brand: unique symbol }>('Link')({
    application: Schema.URLFromString.pipe(Schema.decode({ decode: SchemaGetter.transform((url) => (url.pathname.endsWith('/') ? url : Url.setPathname(url, `${url.pathname}/`))), encode: SchemaGetter.passthrough() })).annotate({ description: 'Served application bundle URL with trailing slash' }),
    extension: Schema.String.annotate({ description: 'Plugin manifest name' }),
}) {
    static readonly dial = <Rpcs extends Rpc.Any, H, EH, RH, E, R>(
        group: RpcGroup.RpcGroup<Rpcs>,
        handlers: Layer.Layer<H, EH, RH>,
        { path, application, serialization, redial }: { readonly path: HttpRouter.PathInput; readonly application: Effect.Effect<URL, E, R>; readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization>; readonly redial: Schedule.Schedule<unknown, Cause.Cause<unknown>> },
    ) =>
        RpcServer.layer(group, { concurrency: 1 }).pipe(
            Layer.provide([handlers, RpcServer.layerProtocolSocketServer]),
            Layer.provide([
                Layer.effect(SocketServer.SocketServer)(
                    Effect.gen(function* () {
                        const [{ name, requiredPermissions }, url] = yield* Effect.all([Plugin, application]);
                        const {
                            domains: [origin],
                        } = yield* Effect.fromOption(requiredPermissions.network, () => new Cause.NoSuchElementError(`${name} manifest lacks requiredPermissions.network domains`));
                        const [dialed, identity] = yield* Effect.all([Effect.fromResult(Url.fromString(path, origin)), Schema.encodeEffect(Link)(Link.make({ application: url, extension: name }))]);
                        const { reader, writer } = yield* Socket.makeWebSocket(Url.setUrlParams(dialed, identity).href);
                        return {
                            address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Unspecified, 0),
                            run: (handler) =>
                                reader.pipe(
                                    Effect.flatMap((opened) => handler(Socket.make({ reader: Effect.succeed(opened), writer }))),
                                    Effect.scoped,
                                    Effect.tapCauseIf(Cause.hasDies, Effect.logError),
                                    Effect.sandbox,
                                    Effect.retry(redial),
                                    Effect.forever,
                                    Effect.mapError((cause) => new SocketServer.SocketServerError({ reason: new SocketServer.SocketServerUnknownError({ cause }) })),
                                ),
                        };
                    }),
                ),
                serialization,
            ]),
            Layer.provide(Socket.layerWebSocketConstructorGlobal),
        );
    static readonly listen = Effect.fn('Link.listen')(function* <Rpcs extends Rpc.Any>(
        group: RpcGroup.RpcGroup<Rpcs>,
        { path, serialization, middleware }: { readonly path: HttpRouter.PathInput; readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization>; readonly middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol> },
    ) {
        class Linked extends Data.Class<{ readonly peer: Link; readonly client: RpcClient.RpcClient.Flat<Rpcs, RpcClientError.RpcClientError> }> {}
        const [router, links] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make(Array.empty<Linked>())]);
        const linking = Effect.fnUntraced(function* (request: HttpServerRequest.HttpServerRequest) {
            const [peer, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(Link), request.upgrade, Deferred.make<Linked>(), Latch.make()]);
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
            yield* Deferred.succeed(made, new Linked({ peer, client }));
            return yield* Effect.as(ended.await, HttpServerResponse.empty());
        });
        yield* router.add('GET', path, (request) => (request.headers.origin === 'file://' ? linking(request) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
        return {
            peers: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))),
            changes: Stream.map(SubscriptionRef.changes(links), Array.map(Struct.get('peer'))),
            call: (serves: Predicate.Predicate<Link>) =>
                SubscriptionRef.get(links).pipe(Effect.map(Array.filter(({ peer }) => serves(peer))), Effect.flatMap(Unmatched.single('linked plugin serving application', Option.none(), ({ peer }) => [`${peer.extension} in ${peer.application.href}`])), Effect.map(Struct.get('client'))),
        };
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Link };
