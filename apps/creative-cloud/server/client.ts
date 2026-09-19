// --- [IMPORTS] -------------------------------------------------------------------------

import './runtime.ts';
import { Effect, Equal, Exit, flow, Layer, Option, Predicate, pipe, Queue, Record, Result, Schema, Scope, Stream, Struct } from 'effect';
import { RpcClient, type RpcClientError, RpcSerialization } from 'effect/unstable/rpc';
import { Socket } from 'effect/unstable/socket';
import { faulted, HostRejection, WriteRejection } from './errors.ts';
import { Frames, type Identity, type Job, type Settle, type State } from './frames.ts';
import { OptionalNumber, OptionalString } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Settled = Omit<Settle, 'jobId'>;

interface Client {
    readonly endpoint: string;
    readonly identity: () => Identity;
    readonly states: Stream.Stream<State>;
    readonly perform: (job: Job) => Effect.Effect<Settled>;
}

interface Lifecycle {
    readonly plugin: { readonly create: () => void; readonly destroy: () => Promise<void> };
}

type Handler = (body: Schema.Json) => Effect.Effect<Schema.Json, HostRejection>;

type Render = (raw: unknown) => Option.Option<Schema.Json>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _AsyncFunction: new (code: string) => () => Promise<unknown> = Object.getPrototypeOf(async () => undefined).constructor;

// --- [DECODERS] ------------------------------------------------------------------------

const _Thrown = Schema.Struct({ number: OptionalNumber, line: OptionalNumber, fileName: OptionalString, code: OptionalNumber, tag: OptionalString });

const _thrown = Schema.decodeUnknownOption(_Thrown);

// --- [REJECTIONS] ----------------------------------------------------------------------

const thrown = (cause: unknown): HostRejection =>
    Schema.is(HostRejection)(cause)
        ? cause
        : HostRejection.cases.scriptThrew.make({
              ...(cause instanceof Error ? { name: cause.name, message: cause.message, stack: Option.fromNullishOr(cause.stack) } : { name: 'Error', message: String(cause), stack: Option.none() }),
              ...Option.getOrElse(_thrown(cause), () => ({ number: Option.none(), line: Option.none(), fileName: Option.none(), code: Option.none(), tag: Option.none() })),
          });

// --- [JOBS] ----------------------------------------------------------------------------

const json = (value: unknown): Effect.Effect<Schema.Json, HostRejection> =>
    Effect.mapError(Schema.decodeUnknownEffect(Schema.Json)(Option.getOrNull(Option.fromNullishOr(value))), (cause) => HostRejection.cases.resultNotJson.make({ cause }));

const evaluate = (code: string): Effect.Effect<Schema.Json, HostRejection> => Effect.flatMap(Effect.tryPromise({ try: () => new _AsyncFunction(code)(), catch: thrown }), json);

const handler =
    <Body, Value>(input: Schema.Codec<Body, unknown, never, never>, output: Schema.Codec<Value, unknown, never, never>, work: (body: Body) => Effect.Effect<Value, HostRejection>): Handler =>
    (body): Effect.Effect<Schema.Json, HostRejection> =>
        Schema.decodeUnknownEffect(Schema.toCodecJson(input))(body).pipe(
            Effect.mapError((cause) => HostRejection.cases.malformedParams.make({ cause })),
            Effect.flatMap(work),
            Effect.flatMap((value) => Effect.mapError(Schema.encodeEffect(Schema.toCodecJson(output))(value), (cause) => HostRejection.cases.resultNotJson.make({ cause }))),
        );

const handle = (handlers: Readonly<Record<string, Handler>>, job: Job): Effect.Effect<Schema.Json, HostRejection> =>
    Option.match(Record.get(handlers, job.kind), { onNone: () => Effect.fail(HostRejection.cases.unknownMethod.make({ method: job.kind })), onSome: (found) => found(job.body) });

const settle = (outcome: Effect.Effect<Schema.Json, HostRejection>): Effect.Effect<Settled> =>
    Effect.map(Effect.result(Effect.catchDefect(outcome, flow(thrown, Effect.fail))), (result) => ({ autocorrections: Option.none(), result }));

// --- [PREFERENCES] ---------------------------------------------------------------------

const _descriptor = (host: object, key: string): Option.Option<PropertyDescriptor> =>
    Option.orElse(Option.fromNullishOr(Object.getOwnPropertyDescriptor(host, key)), () =>
        Option.flatMap(Option.fromNullishOr<object | null>(Object.getPrototypeOf(host)), (parent) => _descriptor(parent, key)),
    );

