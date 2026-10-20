import { Cause, Console, Effect, flow, Layer, Schedule } from 'effect';
import { UrlParams } from 'effect/http';
import { NetAddress } from 'effect/net';
import { type Rpc, type RpcGroup, type RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';

// --- [COMPOSITION] ---------------------------------------------------------------------

const linked = <Rpcs extends Rpc.Any, H, EH, RH, E, R>(group: RpcGroup.RpcGroup<Rpcs>, handlers: Layer.Layer<H, EH, RH>, { url, identity, serialization }: { readonly url: string; readonly identity: Effect.Effect<UrlParams.Input, E, R>; readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization> }) =>
    RpcServer.layer(group, { concurrency: 1 }).pipe(
        Layer.provide([handlers, RpcServer.layerProtocolSocketServer]),
        Layer.provide([
            Layer.effect(SocketServer.SocketServer)(
                Effect.gen(function* () {
                    const { reader, writer } = yield* Socket.makeWebSocket(`${url}?${UrlParams.toString(yield* identity)}`);
                    return {
                        address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Unspecified, 0),
                        run: (handler) =>
                            reader.pipe(
                                Effect.flatMap((opened) => handler(Socket.make({ reader: Effect.succeed(opened), writer }))),
                                Effect.scoped,
                                Effect.tapCauseIf(Cause.hasDies, flow(Cause.pretty, Console.error)),
                                Effect.sandbox,
                                Effect.retry(Schedule.spaced('2 seconds')),
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { linked };
