import { Unmatched } from '@rasm/search';
import { Cause, Deferred, Duration, Effect, Equal, Exit, Function, Latch, Layer, Option, Predicate, Record, Ref, Schedule, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { Headers, HttpRouter, HttpServerRequest, HttpServerResponse, Url, type UrlParams } from 'effect/http';
import { NetAddress } from 'effect/net';
import type { ChildProcessSpawner } from 'effect/process';
import { type Rpc, RpcClient, type RpcClientError, type RpcGroup, RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';

// --- [TYPES] ---------------------------------------------------------------------------

interface Listening<Peer, RD> {
    readonly path: HttpRouter.PathInput;
    readonly origin: string;
    readonly peer: Schema.ConstraintCodec<Peer, Record.ReadonlyRecord<string, string | readonly string[] | undefined>, RD, unknown>;
    readonly key: (peer: Peer) => string;
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
    Effect.gen(function* () {
        const requests = new TransformStream<Uint8Array, Uint8Array>();
        yield* Stream.fromReadableStream({ evaluate: Function.constant(requests.readable), onError: Function.identity }).pipe(Stream.orDie, Stream.run(stdin), Effect.forkScoped);
        const socket = yield* Socket.fromTransformStream(Effect.succeed({ readable: Stream.toReadableStream(stdout), writable: requests.writable }));
        return yield* RpcClient.makeProtocolSocket(UNRETRIED).pipe(Effect.provideService(Socket.Socket, socket), Effect.provide(RpcSerialization.layerNdJsonRpc({ maxBufferSize: 'unbounded' })));
    }).pipe(Layer.effect(RpcClient.Protocol));
const linked = Effect.fnUntraced(
    function* <A, X, Y>(href: string, handler: (socket: Socket.Socket) => Effect.Effect<A, X, Y>) {
        const { reader, writer } = yield* Socket.makeWebSocket(href);
        const [opened, superseded] = yield* Effect.all([reader, Ref.make(false)]);
        const pull = Effect.tapError(opened.pull, ({ reason }) => Ref.set(superseded, Schema.is(Socket.SocketCloseError)(reason) && reason.code === SUPERSEDED.code));
        yield* handler(Socket.make({ reader: Effect.succeed({ ...opened, pull }), writer }));
        return yield* Ref.get(superseded);
    },
    Effect.scoped,
    Effect.exit,
);
const dial = <Rpcs extends Rpc.Any, H, EH, RH, E, R>(group: RpcGroup.RpcGroup<Rpcs>, handlers: Layer.Layer<H, EH, RH>, { url, redial, peer }: { readonly url: URL; readonly redial: Duration.Duration; readonly peer: Effect.Effect<UrlParams.Input, E, R> }) => {
    const server = Effect.map(Effect.context<R | Socket.WebSocketConstructor>(), (services) => ({
        address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, Number(url.port)),
        run: <A, X, Y>(handler: (socket: Socket.Socket) => Effect.Effect<A, X, Y>) =>
            Stream.fromEffectSchedule(peer, Schedule.spaced(redial)).pipe(
                Stream.mapEffect((identity) => linked(Url.setUrlParams(url, identity).href, handler)),
                Stream.takeUntil((exit) => Exit.isSuccess(exit) && exit.value),
                Stream.map((exit) => Option.map(Option.filter(Exit.getCause(exit), Predicate.not(Cause.hasInterruptsOnly)), Cause.pretty)),
                Stream.changes,
                Stream.runForEach(Option.match({ onNone: () => Effect.void, onSome: (failure) => Effect.logError(`Link to ${url.href} failed, redialing every ${Duration.format(redial)}`, failure) })),
                Effect.andThen(Effect.logInfo(`Link to ${url.href} ended, a newer peer of its key replaced it`)),
                Effect.catchCause((cause) => (Cause.hasInterruptsOnly(cause) ? Effect.interrupt : Effect.logError(`Link to ${url.href} ended, its peer identity failed`, cause))),
                Effect.andThen(Effect.never),
                Effect.provideContext(services),
            ),
    }));
    return RpcServer.layer(group, { concurrency: 1, disableFatalDefects: true }).pipe(Layer.provide([handlers, RpcServer.layerProtocolSocketServer]), Layer.provide([Layer.effect(SocketServer.SocketServer)(server), RpcSerialization.layerJson]));
};
const accept = Effect.fnUntraced(function* <Rpcs extends Rpc.Any, Peer, RD>(group: RpcGroup.RpcGroup<Rpcs>, { peer: codec, key }: Listening<Peer, RD>, links: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Linked<Rpcs, Peer>>>, request: HttpServerRequest.HttpServerRequest) {
    const [peer, identity, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(codec), HttpServerRequest.ParsedSearchParams, request.upgrade, Deferred.make<Linked<Rpcs, Peer>>(), Latch.make()]);
    const [writer, name] = [yield* socket.writer, key(peer)];
    const hooks = {
        onConnect: Effect.gen(function* () {
            const entry = yield* Deferred.await(made);
            const stale = yield* SubscriptionRef.modify(links, (held) => [Record.get(held, name), Record.set(held, name, entry)]);
            yield* Effect.transposeOption(Option.map(stale, Struct.get('superseded')));
            yield* Effect.logInfo('Peer linked');
        }),
        onDisconnect: Deferred.await(made).pipe(
            Effect.flatMap((entry) => SubscriptionRef.update(links, Record.filter(Predicate.not(Equal.equals(entry))))),
            Effect.andThen(Effect.logInfo('Peer unlinked')),
            Effect.andThen(ended.open),
        ),
    };
    const protocol = yield* RpcClient.makeProtocolSocket(UNRETRIED).pipe(Effect.provideService(RpcClient.ConnectionHooks, hooks), Effect.provideService(Socket.Socket, socket), Effect.provide(RpcSerialization.layerJson), Effect.annotateLogs(identity));
    const client = yield* Effect.provideService(RpcClient.make(group, { flatten: true }), RpcClient.Protocol, protocol);
    yield* Deferred.succeed(made, { peer, client, superseded: Effect.ignore(writer.write(SUPERSEDED)) });
    return yield* Effect.as(ended.await, HttpServerResponse.empty());
});
const listen = Effect.fn('listen')(function* <Rpcs extends Rpc.Any, Peer, RD>(group: RpcGroup.RpcGroup<Rpcs>, options: Listening<Peer, RD>) {
    const [router, links, services] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make<Record.ReadonlyRecord<string, Linked<Rpcs, Peer>>>({}), Effect.context<Rpc.MiddlewareClient<Rpcs>>()]);
    yield* router.add('GET', options.path, (request) => (Option.contains(Headers.get(request.headers, 'origin'), options.origin) ? Effect.provideContext(accept(group, options, links, request), services) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
    return {
        peers: Effect.map(SubscriptionRef.get(links), (held) => Record.collect(held, (_, { peer }) => peer)),
        changes: Stream.map(SubscriptionRef.changes(links), (held) => Record.collect(held, (_, { peer }) => peer)),
        call: (holds: Predicate.Predicate<Peer>, subject: string) => SubscriptionRef.get(links).pipe(Effect.map(Record.filter(({ peer }) => holds(peer))), Effect.map(Record.values), Effect.flatMap(Unmatched.single(subject, Option.none(), ({ peer }) => [options.key(peer)])), Effect.map(Struct.get('client'))),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { child, dial, listen };
