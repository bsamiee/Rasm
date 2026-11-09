/// <reference types="vite/client" />

import { hmac } from '@noble/hashes/hmac.js';
import { md5, sha1 } from '@noble/hashes/legacy.js';
import { sha256, sha384, sha512 } from '@noble/hashes/sha2.js';
import { HostError } from '@rasm/automation';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Deferred, Duration, Effect, FileSystem, flow, Latch, Layer, Number, Option, Path, Predicate, Record, Ref, Schedule, Schema, SchemaGetter, Stream, Struct, SubscriptionRef } from 'effect';
import { Mac, SecureRandom } from 'effect/crypto';
import { Hex } from 'effect/encoding';
import { HttpRouter, HttpServerRequest, HttpServerResponse, Url } from 'effect/http';
import { NetAddress } from 'effect/net';
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
const PORT = 7490;
const OWNER_ONLY = { folder: 0o700, file: 0o600 } as const;
const HASHES = { MD5: md5, 'SHA-1': sha1, 'SHA-256': sha256, 'SHA-384': sha384, 'SHA-512': sha512 } as const;

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
    static readonly Challenged = Schema.Struct({ ...Peer.fields, challenge: Schema.String.annotate({ description: 'Hex nonce the server answers with its HMAC under the link key' }) });
    static readonly link = {
        path: '/link',
        redial: '1 second',
        listen: NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, PORT),
        origin: `ws://localhost:${PORT}`,
        key: 'Library/Application Support/Atelier/link.key',
        nonce: 32,
    } as const;
    static readonly dial =
        <Rpcs extends Rpc.Any, H, EH, RH, E, R, K, RK>(group: RpcGroup.RpcGroup<Rpcs>, handlers: Layer.Layer<H, EH, RH>, application: Effect.Effect<URL, E, R>, key: Effect.Effect<string, K, RK>) =>
        <F, R2>(extension: string, root: Effect.Effect<URL, F, R2>) =>
            RpcServer.layer(group, { concurrency: 1 }).pipe(
                Layer.provide([handlers, RpcServer.layerProtocolSocketServer]),
                Layer.provide([
                    Layer.effect(SocketServer.SocketServer)(
                        Effect.gen(function* () {
                            const [dialed, url, loaded, mac, random, context] = yield* Effect.all([Effect.fromResult(Url.fromString(import.meta.env.LINK.endpoint)), application, root, Mac.Mac, SecureRandom.SecureRandom, Effect.context<RK | Socket.WebSocketConstructor>()]);
                            const identity = yield* Effect.orDie(Schema.encodeEffect(Peer)(Peer.make({ application: url, extension, root: Option.some(loaded) })));
                            const refused = HostError.refused(`Atelier link at ${dialed.href}`);
                            const proven = Effect.gen(function* () {
                                const [secret, nonce] = yield* Effect.all([
                                    key.pipe(
                                        Effect.flatMap(flow(Hex.decode, Effect.fromResult)),
                                        Effect.flatMap((bytes) => Mac.key(Mac.hmacSha256, bytes)),
                                    ),
                                    random.randomBytes(Peer.link.nonce),
                                ]);
                                const socket = yield* Socket.makeWebSocket(Url.setUrlParams(dialed, { ...identity, challenge: Hex.encode(nonce) }).href);
                                const reader = yield* socket.reader;
                                const [proof, ...rest] = yield* reader.pull;
                                const tag = yield* Effect.fromResult(Hex.decode(Predicate.isString(proof) ? proof : new TextDecoder().decode(proof)));
                                yield* Effect.filterOrFail(mac.verify(secret, tag, nonce), Boolean, () => refused('server answered the challenge without the link key'));
                                const held = yield* Ref.make<readonly (Uint8Array | string)[]>(rest);
                                return Socket.make({ reader: Effect.succeed({ ...reader, pull: Effect.flatMap(Ref.getAndSet(held, []), Array.match({ onEmpty: () => reader.pull, onNonEmpty: Effect.succeed })) }), writer: socket.writer });
                            }).pipe(Effect.tapErrorTag('HostError', Effect.logWarning), Effect.provideContext(context));
                            return {
                                address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, globalThis.Number(dialed.port)),
                                run: (handler) => proven.pipe(Effect.flatMap(handler), Effect.scoped, Effect.tapCauseIf(Cause.hasDies, Effect.logError), Effect.exit, Effect.andThen(Effect.sleep(import.meta.env.LINK.redial)), Effect.forever),
                            };
                        }),
                    ),
                    RpcSerialization.layerJson,
                ]),
                Layer.provide([Socket.layerWebSocketConstructorGlobal, Mac.layerWebCryptoWith({ hmac: (hash, secret, data) => hmac(HASHES[hash], secret, data) }).pipe(Layer.provideMerge(SecureRandom.layerWebCrypto))]),
            );
    static readonly serves = (application: URL) => (peer: Peer) => peer.application.href === application.href;
    static readonly listen = Effect.fn('Peer.listen')(function* <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol>, home: string) {
        const [router, links, connections, mac, fs, path] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make<readonly Linked<Rpcs>[]>([]), Ref.make(0), Mac.Mac, FileSystem.FileSystem, Path.Path]);
        const secret = yield* mac.generateKey(Mac.hmacSha256);
        const file = path.join(home, Peer.link.key);
        const prove = (nonce: Uint8Array) => mac.sign(secret, nonce);
        yield* fs.makeDirectory(path.dirname(file), { recursive: true, mode: OWNER_ONLY.folder });
        yield* fs.chmod(path.dirname(file), OWNER_ONLY.folder);
        yield* fs.writeFileString(file, Hex.encode(yield* Mac.toBytes(secret)), { mode: OWNER_ONLY.file });
        yield* fs.chmod(file, OWNER_ONLY.file);
        yield* router.add('GET', Peer.link.path, (request) => (request.headers.origin === 'file://' ? Effect.flatMap(Ref.updateAndGet(connections, Number.increment), (connection) => accept(group, middleware, links, request, connection, prove)) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
        return {
            peers: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))),
            changes: Stream.map(SubscriptionRef.changes(links), Array.map(Struct.get('peer'))),
            call: (application: URL) => SubscriptionRef.get(links).pipe(Effect.flatMap(Unmatched.single('linked plugin serving application', Option.some(application.href), ({ peer }) => [peer.application.href])), Effect.map(Struct.get('client'))),
        };
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const accept = Effect.fnUntraced(function* <Rpcs extends Rpc.Any, E>(
    group: RpcGroup.RpcGroup<Rpcs>,
    middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol>,
    links: SubscriptionRef.SubscriptionRef<readonly Linked<Rpcs>[]>,
    request: HttpServerRequest.HttpServerRequest,
    connection: number,
    prove: (nonce: Uint8Array) => Effect.Effect<Uint8Array, E>,
) {
    const [{ challenge, ...fields }, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(Peer.Challenged), request.upgrade, Deferred.make<Linked<Rpcs>>(), Latch.make()]);
    const peer = Peer.make(fields);
    const writer = yield* socket.writer;
    yield* writer.write(Hex.encode(yield* prove(yield* Effect.fromResult(Hex.decode(challenge)))));
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
