import process from 'node:process';
import { spawn } from '@rasm/process';
import { child } from '@rasm/rpc';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Effect, Fiber, FileSystem, Function, Layer, Option, Path, Predicate, Record, Schema, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, RpcGroup } from 'effect/rpc';
import { parse } from 'plist';
import { Capture, Execute, Installation, Launch, Launching, Perform, Pressure, ProcessInstance, RunningInstance, State, TermKind, Terms, WindowID } from '#/contract';

// --- [MODELS] --------------------------------------------------------------------------

const Selection = {
    identifiers: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Bundle identifiers' })),
    prefix: Schema.optionalKey(Schema.String.annotate({ description: 'Bundle identifier prefix, joined with identifiers by or, every admitted application when both are absent' })),
};
const Failure = Schema.Union([Unmatched, Schema.ErrorInstance()]);
const Bundle = Schema.Struct({ CFBundleExecutable: Schema.String, CFBundleShortVersionString: Schema.String }).annotate({ description: 'Info.plist of the app bundle holding this Node helper' });
const Contract = RpcGroup.make(
    Rpc.make('installations', { payload: Selection, success: Schema.Result(Schema.Array(Installation), Schema.ErrorInstance()).annotate({ description: 'On subscribe and each Spotlight update, or discovery error' }), stream: true }),
    Rpc.make('instances', { payload: Selection, success: Schema.Array(RunningInstance).annotate({ description: 'On subscribe and each change, exited instances kept until next change' }), stream: true }),
    Rpc.make('pressure', { success: Schema.Result(Pressure, Schema.ErrorInstance()).annotate({ description: 'On subscribe and each kernel change, or read error' }), stream: true }),
    Rpc.make('applications', { payload: { toOpen: Schema.URL }, success: Schema.Array(Schema.URL).annotate({ description: 'Launch Services applications for file, default first' }) }),
    Rpc.make('read', { payload: { instance: ProcessInstance, window: Schema.optionalKey(WindowID.annotate({ description: 'Walks that row alone, absent walks every element' })) }, success: State, error: Failure }),
    Rpc.make('launch', { payload: Launch.fields, success: Launching, error: Failure, stream: true }),
    Rpc.make('perform', { payload: { instance: ProcessInstance, ...Perform.fields }, success: State.annotate({ description: 'State after the last action' }), error: Failure }),
    Rpc.make('quit', { payload: { instance: ProcessInstance, force: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))) }, success: State.annotate({ description: 'Last state, a dialog or unresponsive instance stays running' }), error: Failure }),
    Rpc.make('terms', { payload: { url: Schema.URL }, success: Terms, error: Failure }),
    Rpc.make('definition', { payload: { url: Schema.URL, kind: TermKind, name: Schema.String }, success: Schema.String, error: Failure }),
    Rpc.make('execute', { payload: { instance: ProcessInstance, ...Execute.fields }, success: Execute.Result, error: Failure }),
    Rpc.make('capture', { payload: { instance: ProcessInstance, ...Capture.fields, directory: Schema.URL.annotate({ description: 'Directory receiving <number>.png' }) }, success: Capture.Result, error: Failure }),
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
        const [[installations, discovering], [instances, tracking]] = yield* Effect.all([watched('installations', client.installations({})), watched('instances', client.instances({}))]);
        const installed = Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult);
        return {
            ...client,
            installations,
            instances,
            installed,
            exited: Effect.raceFirst(handle.exitCode, Effect.andThen(Fiber.joinAll([discovering, tracking]), handle.exitCode)),
            select: (holds: Predicate.Predicate<Installation>, kind: string) =>
                Effect.fnUntraced(function* ({ application, instance }: { readonly application: Option.Option<string>; readonly instance: Option.Option<typeof ProcessInstance.Type> }) {
                    const [rows, running] = yield* Effect.all([installed, SubscriptionRef.get(instances)]);
                    const [located, selects] = Option.match(instance, {
                        onNone: () => [`${kind} running in Dock`, (row: RunningInstance) => row.activationPolicy === 'regular'] as const,
                        onSome: (named) => [`${kind} running instance ${named}`, (row: RunningInstance) => row.instance === named] as const,
                    });
                    const selecting = running.filter(selects);
                    const [subject, runs] = Option.isSome(application) ? ([`installed ${kind}`, Function.constTrue] as const) : ([located, (row: Installation) => selecting.some(Installation.runs(row))] as const);
                    const installation = yield* Unmatched.single(rows.filter(Predicate.and(holds, runs)), subject, application, ({ name }) => [name]);
                    const chosen = yield* Unmatched.single(running.filter(Installation.runs(installation)), `running instance of ${installation.name}`, instance, (row) => [row.instance]);
                    return { installation, ...Struct.pick(chosen, ['instance', 'pid']) };
                }),
            opening: (toOpen: URL) =>
                Effect.map(Effect.all([client.applications({ toOpen }), installed]), ([urls, rows]) => {
                    const located = Record.fromIterableBy(rows, (row) => row.url.href);
                    return Array.getSomes(Array.map(urls, (url) => Record.get(located, url.href)));
                }),
            script: Effect.fn('ApplicationServer.script')(function* ({ instance, url, ...script }: Pick<RunningInstance, 'instance' | 'url'> & Parameters<typeof Terms.script>[1]) {
                const execute = yield* Terms.script(yield* client.terms({ url }), script);
                return yield* client.execute(Struct.assign(execute, { instance }));
            }),
        };
    }),
}) {
    static readonly layer = <E, R>(spawned: Effect.Effect<ChildProcessSpawner.ChildProcessHandle, E, R>) => Layer.unwrap(Effect.map(spawned, (handle) => Layer.effect(ApplicationServer, ApplicationServer.make(handle)).pipe(Layer.provideMerge(child(handle)))));
    static readonly bundled = Effect.gen(function* () {
        const [path, files] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
        const contents = path.join(path.dirname(process.execPath), '..');
        const { CFBundleExecutable, CFBundleShortVersionString } = yield* files.readFileString(path.join(contents, 'Info.plist')).pipe(
            Effect.flatMap((text) => Effect.try(() => parse(text))),
            Effect.flatMap(Schema.decodeUnknownEffect(Bundle)),
        );
        return { version: CFBundleShortVersionString, layer: ApplicationServer.layer(spawn([path.join(contents, 'MacOS', CFBundleExecutable)], { stderr: 'inherit' })) };
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationServer };
