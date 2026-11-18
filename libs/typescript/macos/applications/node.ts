import process from 'node:process';
import { spawn } from '@rasm/process';
import { child } from '@rasm/rpc';
import { Unmatched } from '@rasm/search';
import { Cause, Context, Effect, Fiber, FileSystem, Layer, Path, Schema, Sink, Stream, SubscriptionRef } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, RpcGroup } from 'effect/rpc';
import { parse } from 'plist';
import { Act, Acted, Capture, Diagnostics, ElementID, Event, Execute, Installation, Launch, Launching, Node, Pressure, ProcessInstance, RunningInstance, State, TermKind, Terms } from '#/contract';

// --- [MODELS] --------------------------------------------------------------------------

const Identifiers = { identifiers: Schema.NonEmptyArray(Schema.String).annotate({ description: 'Bundle identifiers of the instances monitored, each admitted by the server' }) };
const Failure = Schema.Union([Unmatched, Schema.ErrorInstance()]);
const Bundle = Schema.Struct({ CFBundleExecutable: Schema.String, CFBundleIdentifier: Schema.String, CFBundleShortVersionString: Schema.String }).annotate({ description: 'Info.plist of the app bundle holding this Node helper' });
const Contract = RpcGroup.make(
    Rpc.make('installations', {
        payload: {
            identifiers: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Bundle identifiers' })),
            prefix: Schema.optionalKey(Schema.String.annotate({ description: 'Bundle identifier prefix, joined with identifiers by or, every admitted application when both are absent' })),
        },
        success: Schema.Result(Schema.Array(Installation), Schema.ErrorInstance()).annotate({ description: 'On subscribe and each Spotlight update, or discovery error' }),
        stream: true,
    }),
    Rpc.make('instances', { payload: Identifiers, success: Schema.Array(RunningInstance).annotate({ description: 'On subscribe and each change, exited instances kept until next change' }), stream: true }),
    Rpc.make('events', { payload: Identifiers, success: Event, stream: true }),
    Rpc.make('pressure', { success: Schema.Result(Pressure, Schema.ErrorInstance()).annotate({ description: 'On subscribe and each kernel change, or read error' }), stream: true }),
    Rpc.make('nodes', {
        payload: {
            instance: ProcessInstance,
            root: Schema.optionalKey(ElementID.annotate({ description: 'Element read, the application when absent' })),
            depth: Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Child levels read below the root' }),
        },
        success: Node,
        error: Failure,
    }),
    Rpc.make('launch', { payload: Launch.fields, success: Launching, error: Failure, stream: true }),
    Rpc.make('act', { payload: { instance: ProcessInstance, ...Act.fields }, success: Acted, error: Failure }),
    Rpc.make('quit', { payload: { instance: ProcessInstance, force: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))) }, success: State.annotate({ description: 'Last state, a dialog or unresponsive instance stays running' }), error: Failure }),
    Rpc.make('terms', { payload: { url: Schema.URL }, success: Terms, error: Failure }),
    Rpc.make('definition', { payload: { url: Schema.URL, kind: TermKind, name: Schema.String }, success: Schema.String, error: Failure }),
    Rpc.make('execute', { payload: { instance: ProcessInstance, ...Execute.fields }, success: Execute.Result, error: Failure }),
    Rpc.make('capture', { payload: { instance: ProcessInstance, ...Capture.fields }, success: Capture.Result, error: Failure }),
    Rpc.make('diagnostics', { payload: Diagnostics.fields, success: Diagnostics.Result, error: Failure }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class ApplicationServer extends Context.Service<ApplicationServer>()('@rasm/applications/node/ApplicationServer', {
    make: Effect.fnUntraced(function* (handle: ChildProcessSpawner.ChildProcessHandle) {
        const client = yield* RpcClient.make(Contract);
        const watched = Effect.fnUntraced(function* <A, E>(subject: string, changes: Stream.Stream<A, E>) {
            const [first, rest] = yield* Stream.peel(changes, Sink.head());
            const latest = yield* SubscriptionRef.make(yield* Effect.fromOption(first, () => new Cause.NoSuchElementError(`${subject} ended before its first value`)));
            return [latest, yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(latest, value)))] as const;
        });
        const [installations, discovering] = yield* watched('installations', client.installations({}));
        return {
            ...client,
            installations,
            installed: Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult),
            exited: Effect.raceFirst(handle.exitCode, Effect.andThen(Fiber.join(discovering), handle.exitCode)),
        };
    }),
}) {
    static readonly layer = <E, R>(spawned: Effect.Effect<ChildProcessSpawner.ChildProcessHandle, E, R>) => Layer.unwrap(Effect.map(spawned, (handle) => Layer.effect(ApplicationServer, ApplicationServer.make(handle)).pipe(Layer.provideMerge(child(handle)))));
    static readonly bundled = Effect.gen(function* () {
        const [path, files] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
        const contents = path.join(path.dirname(process.execPath), '..');
        const { CFBundleExecutable, CFBundleIdentifier, CFBundleShortVersionString } = yield* files.readFileString(path.join(contents, 'Info.plist')).pipe(
            Effect.flatMap((text) => Effect.try(() => parse(text))),
            Effect.flatMap(Schema.decodeUnknownEffect(Bundle)),
        );
        return { version: CFBundleShortVersionString, identifier: CFBundleIdentifier, layer: ApplicationServer.layer(spawn([path.join(contents, 'MacOS', CFBundleExecutable)], { stderr: 'inherit' })) };
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationServer };
