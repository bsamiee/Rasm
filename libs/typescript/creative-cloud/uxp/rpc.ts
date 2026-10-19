import { Data, Duration, Effect, Layer, Queue, Scope, Stream } from 'effect';
import { UrlParams } from 'effect/http';
import { NetAddress } from 'effect/net';
import { type Rpc, type RpcGroup, type RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';

// --- [TYPES] ---------------------------------------------------------------------------

type Link = { readonly _tag: 'Linked' } | { readonly _tag: 'Closed' } | { readonly _tag: 'Unreachable'; readonly cause: Socket.SocketError };

// --- [CONSTANTS] -----------------------------------------------------------------------

const REDIAL = Duration.seconds(2);

// --- [MODELS] --------------------------------------------------------------------------

const Link = Data.taggedEnum<Link>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const serve = <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, { url, identity, concurrency, serialization }: { readonly url: string; readonly identity: UrlParams.Input; readonly concurrency: number | 'unbounded'; readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization> }) =>
    Stream.unwrap(
        Effect.gen(function* () {
            const [{ reader, writer }, states, linked] = yield* Effect.all([Socket.makeWebSocket(`${url}?${UrlParams.toString(UrlParams.fromInput(identity))}`), Queue.unbounded<Link>(), Effect.flatMap(Effect.scope, Scope.fork)]);
            const protocol = yield* RpcServer.makeProtocolSocketServer.pipe(
                Effect.provideService(SocketServer.SocketServer, {
                    address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Unspecified, 0),
                    run: (handler) =>
                        handler(
                            Socket.make({
                                reader: reader.pipe(
                                    Effect.tapError((cause) => Queue.offer(states, Link.Unreachable({ cause }))),
                                    Effect.tap(() => Effect.acquireRelease(Queue.offer(states, Link.Linked()), () => Queue.offer(states, Link.Closed()))),
                                ),
                                writer,
                            }),
                        ).pipe(Effect.ignoreCause, Effect.andThen(Effect.sleep(REDIAL)), Effect.forever),
                }),
                Effect.provide(serialization),
                Scope.provide(linked),
            );
            yield* Effect.forkScoped(RpcServer.make(group, { concurrency, disableTracing: true }).pipe(Effect.provideService(RpcServer.Protocol, protocol)));
            return Stream.fromQueue(states);
        }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const linked = <Rpcs extends Rpc.Any, E, R>(group: RpcGroup.RpcGroup<Rpcs>, { peer, url, identity, serialization }: { readonly peer: string; readonly url: string; readonly identity: Effect.Effect<UrlParams.Input, E, R>; readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization> }) =>
    Layer.effectDiscard(
        Effect.gen(function* () {
            const status = yield* Effect.acquireRelease(
                Effect.sync(() => document.body.appendChild(document.createElement('sp-body'))),
                (element) => Effect.sync(() => element.remove()),
            );
            const shown = Link.$match({
                Linked: () => `Linked to ${peer}`,
                Closed: () => `${peer} closed, redialing`,
                Unreachable: ({ cause }) => `No ${peer} at ${url} (${cause.message}), redialing`,
            });
            yield* serve(group, { url, identity: yield* identity, concurrency: 1, serialization }).pipe(
                Stream.runForEach((state) => Effect.sync(() => status.replaceChildren(shown(state)))),
                Effect.forkScoped,
            );
        }),
    ).pipe(Layer.provide(Socket.layerWebSocketConstructorGlobal));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Link, linked, serve };
