import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Console, Deferred, Duration, Effect, flow, Latch, Layer, Option, Predicate, Schedule, Schema, SchemaGetter, Stream, Struct, SubscriptionRef } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse, Url } from 'effect/http';
import { NetAddress } from 'effect/net';
import { type Rpc, RpcClient, type RpcClientError, type RpcGroup, RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';

// --- [TYPES] ---------------------------------------------------------------------------

declare global {
    interface ImportMeta {
        readonly env: ImportMetaEnv;
    }
    interface ImportMetaEnv {
        readonly LINK: { readonly endpoint: string; readonly redial: `${number} ${Duration.Unit}` };
    }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUNDLE = /(?<=\.app)(?:\/.*)?$|(?<=[^/])$/u;

// --- [MODELS] --------------------------------------------------------------------------

class Link extends Schema.Class<Link, { readonly brand: unique symbol }>('Link')({
    application: Schema.URLFromString.pipe(Schema.decode({ decode: SchemaGetter.transform((url) => Url.setPathname(url, url.pathname.replace(BUNDLE, '/'))), encode: SchemaGetter.passthrough() })).annotate({ description: 'Application bundle URL with trailing slash' }),
    extension: Schema.String.annotate({ description: 'UXP plugin or CEP extension identifier' }),
}) {
    static readonly dial =
        <Rpcs extends Rpc.Any, H, EH, RH, E, R>(group: RpcGroup.RpcGroup<Rpcs>, handlers: Layer.Layer<H, EH, RH>, application: Effect.Effect<URL, E, R>) =>
        (extension: string) =>
            RpcServer.layer(group, { concurrency: 1 }).pipe(
                Layer.provide([handlers, RpcServer.layerProtocolSocketServer]),
                Layer.provide([
                    Layer.effect(SocketServer.SocketServer)(
                        Effect.gen(function* () {
                            const [dialed, url] = yield* Effect.all([Effect.fromResult(Url.fromString(import.meta.env.LINK.endpoint)), application]);
                            const identity = yield* Effect.orDie(Schema.encodeEffect(Link)(Link.make({ application: url, extension })));
                            const { reader, writer } = yield* Socket.makeWebSocket(Url.setUrlParams(dialed, identity).href);
                            return {
                                address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, Number(dialed.port)),
                                run: (handler) =>
                                    reader.pipe(
                                        Effect.flatMap((opened) => handler(Socket.make({ reader: Effect.succeed(opened), writer }))),
                                        Effect.scoped,
                                        Effect.tapCauseIf(Cause.hasDies, flow(Cause.pretty, Console.error)),
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
    static readonly origin = (port: number) => `ws://localhost:${port}`;
    static readonly serves = (application: URL) => (peer: Link) => peer.application.href === application.href;
    static readonly listen = Effect.fn('Link.listen')(function* <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, { path, middleware }: { readonly path: HttpRouter.PathInput; readonly middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol> }) {
        interface Linked {
            readonly peer: Link;
            readonly client: RpcClient.RpcClient.Flat<Rpcs, RpcClientError.RpcClientError>;
        }
        const [router, links] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make(Array.empty<Linked>())]);
        const linking = Effect.fnUntraced(function* (request: HttpServerRequest.HttpServerRequest) {
            const [peer, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(Link), request.upgrade, Deferred.make<Linked>(), Latch.make()]);
            const replaced = ({ peer: held }: Linked) => held.extension === peer.extension && Link.serves(peer.application)(held);
            const protocol = yield* RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) }).pipe(
                Effect.provideService(RpcClient.ConnectionHooks, {
                    onConnect: Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, flow(Array.filter<Linked>(Predicate.not(replaced)), Array.prepend(entry)))),
                    onDisconnect: Deferred.await(made).pipe(
                        Effect.flatMap((entry) => SubscriptionRef.update(links, Array.difference([entry]))),
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
        yield* router.add('GET', path, (request) => (request.headers.origin === 'file://' ? linking(request) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
        return {
            peers: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))),
            changes: Stream.map(SubscriptionRef.changes(links), Array.map(Struct.get('peer'))),
            call: (application: URL) => SubscriptionRef.get(links).pipe(Effect.flatMap(Unmatched.single('linked plugin serving application', Option.some(application.href), ({ peer }) => [peer.application.href])), Effect.map(Struct.get('client'))),
        };
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Link };
