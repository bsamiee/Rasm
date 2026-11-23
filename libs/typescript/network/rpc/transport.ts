import { Unmatched } from '@rasm/search';
import { Array, Cause, Deferred, Duration, Effect, Exit, Function, Latch, Layer, Number, Option, type Predicate, Ref, Result, Schedule, type Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { Headers, HttpRouter, HttpServerRequest, HttpServerResponse, Url, type UrlParams } from 'effect/http';
import { NetAddress } from 'effect/net';
import type { ChildProcessSpawner } from 'effect/process';
import { type Rpc, RpcClient, type RpcClientError, type RpcGroup, RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';

// --- [TYPES] ---------------------------------------------------------------------------

interface Listening<Rpcs extends Rpc.Any, Peer, RD> {
    readonly path: HttpRouter.PathInput;
    readonly origin: string;
    readonly peer: Schema.ConstraintCodec<Peer, Readonly<Record<string, string | readonly string[] | undefined>>, RD, unknown>;
    readonly key: (peer: Peer) => string;
    readonly middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol>;
}
interface Linked<Rpcs extends Rpc.Any, Peer> {
    readonly peer: Peer;
    readonly client: RpcClient.RpcClient.Flat<Rpcs, RpcClientError.RpcClientError>;
    readonly superseded: Effect.Effect<void>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNRETRIED = { pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) };
const SUPERSEDED = new Socket.CloseEvent(4000, 'Superseded by a newer peer of its key');

// --- [OPERATIONS] ----------------------------------------------------------------------

const child = ({ stdin, stdout }: Pick<ChildProcessSpawner.ChildProcessHandle, 'stdin' | 'stdout'>) =>
    Layer.effect(RpcClient.Protocol)(
        Effect.gen(function* () {
            const requests = new TransformStream<Uint8Array, Uint8Array>();
            yield* Stream.fromReadableStream({ evaluate: Function.constant(requests.readable), onError: Function.identity }).pipe(Stream.orDie, Stream.run(stdin), Effect.forkScoped);
            const socket = yield* Socket.fromTransformStream(Effect.succeed({ readable: Stream.toReadableStream(stdout), writable: requests.writable }));
            return yield* RpcClient.makeProtocolSocket(UNRETRIED).pipe(Effect.provideService(Socket.Socket, socket), Effect.provide(RpcSerialization.layerNdJsonRpc()));
        }),
    );
const linked = Effect.fnUntraced(
    function* <A, X, Y>(href: string, handler: (socket: Socket.Socket) => Effect.Effect<A, X, Y>) {
        const { reader, writer } = yield* Socket.makeWebSocket(href);
        const [opened, superseded] = yield* Effect.all([reader, Ref.make(false)]);
        const pull = Effect.tapError(opened.pull, ({ reason }) => Ref.set(superseded, reason._tag === 'SocketCloseError' && reason.code === SUPERSEDED.code));
        yield* handler(Socket.make({ reader: Effect.succeed({ ...opened, pull }), writer }));
        return yield* Ref.get(superseded);
    },
    Effect.scoped,
    Effect.exit,
);
const dial = <Rpcs extends Rpc.Any, H, EH, RH, E, R>(group: RpcGroup.RpcGroup<Rpcs>, handlers: Layer.Layer<H, EH, RH>, { url, redial, peer }: { readonly url: URL; readonly redial: Duration.Duration; readonly peer: Effect.Effect<UrlParams.Input, E, R> }) =>
    RpcServer.layer(group, { concurrency: 1 }).pipe(
        Layer.provide([handlers, RpcServer.layerProtocolSocketServer]),
        Layer.provide([
            Layer.effect(SocketServer.SocketServer)(
                Effect.map(Effect.context<R | Socket.WebSocketConstructor>(), (services) => ({
                    address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, globalThis.Number(url.port)),
                    run: <A, X, Y>(handler: (socket: Socket.Socket) => Effect.Effect<A, X, Y>) =>
                        Stream.fromEffectSchedule(
                            Effect.flatMap(peer, (identity) => linked(Url.setUrlParams(url, identity).href, handler)),
                            Schedule.spaced(redial),
                        ).pipe(
                            Stream.takeUntil((exit) => Exit.isSuccess(exit) && exit.value),
                            Stream.map((exit) => (Exit.isFailure(exit) && !Cause.hasInterruptsOnly(exit.cause) ? Option.some(Cause.pretty(exit.cause)) : Option.none())),
                            Stream.changes,
                            Stream.runForEach(Option.match({ onNone: () => Effect.void, onSome: (failure) => Effect.logError(`Link to ${url.href} failed, redialing every ${Duration.format(redial)}`, failure) })),
                            Effect.andThen(Effect.logInfo(`Link to ${url.href} ended, a newer peer of its key replaced it`)),
                            Effect.catchCause((cause) => (Cause.hasInterruptsOnly(cause) ? Effect.interrupt : Effect.logError(`Link to ${url.href} ended, its peer identity failed`, cause))),
                            Effect.andThen(Effect.never),
                            Effect.provideContext(services),
                        ),
                })),
            ),
            RpcSerialization.layerJson,
        ]),
    );
const accept = Effect.fnUntraced(function* <Rpcs extends Rpc.Any, Peer, RD>(group: RpcGroup.RpcGroup<Rpcs>, { peer: codec, key, middleware }: Listening<Rpcs, Peer, RD>, links: SubscriptionRef.SubscriptionRef<readonly Linked<Rpcs, Peer>[]>, request: HttpServerRequest.HttpServerRequest, connection: number) {
    const [peer, identity, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(codec), HttpServerRequest.ParsedSearchParams, request.upgrade, Deferred.make<Linked<Rpcs, Peer>>(), Latch.make()]);
    const [writer, same] = [yield* socket.writer, Result.liftPredicate((entry: Linked<Rpcs, Peer>) => key(entry.peer) === key(peer), Function.identity)];
    const report = (message: string) => Effect.annotateLogs(Effect.logInfo(message), { ...identity, connection });
    const protocol = yield* RpcClient.makeProtocolSocket(UNRETRIED).pipe(
        Effect.provideService(RpcClient.ConnectionHooks, {
            onConnect: Deferred.await(made).pipe(
                Effect.flatMap((entry) =>
                    SubscriptionRef.modify(links, (held) => {
                        const [stale, kept] = Array.partition(held, same);
                        return [stale, Array.prepend(kept, entry)];
                    }),
                ),
                Effect.flatMap(Effect.forEach(Struct.get('superseded'), { discard: true })),
                Effect.andThen(report('Peer linked')),
            ),
            onDisconnect: Deferred.await(made).pipe(
                Effect.flatMap((entry) => SubscriptionRef.update(links, Array.difference([entry]))),
                Effect.andThen(report('Peer unlinked')),
                Effect.andThen(ended.open),
            ),
        }),
        Effect.provideService(Socket.Socket, socket),
        Effect.provide(RpcSerialization.layerJson),
    );
    const clients = yield* Layer.build(Layer.provideMerge(middleware, Layer.succeed(RpcClient.Protocol, protocol)));
    const client = yield* RpcClient.make(group, { flatten: true }).pipe(Effect.provideContext(clients));
    yield* Deferred.succeed(made, { peer, client, superseded: Effect.ignore(writer.write(SUPERSEDED)) });
    return yield* Effect.as(ended.await, HttpServerResponse.empty());
});
const listen = Effect.fn('listen')(function* <Rpcs extends Rpc.Any, Peer, RD>(group: RpcGroup.RpcGroup<Rpcs>, options: Listening<Rpcs, Peer, RD>) {
    const [router, links, connections] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make<readonly Linked<Rpcs, Peer>[]>([]), Ref.make(0)]);
    yield* router.add('GET', options.path, (request) =>
        Option.contains(Headers.get(request.headers, 'origin'), options.origin) ? Effect.flatMap(Ref.updateAndGet(connections, Number.increment), (connection) => accept(group, options, links, request, connection)) : Effect.succeed(HttpServerResponse.empty({ status: 403 })),
    );
    return {
        peers: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))),
        changes: Stream.map(SubscriptionRef.changes(links), Array.map(Struct.get('peer'))),
        call: (holds: Predicate.Predicate<Peer>, subject: string) => SubscriptionRef.get(links).pipe(Effect.map(Array.filter(({ peer }) => holds(peer))), Effect.flatMap(Unmatched.single(subject, Option.none(), ({ peer }) => [options.key(peer)])), Effect.map(Struct.get('client'))),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { child, dial, listen };
