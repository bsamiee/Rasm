// --- [IMPORTS] -------------------------------------------------------------------------

import { Cause, Effect, Fiber, Layer, Option, Queue, Ref, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { RpcClient, type RpcClientError, RpcSerialization } from 'effect/unstable/rpc';
import { Socket } from 'effect/unstable/socket';
import { HostRejection } from './errors.ts';
import { Frames, type Identity, type Job, type Settle, type State } from './frames.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Settled = Omit<Settle, 'jobId'>;

type Status = 'Redialing' | 'Attached';

interface Client {
    readonly endpoint: string;
    readonly version: string;
    readonly identity: () => Identity;
    readonly states: Stream.Stream<State>;
    readonly perform: (job: Job) => Effect.Effect<Settled>;
}

interface Lifecycle {
    readonly plugin: { readonly create: () => Promise<void>; readonly destroy: () => Promise<void> };
    readonly panels: Readonly<Record<string, { readonly create: (root: HTMLElement) => Promise<void>; readonly destroy: () => Promise<void> }>>;
}

type Handler = (body: Schema.Json) => Effect.Effect<Schema.Json, HostRejection>;

type Slot = Ref.Ref<Option.Option<Fiber.Fiber<void>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _AsyncFunction: new (code: string) => () => Promise<unknown> = Object.getPrototypeOf(async () => undefined).constructor;

const _CLOSE_NORMAL = 1000;

const _numbered: (cause: unknown) => Option.Option<{ readonly number: number }> = Schema.decodeUnknownOption(Schema.Struct({ number: Schema.Number }));

// --- [REJECTIONS] ----------------------------------------------------------------------

const thrown = (cause: unknown): HostRejection =>
    HostRejection.cases.scriptThrew.make({
        ...(cause instanceof Error ? { name: cause.name, message: cause.message, stack: Option.fromNullishOr(cause.stack) } : { name: 'Error', message: String(cause), stack: Option.none() }),
        line: Option.none(),
        fileName: Option.none(),
        number: Option.map(_numbered(cause), Struct.get('number')),
    });

// --- [JOBS] ----------------------------------------------------------------------------

const json = (value: unknown): Effect.Effect<Schema.Json, HostRejection> =>
    Effect.mapError(Schema.decodeUnknownEffect(Schema.Json)(Option.getOrNull(Option.fromNullishOr(value))), (cause) => HostRejection.cases.resultNotJson.make({ cause }));

const evaluate = (code: string): Effect.Effect<Schema.Json, HostRejection> => Effect.flatMap(Effect.tryPromise({ try: () => new _AsyncFunction(code)(), catch: thrown }), json);

const handler =
    <Body, Value>(input: Schema.Codec<Body, unknown, never, never>, output: Schema.Codec<Value, unknown, never, never>, work: (body: Body) => Effect.Effect<Value, HostRejection>): Handler =>
    (body): Effect.Effect<Schema.Json, HostRejection> =>
        Schema.decodeUnknownEffect(input)(body).pipe(
            Effect.mapError((cause) => HostRejection.cases.malformedParams.make({ cause })),
            Effect.flatMap(work),
            Effect.flatMap((value) => Effect.mapError(Schema.encodeEffect(Schema.toCodecJson(output))(value), (cause) => HostRejection.cases.resultNotJson.make({ cause }))),
        );

// --- [LINK] ----------------------------------------------------------------------------

const _line = (version: string, state: Status): string => `${version} › ${state}`;

const _mark = (client: Client, status: SubscriptionRef.SubscriptionRef<string>, state: Status): Effect.Effect<void> => SubscriptionRef.set(status, _line(client.version, state));

const _faulted = (cause: Cause.Cause<unknown>): Effect.Effect<void> => (Cause.hasInterruptsOnly(cause) ? Effect.void : Effect.logError(cause));

const _attached = (client: Client, rpc: RpcClient.FromGroup<typeof Frames, RpcClientError.RpcClientError>, status: SubscriptionRef.SubscriptionRef<string>): Effect.Effect<void> =>
    _mark(client, status, 'Attached').pipe(
        Effect.andThen(
            Stream.runDrain(
                Stream.merge(
                    Stream.mapEffect(rpc.attach(client.identity()), (job) => Effect.flatMap(client.perform(job), (settled) => rpc.settle({ jobId: job.jobId, ...settled }))),
                    Stream.mapEffect(client.states, (state) => rpc.state(state)),
                ),
            ),
        ),
        Effect.catchCause(_faulted),
        Effect.ensuring(_mark(client, status, 'Redialing')),
    );

const run = (client: Client, status: SubscriptionRef.SubscriptionRef<string>): Effect.Effect<void> =>
    Effect.gen(function* () {
        const connections = yield* Queue.unbounded<'connected' | 'disconnected'>();
        const hooks = Layer.succeed(RpcClient.ConnectionHooks, {
            onConnect: Effect.asVoid(Queue.offer(connections, 'connected')),
            onDisconnect: Effect.asVoid(Queue.offer(connections, 'disconnected')),
        });
        const socket = Socket.fromWebSocket(
            Effect.acquireRelease(
                Effect.try({
                    try: () => new WebSocket(client.endpoint),
                    catch: (cause) => new Socket.SocketError({ reason: new Socket.SocketOpenError({ kind: 'Unknown', cause }) }),
                }),
                (ws) =>
                    Effect.sync(() => {
                        ws.close(_CLOSE_NORMAL);
                    }),
            ),
        );
        const protocol = RpcClient.layerProtocolSocket({ retryTransientErrors: true }).pipe(
            Layer.provide(hooks),
            Layer.provide(Layer.effect(Socket.Socket, socket)),
            Layer.provide(RpcSerialization.layerJson),
        );
        yield* Effect.flatMap(RpcClient.make(Frames), (rpc) =>
            Stream.runDrain(Stream.switchMap(Stream.fromQueue(connections), (connection) => (connection === 'connected' ? Stream.fromEffect(_attached(client, rpc, status)) : Stream.empty))),
        ).pipe(Effect.scoped, Effect.provide(protocol));
    }).pipe(Effect.tapCause(_faulted));

// --- [LIFECYCLE] -----------------------------------------------------------------------

const _start = (slot: Slot, work: Effect.Effect<void>): Promise<void> => Effect.runPromise(Ref.set(slot, Option.some(Effect.runFork(work))));

const _stop = (slot: Slot): Promise<void> => Effect.runPromise(Effect.flatMap(Ref.getAndSet(slot, Option.none()), Option.match({ onNone: () => Effect.void, onSome: Fiber.interrupt })));

const _render = (status: SubscriptionRef.SubscriptionRef<string>, root: HTMLElement): Effect.Effect<void> =>
    Effect.scoped(
        Effect.gen(function* () {
            const line = yield* Effect.acquireRelease(
                Effect.sync(() => root.appendChild(document.createElement('p'))),
                (appended) => Effect.sync(() => appended.remove()),
            );
            yield* Stream.runForEach(SubscriptionRef.changes(status), (text) =>
                Effect.sync(() => {
                    line.textContent = text;
                }),
            );
        }),
    );

const lifecycle = (client: Client, panel: string): Lifecycle => {
    const status = Effect.runSync(SubscriptionRef.make(_line(client.version, 'Redialing')));
    const link: Slot = Ref.makeUnsafe(Option.none());
    const view: Slot = Ref.makeUnsafe(Option.none());
    return {
        plugin: { create: () => _start(link, run(client, status)), destroy: () => _stop(link) },
        panels: { [panel]: { create: (root) => _start(view, _render(status, root)), destroy: () => _stop(view) } },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Client, Handler, Lifecycle, Settled };
export { evaluate, handler, json, lifecycle, run, thrown };
