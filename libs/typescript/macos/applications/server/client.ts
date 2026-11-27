import { Program } from '@rasm/current-process';
import { spawn } from '@rasm/process';
import { layerProtocolChildProcess } from '@rasm/rpc';
import { Array, Cause, Context, Deferred, Effect, Fiber, Function, Layer, MutableRef, Number, Option, Path, PubSub, Queue, Ref, Result, Schema, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import { Rpc, RpcClient, type RpcClientError, RpcGroup, RpcMessage } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { Act } from '#/server/act';
import { Capture } from '#/server/capture';
import { Element } from '#/server/element';
import { Event } from '#/server/event';
import { ApplicationFailure } from '#/server/failure';
import { Installation } from '#/server/installation';
import { Instance } from '#/server/instance';

// --- [MODELS] --------------------------------------------------------------------------

const { instance } = Instance.fields;
const Launched = Schema.Union([
    Schema.TaggedStruct('launched', { instance, identifier: Schema.String, created: Schema.Boolean, replacedOnDisk: Schema.optionalKey(Schema.Boolean) }),
    Schema.TaggedStruct('state', { state: Instance.State }),
    Schema.TaggedStruct('opened', { reply: Schema.Json }),
    Instance.State.cases.failed,
]).pipe(Schema.toTaggedUnion('_tag'));
const Diagnostics = Schema.Struct({
    application: Schema.optionalKey(Schema.Struct({ url: Schema.URL, instance: Schema.optionalKey(instance) })),
    folders: Schema.Array(Schema.Struct({ url: Schema.URL, glob: Schema.optionalKey(Schema.String) })).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
    since: Schema.optionalKey(Schema.Finite.check(Schema.isGreaterThan(0))),
    limit: Schema.Int.check(Schema.isGreaterThan(0)),
    find: Schema.optionalKey(Schema.String),
});
const ApplicationRpcs = RpcGroup.make(
    Rpc.make('subscribe', { success: Event, stream: true }),
    Rpc.make('installations', { payload: { name: Schema.String }, success: Installation.Listing, error: ApplicationFailure.Thrown }),
    Rpc.make('installation', { payload: { url: Schema.URL }, success: Installation, error: ApplicationFailure.Thrown }),
    Rpc.make('pasteboard', { payload: { content: Schema.Boolean }, success: Act.Pasteboard, error: ApplicationFailure.Thrown }),
    Rpc.make('watch', { payload: { instance }, success: Instance.Watched, error: ApplicationFailure.Thrown }),
    Rpc.make('release', { payload: { instance } }),
    Rpc.make('model', {
        payload: { instance, scope: Element.Scope, known: Schema.optionalKey(Schema.Struct({ build: Schema.OptionFromOptionalKey(Schema.String), containers: Schema.Array(Element.Locator), labels: Schema.Array(Schema.String) })) },
        success: Element.Model,
        error: ApplicationFailure.Thrown,
    }),
    Rpc.make('intern', { payload: { instance, locators: Schema.Array(Element.Locator) }, success: Schema.Array(Element.Ref), error: ApplicationFailure.Thrown }),
    Rpc.make('installationsOpening', { payload: { document: Schema.URL }, success: Installation.Listing, error: ApplicationFailure.Thrown }),
    Rpc.make('launch', {
        payload: {
            url: Schema.URL,
            documents: Schema.Array(Schema.URL).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
            arguments: Schema.Array(Schema.String).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
            environment: Schema.Record(Schema.String, Schema.String).pipe(Schema.withConstructorDefault(Effect.succeed({}))),
            new: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))),
            opening: Schema.Record(Schema.String, Schema.Json).pipe(Schema.withConstructorDefault(Effect.succeed({}))),
        },
        success: Launched,
        error: ApplicationFailure.Thrown,
        stream: true,
    }),
    Rpc.make('act', { payload: { instance, steps: Schema.NonEmptyArray(Act.Step), record: Schema.optionalKey(Act.Record) }, success: Act.Outcome, error: ApplicationFailure.Thrown }),
    Rpc.make('activate', { payload: { instance }, success: Instance.Presentation, error: ApplicationFailure.Thrown }),
    Rpc.make('quit', { payload: { instance, mode: Instance.QuitMode.pipe(Schema.withConstructorDefault(Effect.succeed('asking'))) }, success: Instance.State, error: ApplicationFailure.Thrown }),
    Rpc.make('dictionary', { payload: { url: Schema.URL }, success: Installation.Dictionary, error: ApplicationFailure.Thrown }),
    Rpc.make('execute', {
        payload: { instance, arguments: Schema.Record(Schema.String, Schema.Json).pipe(Schema.withConstructorDefault(Effect.succeed({}))) },
        success: Schema.Struct({ reply: Schema.Json, written: Act.Written }),
        error: ApplicationFailure.Thrown,
    }),
    Rpc.make('capture', { payload: { instance, ...Capture.fields }, success: Capture.Frames, error: ApplicationFailure.Thrown }),
    Rpc.make('scale', {
        payload: { image: Schema.Uint8Array, longestSide: Schema.Int.check(Schema.isGreaterThan(0)) },
        success: Schema.Struct(Struct.pick(Capture.Frame.fields, ['png', 'width', 'height'])),
        error: ApplicationFailure.Thrown,
    }),
    Rpc.make('diagnostics', {
        payload: Diagnostics.fields,
        success: Schema.Struct({
            reports: Schema.Array(Instance.Report),
            samples: Schema.Array(Schema.URLFromString),
            logs: Schema.Array(Schema.Struct({ file: Schema.String, text: Schema.String })),
            unified: Act.LogEntry.pipe(Schema.Array, ApplicationFailure.read),
            unreadable: Schema.Array(ApplicationFailure.Stated),
        }),
        error: ApplicationFailure.Thrown,
    }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class ApplicationServer extends Context.Service<ApplicationServer>()('@rasm/applications/server/client/ApplicationServer', {
    make: Effect.fnUntraced(function* (client: RpcClient.FromGroup<typeof ApplicationRpcs, RpcClientError.RpcClientError>, connections: SubscriptionRef.SubscriptionRef<number>, traffic: Effect.Effect<{ readonly requests: number; readonly bytes: number }>) {
        const first = Effect.fnUntraced(function* <A, X>(subject: string, stream: Stream.Stream<A, X>) {
            const [head, rest] = yield* Stream.peel(stream, Sink.head());
            const state = yield* SubscriptionRef.make(yield* Effect.fromOption(head, () => new Cause.NoSuchElementError(`${subject} ended before its first value`)));
            return { state, following: yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(state, value))) };
        });
        const [listings, snapshots, sessions, pressures, changes, observations] = yield* Effect.all([
            Queue.unbounded<typeof Installation.Listing.Type>(),
            Queue.unbounded<typeof Event.cases.instances.Type.read>(),
            Queue.unbounded<typeof Event.cases.session.Type.read>(),
            Queue.unbounded<typeof Event.cases.pressure.Type.read>(),
            PubSub.unbounded<typeof Event.Changed.Type>(),
            PubSub.unbounded<typeof Event.cases.observed.Type>(),
        ]);
        const connected = SubscriptionRef.changes(connections).pipe(Stream.map(Number.max(1)), Stream.changes);
        const pushing = yield* connected.pipe(
            Stream.switchMap(() =>
                client.subscribe().pipe(
                    Stream.catchIf(
                        ({ reason }) => !Schema.is(Socket.SocketOpenError)(reason),
                        (error) => Stream.fromEffectDrain(Effect.logWarning('Application server connection ended, the next connection subscribes again', error)),
                    ),
                ),
            ),
            Stream.runForEach(
                Event.match({
                    installations: (listing) => Effect.asVoid(Queue.offer(listings, listing)),
                    instances: ({ read: rows }) => Effect.asVoid(Queue.offer(snapshots, rows)),
                    changed: ({ read: change }) => Result.match(change, { onSuccess: (event) => Effect.asVoid(PubSub.publish(changes, event)), onFailure: (failure) => Effect.logWarning('Application event failed', failure) }),
                    observed: (report) => Effect.asVoid(PubSub.publish(observations, report)),
                    session: ({ read: session }) => Effect.asVoid(Queue.offer(sessions, session)),
                    pressure: ({ read: pressure }) => Effect.asVoid(Queue.offer(pressures, pressure)),
                }),
            ),
            Effect.forkScoped,
        );
        const ended = Effect.andThen(Fiber.join(pushing), Effect.never);
        const [listing, instances, session, pressure] = yield* Effect.raceFirst(
            Effect.all([first('installations', Stream.fromQueue(listings)), first('instances', standing('Instance listing', Stream.fromQueue(snapshots))), latest(standing('Session reading', Stream.fromQueue(sessions))), latest(standing('Pressure reading', Stream.fromQueue(pressures)))]),
            ended,
        );
        return {
            ...client,
            launch: Effect.fnUntraced(function* <R>(payload: Parameters<typeof client.launch>[0], launching: (launched: typeof Launched.cases.launched.Type) => Effect.Effect<void, never, R>) {
                const { launched, ...outcome } = yield* client.launch(payload).pipe(
                    Stream.tap(Launched.matchOrElse({ launched: launching }, () => Effect.void)),
                    Stream.runFold(
                        () => ({
                            launched: Option.none<typeof Launched.cases.launched.Type>(),
                            state: Option.none<typeof Instance.State.Type>(),
                            opened: Option.none<Schema.Json>(),
                            failed: Array.empty<typeof Launched.cases.failed.Type>(),
                        }),
                        (folded, event) =>
                            Launched.match(event, {
                                launched: (begun) => ({ ...folded, launched: Option.some(begun) }),
                                state: ({ state }) => ({ ...folded, state: Option.some(state) }),
                                opened: ({ reply }) => ({ ...folded, opened: Option.some(reply) }),
                                failed: (failure) => ({ ...folded, failed: Array.append(folded.failed, failure) }),
                            }),
                    ),
                );
                const refusal = outcome.failed.map(({ subject, reason, values }) => ApplicationFailure.of(reason, subject, values).message);
                const begun = yield* Effect.fromOption(launched, () => new Cause.UnknownError(outcome.failed, Array.isReadonlyArrayNonEmpty(refusal) ? refusal.join('; ') : `launch of ${payload.url.href} ended before an instance launched`));
                return { ...begun, ...outcome };
            }),
            installed: listing.state,
            running: instances.state,
            changes: Stream.fromPubSub(changes),
            observations: Stream.fromPubSub(observations),
            session,
            pressure,
            connected,
            ended,
            traffic,
        };
    }),
}) {
    static readonly Diagnostics = Diagnostics;
    static readonly fromBundle = Effect.gen(function* () {
        const [path, started, program, connections, requests, bytes] = yield* Effect.all([Path.Path, Deferred.make<ApplicationServer['Service']>(), Program, SubscriptionRef.make(0), Effect.sync(() => MutableRef.make(0)), Ref.make(0)]);
        const child = Effect.map(spawn([program.executable], { stderr: 'inherit', forceKillAfter: Option.getOrUndefined(program.share) }), ({ stdin, stdout, exitCode }) => ({ stdin, exitCode, stdout: Stream.tap(stdout, (chunk) => Ref.update(bytes, Number.sum(chunk.byteLength))) }));
        const traffic = Effect.all({ requests: Effect.sync(() => MutableRef.get(requests)), bytes: Ref.get(bytes) });
        const linked = RpcClient.make(ApplicationRpcs, { generateRequestId: () => requests.pipe(MutableRef.incrementAndGet, RpcMessage.RequestId) }).pipe(Effect.flatMap((client) => ApplicationServer.make(client, connections, traffic)));
        const hooks = Layer.succeed(RpcClient.ConnectionHooks)({ onConnect: SubscriptionRef.update(connections, Number.increment), onDisconnect: Effect.void });
        return {
            layer: Layer.tap(Layer.provide(Layer.effect(ApplicationServer, linked), Layer.provide(layerProtocolChildProcess(child), hooks)), (context) => Deferred.succeed(started, Context.get(context, ApplicationServer))),
            ended: Deferred.await(started).pipe(
                Effect.flatMap(Struct.get('ended')),
                Effect.mapError((cause) => new Cause.UnknownError(cause, `${path.basename(program.executable)} ended before it answered`)),
            ),
        };
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const standing = <A, E, X>(subject: string, stream: Stream.Stream<Result.Result<A, E>, X>) =>
    stream.pipe(
        Stream.tap((value) => Result.match(value, { onSuccess: () => Effect.void, onFailure: (failure) => Effect.logWarning(`${subject} failed, the last value stands`, failure) })),
        Stream.filterMap(Function.identity),
        Stream.catchCause((cause) => Stream.fromEffect(Effect.andThen(Effect.logError(`${subject} ended`, cause), Effect.never))),
    );
const latest = <A, X>(stream: Stream.Stream<A, X>) =>
    SubscriptionRef.make(Option.none<A>()).pipe(
        Effect.tap((state) => Effect.forkScoped(Stream.runForEach(stream, (value) => SubscriptionRef.set(state, Option.some(value))))),
        Effect.map(SubscriptionRef.get),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationServer };
