/// <reference types="vite/client" />

import { Unmatched } from '@rasm/search';
import { Array, Cause, Deferred, Duration, Effect, flow, Latch, Layer, Number, Option, Predicate, Record, Ref, Schedule, Schema, SchemaGetter, Stream, Struct, SubscriptionRef } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse, Url } from 'effect/http';
import { Host, NetAddress } from 'effect/net';
import { type Rpc, RpcClient, type RpcClientError, type RpcGroup, RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';
import { HostUrl } from '#/requests';

// --- [TYPES] ---------------------------------------------------------------------------

declare global {
    interface ImportMetaEnv {
        readonly LINK: { readonly endpoint: string; readonly redial: `${number} ${Duration.Unit}` };
    }
}
interface Linked<Rpcs extends Rpc.Any> {
    readonly peer: Peer;
    readonly client: RpcClient.RpcClient.Flat<Rpcs, RpcClientError.RpcClientError>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUNDLE = /(?<=\.app)(?:\/.*)?$|(?<=[^/])$/u;
const ENDPOINT = Host.hostPortFromStringUnsafe('localhost:7490');

// --- [MODELS] --------------------------------------------------------------------------

class Peer extends Schema.Class<Peer, { readonly brand: unique symbol }>('Peer')({
    application: HostUrl.pipe(Schema.decode({ decode: SchemaGetter.transform((url) => Url.setPathname(url, url.pathname.replace(BUNDLE, '/'))), encode: SchemaGetter.passthrough() })).annotate({ description: 'Application bundle URL with trailing slash' }),
    extension: Schema.String.annotate({ description: 'UXP plugin or CEP extension identifier' }),
    root: Schema.OptionFromOptionalKey(HostUrl).annotate({ description: 'Directory the host loaded for this plugin' }),
}) {
    static readonly Connection = Schema.Struct({
        adobePlugin: Schema.Literals(['connected', 'disconnected']),
        connection: Schema.Int,
        application: Schema.String,
        extension: Schema.String,
        root: Schema.optionalKey(Schema.String),
    });
    static readonly dial =
        <Rpcs extends Rpc.Any, H, EH, RH, E, R>(group: RpcGroup.RpcGroup<Rpcs>, handlers: Layer.Layer<H, EH, RH>, application: Effect.Effect<URL, E, R>) =>
        <F, R2>(extension: string, root: Effect.Effect<URL, F, R2>) =>
            RpcServer.layer(group, { concurrency: 1 }).pipe(
                Layer.provide([handlers, RpcServer.layerProtocolSocketServer]),
                Layer.provide([
                    Layer.effect(SocketServer.SocketServer)(
                        Effect.gen(function* () {
                            const [dialed, url, loaded] = yield* Effect.all([Effect.fromResult(Url.fromString(import.meta.env.LINK.endpoint)), application, root]);
                            const identity = yield* Effect.orDie(Schema.encodeEffect(Peer)(Peer.make({ application: url, extension, root: Option.some(loaded) })));
                            const { reader, writer } = yield* Socket.makeWebSocket(Url.setUrlParams(dialed, identity).href);
                            return {
                                address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, globalThis.Number(dialed.port)),
                                run: (handler) =>
                                    reader.pipe(
                                        Effect.flatMap((opened) => handler(Socket.make({ reader: Effect.succeed(opened), writer }))),
                                        Effect.scoped,
                                        Effect.tapCauseIf(Cause.hasDies, Effect.logError),
                                        Effect.exit,
                                        Effect.andThen(Effect.sleep(import.meta.env.LINK.redial)),
                                        Effect.forever,
                                    ),
                            };
                        }),
                    ),
                    RpcSerialization.layerJson,
                ]),
                Layer.provide(Socket.layerWebSocketConstructorGlobal),
            );
    static readonly link = { endpoint: ENDPOINT, path: '/link', redial: '1 second', origin: `ws://${Host.formatHostPort(ENDPOINT)}` } as const;
    static readonly serves = (application: URL) => (peer: Peer) => peer.application.href === application.href;
    static readonly listen = Effect.fn('Peer.listen')(function* <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol>) {
        const [router, links, connections] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make<readonly Linked<Rpcs>[]>([]), Ref.make(0)]);
        yield* router.add('GET', Peer.link.path, (request) => (request.headers.origin === 'file://' ? Effect.flatMap(Ref.updateAndGet(connections, Number.increment), (connection) => accept(group, middleware, links, request, connection)) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
        return {
            peers: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))),
            changes: Stream.map(SubscriptionRef.changes(links), Array.map(Struct.get('peer'))),
            call: (application: URL) => SubscriptionRef.get(links).pipe(Effect.flatMap(Unmatched.single('linked plugin serving application', Option.some(application.href), ({ peer }) => [peer.application.href])), Effect.map(Struct.get('client'))),
        };
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const accept = Effect.fnUntraced(function* <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol>, links: SubscriptionRef.SubscriptionRef<readonly Linked<Rpcs>[]>, request: HttpServerRequest.HttpServerRequest, connection: number) {
    const [peer, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(Peer), request.upgrade, Deferred.make<Linked<Rpcs>>(), Latch.make()]);
    const replaced = ({ peer: held }: Linked<Rpcs>) => held.extension === peer.extension && Peer.serves(peer.application)(held);
    const report = (adobePlugin: typeof Peer.Connection.Type.adobePlugin) => Effect.logInfo(Peer.Connection.make({ adobePlugin, connection, application: peer.application.href, extension: peer.extension, ...Record.getSomes({ root: Option.map(peer.root, Struct.get('href')) }) }));
    const protocol = yield* RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) }).pipe(
        Effect.provideService(RpcClient.ConnectionHooks, {
            onConnect: Deferred.await(made).pipe(
                Effect.flatMap((entry) => SubscriptionRef.update(links, flow(Array.filter<Linked<Rpcs>>(Predicate.not(replaced)), Array.prepend(entry)))),
                Effect.andThen(report('connected')),
            ),
            onDisconnect: Deferred.await(made).pipe(
                Effect.flatMap((entry) => SubscriptionRef.update(links, Array.difference([entry]))),
                Effect.andThen(report('disconnected')),
                Effect.andThen(ended.open),
            ),
        }),
        Effect.provideService(Socket.Socket, socket),
        Effect.provide(RpcSerialization.layerJson),
    );
    const clients = yield* Layer.build(Layer.provideMerge(middleware, Layer.succeed(RpcClient.Protocol, protocol)));
    const client = yield* RpcClient.make(group, { flatten: true }).pipe(Effect.provideContext(clients));
    yield* Deferred.succeed(made, { peer, client });
    return yield* Effect.as(ended.await, HttpServerResponse.empty());
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Peer };
