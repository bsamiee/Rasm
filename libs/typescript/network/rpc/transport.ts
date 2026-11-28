import { Unmatched } from '@rasm/search/unmatched';
import { Cause, Deferred, Duration, Effect, Equal, Exit, Function, Latch, Layer, Match, MutableRef, Option, pipe, Predicate, Record, Ref, Result, Schedule, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { Headers, HttpRouter, HttpServerRequest, HttpServerResponse, Url, type UrlParams } from 'effect/http';
import { Host, NetAddress } from 'effect/net';
import type { ChildProcessSpawner } from 'effect/process';
import { type Rpc, RpcClient, type RpcClientError, type RpcGroup, RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';

// --- [TYPES] ---------------------------------------------------------------------------

interface ListenOptions<Peer, RD> {
    readonly path: HttpRouter.PathInput;
    readonly origin: string;
    readonly peer: Schema.ConstraintCodec<Peer, Record.ReadonlyRecord<string, string | readonly string[] | undefined>, RD, unknown>;
    readonly key: (peer: Peer) => string;
}
interface Connection<Rpcs extends Rpc.Any, Peer> {
    readonly peer: Peer;
    readonly client: RpcClient.RpcClient.Flat<Rpcs, RpcClientError.RpcClientError>;
    readonly superseded: Effect.Effect<void>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SUPERSEDED = new Socket.CloseEvent(4000, 'Superseded by a newer peer of its key');
const DEFAULT_PORTS: Record.ReadonlyRecord<string, number> = { 'ws:': 80, 'wss:': 443 };
const LOCALHOST = Host.domainNameFromStringUnsafe('localhost');

// --- [OPERATIONS] ----------------------------------------------------------------------

const openError = (cause: unknown) => Socket.SocketError.make({ reason: Socket.SocketOpenError.make({ kind: 'Unknown', cause }) });
const layerProtocolChildProcess = <E, R>(spawn: Effect.Effect<Pick<ChildProcessSpawner.ChildProcessHandle, 'stdin' | 'stdout' | 'exitCode'>, E, R>) =>
    Effect.gen(function* () {
        const piped = yield* Socket.fromTransformStream(
            Effect.gen(function* () {
                const { stdin, stdout, exitCode } = yield* Effect.mapError(spawn, openError);
                const requests = new TransformStream<Uint8Array, Uint8Array>();
                yield* Stream.fromReadableStream({ evaluate: Function.constant(requests.readable), onError: Function.identity }).pipe(Stream.orDie, Stream.run(stdin), Effect.forkScoped);
                return { readable: Stream.toReadableStream(Stream.interruptWhen(stdout, exitCode)), writable: requests.writable };
            }),
        );
        const reader = Effect.gen(function* () {
            const [{ pull, upgrade }, answered] = yield* Effect.all([piped.reader, Effect.sync(() => MutableRef.make(false))]);
            return {
                upgrade,
                pull: pull.pipe(
                    Effect.tap(Effect.sync(() => MutableRef.set(answered, true))),
                    Effect.mapError((error) => (MutableRef.get(answered) ? error : openError(error))),
                ),
            };
        });
        const retryPolicy = Schedule.forever.pipe(
            Schedule.setInputType<Socket.SocketError>(),
            Schedule.while(({ input }) => !Schema.is(Socket.SocketOpenError)(input.reason)),
        );
        return yield* RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy }).pipe(Effect.provideService(Socket.Socket, Socket.make({ reader, writer: piped.writer })), Effect.provide(RpcSerialization.layerNdJsonRpc({ maxBufferSize: 'unbounded' })));
    }).pipe(Layer.effect(RpcClient.Protocol));
const connect = Effect.fnUntraced(
    function* <A, X, Y>(href: string, handler: (socket: Socket.Socket) => Effect.Effect<A, X, Y>) {
        const { reader, writer } = yield* Socket.makeWebSocket(href);
        const [opened, superseded] = yield* Effect.all([Effect.mapError(reader, openError), Ref.make(false)]);
        const pull = Effect.tapError(opened.pull, ({ reason }) => Ref.set(superseded, Schema.is(Socket.SocketCloseError)(reason) && reason.code === SUPERSEDED.code));
        yield* handler(Socket.make({ reader: Effect.succeed({ ...opened, pull }), writer }));
        return yield* Ref.get(superseded);
    },
    Effect.scoped,
    Effect.exit,
);
const dial = <Rpcs extends Rpc.Any, H, EH, RH, E, R>(group: RpcGroup.RpcGroup<Rpcs>, handlers: Layer.Layer<H, EH, RH>, { url, redial, peer }: { readonly url: URL; readonly redial: Duration.Duration; readonly peer: Effect.Effect<UrlParams.Input, E, R> }) => {
    const server = Effect.map(
        Effect.all([
            Effect.fromResult(
                pipe(
                    url.port === '' ? Record.get(DEFAULT_PORTS, url.protocol) : Option.some(Number(url.port)),
                    Result.fromOption(() => new NetAddress.NetAddressError({ input: url.href, message: `${url.href} states no port, and ${url.protocol} has no default port` })),
                    Result.flatMap((port) => Host.hostPortFromString(`${url.hostname}:${port}`)),
                    Result.flatMap(({ host, port }) =>
                        Match.value(host).pipe(
                            Match.when(Equal.equals(LOCALHOST), () => NetAddress.inetAddress(NetAddress.ipv4Loopback, port)),
                            Match.when(Host.isDomainName, (name) => Result.fail(new NetAddress.NetAddressError({ input: url.href, message: `${url.href} names ${name}, which resolves through DNS alone, and dial holds no resolver` }))),
                            Match.orElse((literal) => NetAddress.inetAddressFromHostString(Host.formatHost(literal), port)),
                        ),
                    ),
                ),
            ),
            Effect.context<R | Socket.WebSocketConstructor>(),
        ]),
        ([address, services]) => ({
            address,
            run: <A, X, Y>(handler: (socket: Socket.Socket) => Effect.Effect<A, X, Y>) =>
                Stream.fromEffectSchedule(
                    Effect.flatMap(peer, (identity) => connect(Url.setUrlParams(url, identity).href, handler)),
                    Schedule.forever.pipe(
                        Schedule.setInputType<Exit.Exit<boolean, X | Socket.SocketError>>(),
                        Schedule.addDelay(({ input }) => Effect.succeed(Option.exists(Exit.findErrorOption(input), (error) => Socket.isSocketError(error) && Schema.is(Socket.SocketOpenError)(error.reason)) ? redial : Duration.zero)),
                    ),
                ).pipe(
                    Stream.takeUntil((exit) => Exit.isSuccess(exit) && exit.value),
                    Stream.map((exit) => Option.map(Option.filter(Exit.getCause(exit), Predicate.not(Cause.hasInterruptsOnly)), Cause.pretty)),
                    Stream.changes,
                    Stream.runForEach(Option.match({ onNone: () => Effect.void, onSome: (failure) => Effect.logError(`Link to ${url.href} failed, redialing at once after a close and in ${Duration.format(redial)} after a refusal`, failure) })),
                    Effect.andThen(Effect.logInfo(`Link to ${url.href} ended, a newer peer of its key replaced it`)),
                    Effect.catchCause((cause) => (Cause.hasInterruptsOnly(cause) ? Effect.interrupt : Effect.logError(`Link to ${url.href} ended, its peer identity failed`, cause))),
                    Effect.andThen(Effect.never),
                    Effect.provideContext(services),
                ),
        }),
    );
    return RpcServer.layer(group, { concurrency: 1, disableFatalDefects: true }).pipe(Layer.provide([handlers, RpcServer.layerProtocolSocketServer]), Layer.provide([Layer.effect(SocketServer.SocketServer)(server), RpcSerialization.layerJson]));
};
const accept = Effect.fnUntraced(function* <Rpcs extends Rpc.Any, Peer, RD>(group: RpcGroup.RpcGroup<Rpcs>, { peer: codec, key }: ListenOptions<Peer, RD>, links: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Connection<Rpcs, Peer>>>, request: HttpServerRequest.HttpServerRequest) {
    const [peer, identity, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(codec), HttpServerRequest.ParsedSearchParams, request.upgrade, Deferred.make<Connection<Rpcs, Peer>>(), Latch.make()]);
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
    const protocol = yield* RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) }).pipe(Effect.provideService(RpcClient.ConnectionHooks, hooks), Effect.provideService(Socket.Socket, socket), Effect.provide(RpcSerialization.layerJson), Effect.annotateLogs(identity));
    const client = yield* Effect.provideService(RpcClient.make(group, { flatten: true }), RpcClient.Protocol, protocol);
    yield* Deferred.succeed(made, { peer, client, superseded: Effect.ignore(writer.write(SUPERSEDED)) });
    return yield* Effect.as(ended.await, HttpServerResponse.empty());
});
const listen = Effect.fn('listen')(function* <Rpcs extends Rpc.Any, Peer, RD>(group: RpcGroup.RpcGroup<Rpcs>, options: ListenOptions<Peer, RD>) {
    const [router, links, services] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make<Record.ReadonlyRecord<string, Connection<Rpcs, Peer>>>({}), Effect.context<Rpc.MiddlewareClient<Rpcs>>()]);
    yield* router.add('GET', options.path, (request) => (Option.contains(Headers.get(request.headers, 'origin'), options.origin) ? Effect.provideContext(accept(group, options, links, request), services) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
    return {
        peers: Effect.map(SubscriptionRef.get(links), (held) => Record.collect(held, (_, { peer }) => peer)),
        changes: Stream.map(SubscriptionRef.changes(links), (held) => Record.collect(held, (_, { peer }) => peer)),
        call: (holds: Predicate.Predicate<Peer>, subject: string) => SubscriptionRef.get(links).pipe(Effect.map(Record.filter(({ peer }) => holds(peer))), Effect.map(Record.values), Effect.flatMap(Unmatched.single(subject, Option.none(), ({ peer }) => [options.key(peer)])), Effect.map(Struct.get('client'))),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { dial, layerProtocolChildProcess, listen };