const read =
    (render: Render) =>
    (target: object, key: string): Result.Result<Schema.Json, unknown> =>
        Result.flatMap(
            Result.try(() => {
                const raw = Reflect.get(target, key);
                return [raw, render(raw)] as const;
            }),
            ([raw, rendered]) => Result.fromOption(rendered, () => raw),
        );

const written =
    (render: Render) =>
    (target: object, key: string, assigned: unknown, intended: Schema.Json): Result.Result<{ readonly from: Schema.Json; readonly to: Schema.Json }, WriteRejection> =>
        pipe(
            Result.liftPredicate(
                target,
                (host) => Reflect.has(host, key),
                () => WriteRejection.cases.unknownKey.make({}),
            ),
            Result.filterOrFail(
                (host) => Option.exists(_descriptor(host, key), (descriptor) => Predicate.isNotUndefined(descriptor.set) || descriptor.writable === true),
                () => WriteRejection.cases.readOnly.make({}),
            ),
            Result.flatMap((host) => Result.mapError(read(render)(host, key), (cause) => WriteRejection.cases.threw.make({ cause }))),
            Result.flatMap((from) =>
                Result.mapError(
                    Result.try(() => Reflect.set(target, key, assigned)).pipe(
                        Result.flatMap(() => read(render)(target, key)),
                        Result.map((to) => ({ from, to })),
                    ),
                    (cause) => WriteRejection.cases.threw.make({ cause }),
                ),
            ),
            Result.filterOrFail(
                ({ from, to }) => Equal.equals(to, intended) || !Equal.equals(to, from),
                () => WriteRejection.cases.unchanged.make({}),
            ),
        );

// --- [LINK] ----------------------------------------------------------------------------

const _attached = (client: Client, rpc: RpcClient.FromGroup<typeof Frames, RpcClientError.RpcClientError>): Effect.Effect<void> =>
    Stream.runDrain(
        Stream.merge(
            Stream.mapEffect(rpc.attach(client.identity()), (job) => Effect.flatMap(client.perform(job), (settled) => rpc.settle({ jobId: job.jobId, ...settled }))),
            Stream.mapEffect(client.states, (state) => rpc.state(state)),
        ),
    ).pipe(
        Effect.catchTag('RpcClientError', (error) => (error.reason._tag === 'SocketCloseError' ? Effect.logDebug(error) : Effect.fail(error))),
        Effect.catchCause(faulted),
    );

const run = (client: Client): Effect.Effect<void> =>
    Effect.gen(function* () {
        const connections = yield* Queue.unbounded<'connected' | 'disconnected'>();
        const protocol = RpcClient.layerProtocolSocket({ retryTransientErrors: true }).pipe(
            Layer.provide(
                Layer.succeed(RpcClient.ConnectionHooks, {
                    onConnect: Effect.asVoid(Queue.offer(connections, 'connected')),
                    onDisconnect: Effect.asVoid(Queue.offer(connections, 'disconnected')),
                }),
            ),
            Layer.provide(Socket.layerWebSocket(client.endpoint)),
            Layer.provide(Socket.layerWebSocketConstructorGlobal),
            Layer.provide(RpcSerialization.layerJson),
        );
        yield* Effect.flatMap(RpcClient.make(Frames), (rpc) =>
            Stream.runDrain(Stream.switchMap(Stream.fromQueue(connections), (connection) => (connection === 'connected' ? Stream.fromEffect(_attached(client, rpc)) : Stream.empty))),
        ).pipe(Effect.scoped, Effect.provide(protocol));
    }).pipe(Effect.tapCause(faulted));

// --- [LIFECYCLE] -----------------------------------------------------------------------

const lifecycle = (client: Client): Lifecycle => {
    const scope = Scope.makeUnsafe();
    return {
        plugin: { create: () => Effect.runSync(Effect.asVoid(Effect.forkIn(run(client), scope))), destroy: () => Effect.runPromise(Scope.close(scope, Exit.void)) },
    };
};

// --- [HOST] ----------------------------------------------------------------------------

const active = <D>(host: { readonly documents: { readonly length: number }; readonly activeDocument: D }): Option.Option<D> =>
    Option.map(
        Option.liftPredicate(host, (candidate) => candidate.documents.length > 0),
        Struct.get('activeDocument'),
    );

const opened = <D>(host: { readonly documents: { readonly length: number }; readonly activeDocument: D }): Effect.Effect<D, HostRejection> =>
    Effect.fromOption(active(host), () => HostRejection.cases.noActiveDocument.make({}));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Client, Handler, Lifecycle, Render, Settled };
export { active, evaluate, handle, handler, json, lifecycle, opened, read, run, settle, thrown, written };
