import { Cause, Console, Effect, flow, Layer, Schedule, Schema, SchemaGetter } from 'effect';
import { UrlParams } from 'effect/http';
import { NetAddress } from 'effect/net';
import { type Rpc, type RpcGroup, type RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';
import { Plugin } from './runtime.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Peer = Schema.Struct({
    application: Schema.URLFromString.pipe(Schema.decodeTo(Schema.URL, { decode: SchemaGetter.transform((url) => (url.pathname.endsWith('/') ? url : new URL(`${url.href}/`))), encode: SchemaGetter.passthrough() })).annotate({
        description: 'Bundle url of the application the plug-in serves, in the directory form Launch Services gives a bundle',
    }),
    extension: Schema.String.annotate({ description: 'Plugin name its manifest declares' }),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const linked = <Rpcs extends Rpc.Any, H, EH, RH, E, R>(group: RpcGroup.RpcGroup<Rpcs>, handlers: Layer.Layer<H, EH, RH>, { path, application, serialization }: { readonly path: string; readonly application: Effect.Effect<URL, E, R>; readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization> }) =>
    RpcServer.layer(group, { concurrency: 1 }).pipe(
        Layer.provide([handlers, RpcServer.layerProtocolSocketServer]),
        Layer.provide([
            Layer.effect(SocketServer.SocketServer)(
                Effect.gen(function* () {
                    const [{ name, requiredPermissions }, url] = yield* Effect.all([Plugin, application]);
                    const {
                        domains: [origin],
                    } = yield* Effect.fromOption(requiredPermissions.network, () => new Cause.NoSuchElementError(`${name} declares no network domain`));
                    const identity = yield* Schema.encodeEffect(Peer)({ application: url, extension: name });
                    const { reader, writer } = yield* Socket.makeWebSocket(`${origin}${path}?${UrlParams.toString(UrlParams.fromInput(identity))}`);
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

export { linked, Peer };
